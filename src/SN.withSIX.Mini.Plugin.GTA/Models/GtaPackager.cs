// <copyright company="SIX Networks GmbH" file="GtaPackager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using NDepend.Path;
using RageLib.GTA5.Cryptography;
using RpfGeneratorTool;
using SN.withSIX.Core;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Mini.Plugin.GTA.Models
{
    public class GtaPackager
    {
        readonly IAbsoluteDirectoryPath _gameDir;

        public GtaPackager(IAbsoluteDirectoryPath gameDir) {
            _gameDir = gameDir;
        }

        public Task HandlePackages() {
            return Task.Factory.StartNew(HandlePackagesImpl);
        }

        void HandlePackagesImpl() {
            Tools.FileUtil.Ops.CreateDirectoryAndSetACLWithFallbackAndRetry(_gameDir);
            var syncPackagesPath = _gameDir.GetChildDirectoryWithName("syncpackages");
            if (!syncPackagesPath.Exists)
                return;

            var packages = syncPackagesPath.DirectoryInfo.EnumerateDirectories()
                .Where(x => File.Exists(Path.Combine(x.FullName, "package.config"))).ToArray();
            if (!packages.Any())
                return;

            var tempPath = Path.GetTempPath().ToAbsoluteDirectoryPath().GetChildDirectoryWithName("RpfGenerator");
            if (!tempPath.Exists)
                Directory.CreateDirectory(tempPath.ToString());

            var consts = new Uri("https://dl.dropboxusercontent.com/u/18252370/GTA/gta5_const.dat.gz");
            using (var wc = new WebClient()) {
                var data = wc.DownloadData(consts);
                var decompressed = Decompress(data);
                using (var ms = new MemoryStream(decompressed))
                    LoadConsts(ms);
            }

            var packager = new Packager(_gameDir,
                _gameDir.GetChildDirectoryWithName("mods"),
                tempPath,
                new Packager.PackagerConfig {
                    TreatImportsAsInserts = true,
                    BuilderConfig = new RpfListBuilder.RpfListBuilderConfig {AudioPathsOnly = true}
                });

            foreach (var p in packages)
                packager.PackageMod(p.FullName.ToAbsoluteDirectoryPath());
        }

        static void LoadConsts(Stream fs) {
            var bf = new BinaryFormatter();

            GTA5Constants.PC_AES_KEY = (byte[]) bf.Deserialize(fs);
            GTA5Constants.PC_NG_KEYS = (byte[][]) bf.Deserialize(fs);
            GTA5Constants.PC_NG_DECRYPT_TABLES = (byte[][]) bf.Deserialize(fs);
            GTA5Constants.PC_LUT = (byte[]) bf.Deserialize(fs);
        }

        static byte[] Decompress(byte[] gzip) {
            using (var stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress)) {
                const int size = 4096;
                var buffer = new byte[size];
                using (var memory = new MemoryStream()) {
                    var count = 0;
                    do {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                            memory.Write(buffer, 0, count);
                    } while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}