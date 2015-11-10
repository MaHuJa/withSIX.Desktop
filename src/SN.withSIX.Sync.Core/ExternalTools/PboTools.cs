// <copyright company="SIX Networks GmbH" file="PboTools.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Services;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Sync.Core.ExternalTools
{
    public static class MikeroParameters
    {
        public const string NoPause = "P";
    }

    public static class MakePboParameters
    {
        public const string Binarize = "B";
        public const string DoNothing = "N";
        public const string AllowNoPrefix = "$";
        public const string AllowUnbinarizedP3D = "U";
        public const string IgnoreMissingConfigCppOrMissionSqm = "J";
    }

    public static class ExtractPboParameters
    {
        public const string IgnorePrefix = "K";
        public const string DontDeRapify = "R";
    }

    public class PboTools : IDomainService
    {
        readonly IAbsoluteFilePath _dsCreateKeyBin =
            Common.Paths.ToolPath.GetChildDirectoryWithName("bitools").GetChildFileWithName("dsCreateKey.exe");
        readonly IAbsoluteFilePath _dsSignFileBin =
            Common.Paths.ToolPath.GetChildDirectoryWithName("bitools").GetChildFileWithName("dsSignFile.exe");
        readonly IAbsoluteFilePath _extractPboBin =
            Common.Paths.ToolPath.GetChildDirectoryWithName("pbodll").GetChildFileWithName("ExtractPbo.exe");
        readonly Tools.FileTools.IFileOps _fileOps;
        readonly IAbsoluteFilePath _makePboBin =
            Common.Paths.ToolPath.GetChildDirectoryWithName("pbodll").GetChildFileWithName("MakePbo.exe");
        readonly IProcessManager _processManager;

        public PboTools(IProcessManager procsesManager, Tools.FileTools.IFileOps fileOps) {
            _processManager = procsesManager;
            _fileOps = fileOps;
        }

        public void UnpackPbo(IAbsoluteFilePath input, IAbsoluteDirectoryPath output) {
            RunExtractPboWithParameters(input, output, MikeroParameters.NoPause);
        }

        public void RunExtractPboWithParameters(IAbsoluteFilePath input, IAbsoluteDirectoryPath output,
            params string[] parameters) {
            if (!input.Exists)
                throw new IOException("File doesn't exist: " + input);
            var startInfo =
                new ProcessStartInfoBuilder(_extractPboBin,
                    BuildParameters(input.ToString(), output.ToString(), parameters)) {
                        WorkingDirectory = Common.Paths.AppPath.ToString()
                    }.Build();

            ProcessExitResult(_processManager.LaunchAndGrabTool(startInfo));
        }

        public void CreatePbo(IAbsoluteDirectoryPath input, IAbsoluteDirectoryPath output, bool overwrite = false) {
            ConfirmPackValidity(input, output, overwrite);
            RunMakePboWithParameters(input, output, MikeroParameters.NoPause);
        }

        public void CreateMissionPbo(IAbsoluteDirectoryPath input, IAbsoluteDirectoryPath output, bool overwrite = false) {
            ConfirmPackValidity(input, output, overwrite);
            RunMakePboWithParameters(input, output, MikeroParameters.NoPause, MakePboParameters.DoNothing,
                MakePboParameters.AllowNoPrefix, MakePboParameters.AllowUnbinarizedP3D);
        }

        public void CreateBinarizedPbo(IAbsoluteDirectoryPath input, IAbsoluteDirectoryPath output,
            bool overwrite = false) {
            ConfirmPackValidity(input, output, overwrite);
            RunMakePboWithParameters(input, output, MikeroParameters.NoPause, MakePboParameters.Binarize);
        }

        public void RepackPbo(IAbsoluteFilePath pboFile) {
            var unpackedPboFolder = pboFile.GetBrotherDirectoryWithName(pboFile.FileNameWithoutExtension);

            using (new TmpDirectory(unpackedPboFolder)) {
                UnpackPbo(pboFile);
                CreatePboPrefixFileIfNeeded(unpackedPboFolder);
                PackFolder(unpackedPboFolder);
            }
        }

        void CreatePboPrefixFileIfNeeded(IAbsoluteDirectoryPath unpackedPboFolder) {
            CreatePboPrefixFileIfNeeded(unpackedPboFolder.GetChildFileWithName("$PBOPREFIX$.txt"));
        }

        void CreatePboPrefixFileIfNeeded(IAbsoluteFilePath pboPrefixFile) {
            if (!pboPrefixFile.Exists)
                CreatePboPrefixFile(pboPrefixFile, pboPrefixFile.ParentDirectoryPath.DirectoryName);
        }

        void CreatePboPrefixFile(IAbsoluteFilePath pboPrefixFile, string prefix) {
            _fileOps.CreateText(pboPrefixFile, prefix);
        }

        void UnpackPbo(IAbsoluteFilePath pbo) {
            RunExtractPboWithParameters(pbo, pbo.ParentDirectoryPath, ExtractPboParameters.IgnorePrefix,
                ExtractPboParameters.DontDeRapify, MikeroParameters.NoPause);
        }

        void PackFolder(IAbsoluteDirectoryPath folder) {
            RunMakePboWithParameters(folder, folder.ParentDirectoryPath,
                MakePboParameters.AllowUnbinarizedP3D,
                MakePboParameters.IgnoreMissingConfigCppOrMissionSqm, MakePboParameters.DoNothing,
                MikeroParameters.NoPause);
        }

        public void CreateKey(IAbsoluteFilePath outFile, bool overwrite = false) {
            Contract.Requires<ArgumentNullException>(outFile != null);

            var privateFile = outFile + ".biprivatekey";
            var publicFile = outFile + ".bikey";

            if (!overwrite) {
                if (File.Exists(privateFile))
                    throw new IOException("File exists: " + privateFile);
                if (File.Exists(publicFile))
                    throw new IOException("File exists: " + publicFile);
            }
            var startInfo = new ProcessStartInfoBuilder(_dsCreateKeyBin, BuildPathParameters(outFile.FileName)) {
                WorkingDirectory = outFile.ParentDirectoryPath.ToString()
            }.Build();

            ProcessExitResult(_processManager.LaunchAndGrabTool(startInfo));
        }

        public void SignFile(IAbsoluteFilePath file, IAbsoluteFilePath privateFile) {
            Contract.Requires<ArgumentNullException>(file != null);

            if (!file.Exists)
                throw new IOException("File doesn't exist: " + file);
            if (!privateFile.Exists)
                throw new IOException("File doesn't exist: " + privateFile);

            var startInfo =
                new ProcessStartInfoBuilder(_dsSignFileBin, BuildPathParameters(privateFile.ToString(), file.ToString())) {
                    WorkingDirectory = Common.Paths.StartPath.ToString()
                }.Build();
            ProcessExitResult(_processManager.LaunchAndGrabTool(startInfo));
        }

        public void RunMakePboWithParameters(IAbsoluteDirectoryPath input, IAbsoluteDirectoryPath output,
            params string[] parameters) {
            RunMakePbo(BuildParameters(input.ToString(), output.ToString(), parameters));
        }

        void RunMakePbo(string parameters) {
            var startInfo = new ProcessStartInfoBuilder(_makePboBin, parameters) {
                WorkingDirectory = Common.Paths.AppPath.ToString()
            }.Build();
            ProcessExitResult(_processManager.LaunchAndGrabTool(startInfo));
        }

        static void ProcessExitResult(ProcessExitResultWithOutput exitResult) {
            if (exitResult.ExitCode != 0)
                throw exitResult.GenerateException();
        }

        static string BuildParameters(string input, string output, params string[] parameters) {
            var mikeroParameters = BuildMikeroParameters(parameters);
            return mikeroParameters == null
                ? BuildPathParameters(input, output)
                : CombineParameters(mikeroParameters, BuildPathParameters(input, output));
        }

        static string BuildPathParameters(params string[] paths) {
            return string.Join(" ", paths.Select(EscapePath));
        }

        static string EscapePath(string x) {
            return "\"" + x + "\"";
        }

        static string CombineParameters(string combinedParameters, string pathParameters) {
            return combinedParameters + " " + pathParameters;
        }

        static string BuildMikeroParameters(params string[] parameters) {
            var joinParameters = parameters.Where(x => !x.StartsWith("-")).ToArray();
            return parameters.Any()
                ? GetAllParameters(joinParameters, parameters.Except(joinParameters).ToArray())
                : null;
        }

        static string GetAllParameters(string[] joinParameters, IEnumerable<string> separateParameters) {
            return string.Join(" ",
                new[] {GetJoinedParameters(joinParameters)}.Concat(separateParameters));
        }

        static string GetJoinedParameters(params string[] joinParameters) {
            return string.Format("-{0}", string.Join("", joinParameters));
        }

        static void ConfirmPackValidity(IAbsoluteDirectoryPath input, IAbsoluteFilePath output, bool overwrite) {
            if (!input.Exists)
                throw new IOException("Directory doesn't exist: " + input);
            if (!overwrite && output.Exists)
                throw new IOException("File exists: " + output);
        }

        static void ConfirmPackValidity(IAbsoluteDirectoryPath input, IAbsoluteDirectoryPath output, bool overwrite,
            string extension = ".pbo") {
            ConfirmPackValidity(input, output.GetChildFileWithName(input.DirectoryName + extension), overwrite);
        }
    }
}