// <copyright company="SIX Networks GmbH" file="GamespyMasterQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using SN.withSIX.Api.Models;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public class GamespyMasterQuery
    {
        readonly string _gsListPath = Path.Combine(Common.Paths.ToolPath.ToString(), "gslist.exe");
        readonly string _serverBrowserTag;

        public GamespyMasterQuery(string serverBrowserTag) {
            _serverBrowserTag = serverBrowserTag;
        }

        public IEnumerable<IDictionary<string, string>> RetrieveServers(string mod = null) {
            ExecuteGsList("-u");
            return RetrieveGamespyServers(mod);
        }

        IEnumerable<IDictionary<string, string>> RetrieveGamespyServers(string mod) {
            return ExecuteGsList(FormatModParameters(mod))
                .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => ParseLine(mod, line))
                .Where(x => x != null).ToArray();
        }

        string FormatModParameters(string mod) {
            return mod == null
                ? string.Format("-n {0} -f \"dedicated = 1\" -X \\hostname",
                    _serverBrowserTag)
                : String.Format("-n {1} -f \"dedicated = 1 AND mod LIKE '%{0}%'\" -X \\hostname", mod,
                    _serverBrowserTag);
        }

        string ExecuteGsList(string arguments) {
            Contract.Requires<ArgumentNullException>(arguments != null);
            string output;
            using (var p = CreateGSProcess(arguments)) {
                p.Start();
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }

            return output;
        }

        IDictionary<string, string> ParseLine(string mod, string line) {
            var indexOfFirstSpace = line.IndexOf(' ');
            if (indexOfFirstSpace > -1)
                return null;
            return CreateServerDictionary(mod, new ServerAddress(line.Substring(0, indexOfFirstSpace)),
                line.Substring(indexOfFirstSpace + 11));
        }

        IDictionary<string, string> CreateServerDictionary(string mod, ServerAddress address, string hostName) {
            return mod == null
                ? new Dictionary<string, string> {
                    {"address", address.ToString()},
                    {"hostname", hostName},
                    {"gamename", _serverBrowserTag}
                }
                : new Dictionary<string, string> {
                    {"address", address.ToString()},
                    {"hostname", hostName},
                    {"mod", mod},
                    {"gamename", _serverBrowserTag}
                };
        }

        Process CreateGSProcess(string arguments) {
            return new Process {
                StartInfo = {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    FileName = _gsListPath,
                    Arguments = arguments
                }
            };
        }
    }
}