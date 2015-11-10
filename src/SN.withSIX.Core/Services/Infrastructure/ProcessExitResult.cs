// <copyright company="SIX Networks GmbH" file="ProcessExitResult.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Diagnostics;

namespace SN.withSIX.Core.Services.Infrastructure
{
    public class ProcessExitResult
    {
        public ProcessExitResult(int exitCode, int id, ProcessStartInfo startInfo) {
            ExitCode = exitCode;
            Id = id;
            StartInfo = startInfo;
        }

        public int ExitCode { get; }
        public int Id { get; }
        public ProcessStartInfo StartInfo { get; }
    }

    public class ProcessExitResultWithOutput : ProcessExitResult
    {
        public ProcessExitResultWithOutput(int exitCode, int id, ProcessStartInfo startInfo, string standardOutput,
            string standardError) : base(exitCode, id, startInfo) {
            StandardOutput = standardOutput;
            StandardError = standardError;
        }

        public string StandardOutput { get; }
        public string StandardError { get; }

        public static ProcessExitResultWithOutput FromProcessExitResult(ProcessExitResult result, string output,
            string error = null) {
            return new ProcessExitResultWithOutput(result.ExitCode, result.Id, result.StartInfo, output, error);
        }
    }
}