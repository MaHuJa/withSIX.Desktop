// <copyright company="SIX Networks GmbH" file="AwesomiumInstaller.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.Win32;
using SmartAssembly.Attributes;

namespace SN.withSIX.Core.Presentation.Assemblies
{
    public class AwesomiumInstaller
    {
        readonly Uri _src;
        readonly string _tmpFile;
        readonly string _tmpLocation;

        public AwesomiumInstaller(string tmpLocation, Uri src) {
            _tmpLocation = tmpLocation;
            _tmpFile = Path.Combine(tmpLocation, "awesomiumsetup.exe");
            _src = src;
        }

        public bool IsInstalled() {
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                .OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{5BCB064B-9F65-4E15-BAFB-669E72E54FD9}") !=
                   null;
        }

        public void Install() {
            using (new TmpFile(_tmpFile)) {
                Download();
                RunInstaller();
            }
        }

        void RunInstaller() {
            using (var p = Process.Start(_tmpFile,
                "/s /v/qb")) {
                //"/debugLog\"" + LogLocation + "\" /s /v\"/qb /l*v \\\"" + LogLocation + "\\\"\"")) {
                //  /vREINSTALL=ALL /vREINSTALLMODE=vomus /v/qb
                p.WaitForExit();
            }
            if (!IsInstalled())
                throw new InstallationFailed();
        }

        void Download() {
            Directory.CreateDirectory(_tmpLocation);
            using (var wc = new WebClient())
                wc.DownloadFile(_src, _tmpFile);
        }
    }

    [DoNotObfuscate]
    public class InstallationFailed : Exception {}

    class TmpFile : IDisposable
    {
        readonly string _file;

        public TmpFile(string file) {
            _file = file;
        }

        public void Dispose() {
            if (File.Exists(_file))
                File.Delete(_file);
        }
    }

    class TmpFileCreated : IDisposable
    {
        public TmpFileCreated() {
            FilePath = Path.GetTempFileName();
        }

        public string FilePath { get; }

        public void Dispose() {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }

    class TmpDirectory : IDisposable
    {
        readonly string _path;

        public TmpDirectory(string path) {
            Directory.CreateDirectory(path);
            _path = path;
        }

        public void Dispose() {
            if (Directory.Exists(_path))
                Directory.Delete(_path, true);
        }
    }
}