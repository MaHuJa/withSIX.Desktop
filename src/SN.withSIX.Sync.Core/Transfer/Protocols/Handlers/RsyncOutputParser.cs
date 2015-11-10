// <copyright company="SIX Networks GmbH" file="RsyncOutputParser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Sync.Core.Transfer.Protocols.Handlers
{
    public class RsyncOutputParser : OutputParser
    {
        static readonly Regex RSYNC_START =
            new Regex(@"([0-9\.]+)%[\s\t]+([0-9\.]+)([a-z\/]+)\/s[\s\t]+([0-9\.:]+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        static readonly Regex RSYNC_END =
            new Regex(@"sent ([0-9\.]*)([KMG]?) bytes  received ([0-9\.]*)([KMG]?) bytes",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override void ParseOutput(Process sender, string data, ITransferProgress progress) {
            //        3.51M  43%  177.98kB/s    0:00:25
            // sent 1.39K bytes  received 1.01K bytes  1.60K bytes/sec
            // total size is 114.55K  speedup is 47.57
            if (data == null)
                return;
            progress.UpdateOutput(data);

            var matches = RSYNC_START.Matches(data);
            if (matches.Count > 0) {
                var match = matches[0];

                progress.Progress = match.Groups[1].Value.TryDouble();

                var speed = match.Groups[2].Value.TryDouble();
                var speedUnit = match.Groups[3].Value;
                progress.Speed = GetByteSize(speed, speedUnit);

                TimeSpan ts;
                if (TimeSpan.TryParse(match.Groups[4].Value, out ts))
                    progress.Eta = ts;

                return;
            }

            matches = RSYNC_END.Matches(data);
            if (matches.Count > 0) {
                var match = matches[0];
                var sent = match.Groups[1].Value.TryDouble();
                var sentUnit = match.Groups[2].Value;

                var received = match.Groups[3].Value.TryDouble();
                var receivedUnit = match.Groups[4].Value;

                // Rsync final message omits B(yte) indication
                progress.FileSizeTransfered = GetByteSize(sent, sentUnit + "b") +
                                              GetByteSize(received, receivedUnit + "b");
                progress.Completed = true;
            }
        }
    }
}