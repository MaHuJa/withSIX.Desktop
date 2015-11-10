// <copyright company="SIX Networks GmbH" file="ZsyncOutputParser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Sync.Core.Transfer.Protocols.Handlers
{
    public class ZsyncOutputParser : OutputParser
    {
        static readonly string StrZsyncIncompatible =
            "zsync received a data response (code 200) but this is not a partial content response";
        static readonly Regex ZsyncLoop = new Regex("^downloading from ",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex ZSYNC_START =
            new Regex(@"^[#\-]+ ([0-9\.]+)% ([0-9\.]+) ([a-z]+)ps ([\w:]+) ETA",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex ZSYNC_END = new Regex(@"^used ([0-9]*) local, fetched ([0-9]*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex tspan = new Regex("^[0-9]+:[0-9]+$", RegexOptions.Compiled);

        public bool VerifyZsyncCompatible(string data) {
            return !data.Contains(StrZsyncIncompatible);
        }

        public override void ParseOutput(Process sender, string data, ITransferProgress progress) {
            // ###################- 97.3% 141.5 kBps 0:00 ETA  
            // used 0 local, fetched 894
            if (data == null)
                return;
            progress.UpdateOutput(data);

            if (!VerifyZsyncCompatible(data))
                progress.ZsyncIncompatible = true;

            if (CheckZsyncLoop(sender, data, progress))
                return;

            var matches = ZSYNC_START.Matches(data);
            if (matches.Count > 0) {
                var match = matches[0];
                progress.Progress = match.Groups[1].Value.TryDouble();

                var speed = match.Groups[2].Value.TryDouble();
                var speedUnit = match.Groups[3].Value;
                progress.Speed = GetByteSize(speed, speedUnit);

                var eta = match.Groups[4].Value;
                if (tspan.IsMatch(eta))
                    eta = "0:" + eta;
                TimeSpan ts;
                if (TimeSpan.TryParse(eta, out ts))
                    progress.Eta = ts;

                return;
            }

            matches = ZSYNC_END.Matches(data);
            if (matches.Count > 0) {
                var match = matches[0];
                progress.FileSizeTransfered = match.Groups[2].Value.TryInt();
                progress.Completed = true;
            }
        }

        public bool CheckZsyncLoop(Process process, string data, ITransferProgress progress) {
            // By not resetting the loopdata/counts we set ourselves up for a situation where it's possible to detect loops that span multiple lines..
            if (!ZsyncLoop.IsMatch(data))
                return false;

            if (progress.ZsyncLoopData == null) {
                progress.ZsyncLoopData = data;
                progress.ZsyncLoopCount = 0;
                return false;
            }

            if (progress.ZsyncLoopData.Equals(data, StringComparison.OrdinalIgnoreCase)) {
                var loopCount = ++progress.ZsyncLoopCount;
                if (loopCount < 2)
                    return false;
                if (!process.SafeHasExited())
                    TryKillProcess(process);
                return true;
            }

            progress.ZsyncLoopData = data;
            progress.ZsyncLoopCount = 0;

            return false;
        }

        static void TryKillProcess(Process process) {
            try {
                process.Kill();
            } catch (Win32Exception e) {
                MainLog.Logger.FormattedWarnException(e, "Error while trying to kill zsync loop process");
            } catch (InvalidOperationException e) {
                MainLog.Logger.FormattedWarnException(e, "Error while trying to kill zsync loop process");
            }
        }
    }
}