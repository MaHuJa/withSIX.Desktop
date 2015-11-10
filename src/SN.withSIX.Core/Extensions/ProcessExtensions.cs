// <copyright company="SIX Networks GmbH" file="ProcessExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics;
using NDepend.Path;

namespace SN.withSIX.Core.Extensions
{
    public static class ProcessValidationExtensions
    {
        public static void Validate(this ProcessStartInfo startInfo) {
            if ((startInfo.Verb == "runas")
                && !startInfo.UseShellExecute)
                throw new NotSupportedException("Cannot use verb: runas, when shellExecute is disabled");

            if ((startInfo.RedirectStandardError || startInfo.RedirectStandardInput || startInfo.RedirectStandardOutput)
                && startInfo.UseShellExecute)
                throw new NotSupportedException("Cannot use redirect, when shellExecute is enabled");
        }

        public static void Validate(this Process process) {
            process.StartInfo.Validate();
        }
    }

    public static class ProcessExtensions
    {
        public static IAbsoluteDirectoryPath DefaultWorkingDirectory { get; set; }

        public static ProcessStartInfo SetWorkingDirectoryOrDefault(this ProcessStartInfo arg, string workingDirectory) {
            arg.WorkingDirectory = workingDirectory ?? DefaultWorkingDirectory.ToString();
            return arg;
        }

        public static ProcessStartInfo EnableRedirect(this ProcessStartInfo startInfo) {
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.CreateNoWindow = true;

            return startInfo;
        }

        public static bool SafeHasExited(this Process process) {
            try {
                return process.HasExited;
            } catch (Win32Exception) {
                return false;
            }
        }

        public static string Format(this ProcessStartInfo startInfo) {
            return string.Format("{0}, from: {1}, with: {2}, verb: {3}", startInfo.FileName,
                startInfo.WorkingDirectory, startInfo.Arguments, startInfo.Verb);
        }

        public static ProcessStartInfo EnableRunAsAdministrator(this ProcessStartInfo arg) {
            if (Environment.OSVersion.Version.Major >= 6)
                arg.Verb = "runas";

            return arg;
        }
    }
}