// <copyright company="SIX Networks GmbH" file="ProcessStartInfoBuilder.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using NDepend.Path;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Core.Services.Infrastructure
{
    public class ProcessStartInfoBuilder
    {
        public ProcessStartInfoBuilder() {}

        public ProcessStartInfoBuilder(string exePath) : this() {
            ExePath = exePath;
        }

        public ProcessStartInfoBuilder(string exePath, string arguments) : this(exePath) {
            Arguments = arguments;
        }

        public ProcessStartInfoBuilder(IAbsoluteFilePath exePath)
            : this() {
            ExePath = exePath.ToString();
        }

        public ProcessStartInfoBuilder(IAbsoluteFilePath exePath, string arguments)
            : this(exePath) {
            Arguments = arguments;
        }

        public string WorkingDirectory { get; set; }
        public bool AsAdministrator { get; set; }
        public bool Redirect { get; set; }
        public string ExePath { get; set; }
        public string Arguments { get; set; }

        public ProcessStartInfo Build() {
            var info = new ProcessStartInfo(ExePath, Arguments)
                .SetWorkingDirectoryOrDefault(WorkingDirectory);

            if (AsAdministrator)
                info = info.EnableRunAsAdministrator();

            if (Redirect)
                info = info.EnableRedirect();

            return info;
        }
    }

    public static class ProcessExtensions
    {
        public static ProcessException GenerateException(this ProcessExitResult exitResult) {
            return new ProcessException(GenerateMessage(exitResult));
        }

        static string GenerateMessage(ProcessExitResult exitResult) {
            return "The process with ID: " + exitResult.Id + ", did not exit gracefully, CODE: " + exitResult.ExitCode;
        }

        public static ProcessException GenerateException(this ProcessExitResultWithOutput exitResult) {
            return
                new ProcessException(GenerateMessage(exitResult) + "\nOutput: " + exitResult.StandardOutput +
                                     "\nError: " + exitResult.StandardError);
        }
    }

    public class ProcessException : Exception
    {
        public ProcessException(string message) : base(message) {}
    }
}