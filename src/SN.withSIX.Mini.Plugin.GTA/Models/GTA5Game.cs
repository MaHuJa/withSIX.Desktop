// <copyright company="SIX Networks GmbH" file="GTA5Game.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Attributes;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller.Attributes;

namespace SN.withSIX.Mini.Plugin.GTA.Models
{
    [Game(GameUUids.GTAV, Name = "GTA 5", Slug = "GTA-5",
        Executables = new[] {"PlayGTAV.exe", "GTAVLauncher.exe"},
        LaunchTypes = new[] {LaunchType.Singleplayer, LaunchType.Multiplayer},
        IsPublic = true,
        FirstTimeRunInfo =
            @"Welcome to Sync withSIX

Some important information before you get started:

In order to ensure a save and working mod experience, For now Sync will automatically backup and delete all pre installed modifications from your GTA5 folder, when launching of the game."
        )]
    [SteamInfo(271590, "Grand Theft Auto V")]
    [SynqRemoteInfo(GameUUids.GTAV)]
    [GTA5ContentCleaning]
    [RegistryInfo(@"SOFTWARE\Rockstar Games\Grand Theft Auto V", "InstallFolder")]
    public class GTA5Game : GTAGame
    {
        protected GTA5Game(Guid id) : this(id, new GTA5GameSettings()) {}
        public GTA5Game(Guid id, GTA5GameSettings settings) : base(id, settings) {}

        protected override void ConfirmLaunch() {
            base.ConfirmLaunch();
            new RequirementsHandler(InstalledState).ConfirmRequirements();
        }

        protected override void ConfirmInstall() {
            base.ConfirmInstall();
            new RequirementsHandler(InstalledState).ConfirmRequirements();
        }

        protected override IAbsoluteDirectoryPath GetDefaultDirectory() {
            return base.GetDefaultDirectory() ?? GetDefaultRockstarDirectory();
        }

        protected override async Task InstallImpl(IContentInstallationService installationService,
            IContentAction<IInstallableContent> content) {
            await base.InstallImpl(installationService, content).ConfigureAwait(false);
            await new GtaPackager(InstalledState.Directory).HandlePackages().ConfigureAwait(false);
            HandleBackups();
        }

        void HandleBackups() {
            var backupDir = InstalledState.Directory.GetChildDirectoryWithName(ContentInstaller.SyncBackupDir);
            if (!backupDir.Exists)
                return;
            RestoreFileFromBackup(backupDir, "VirtualFileSystem.dat");
            RestoreFileFromBackup(backupDir, "NativeTrainer.asi");
        }

        void RestoreFileFromBackup(IAbsoluteDirectoryPath backupDir, string fileName) {
            var existingFile = InstalledState.Directory.GetChildFileWithName(fileName);
            if (existingFile.Exists)
                return;
            var backupFile = backupDir.GetChildFileWithName(fileName);
            if (!backupFile.Exists)
                return;
            backupFile.Copy(existingFile);
        }

        static IAbsoluteDirectoryPath GetDefaultRockstarDirectory() {
            IAbsoluteDirectoryPath dir;
            var pf64 = GetProgramFiles64();
            if (pf64 != null) {
                dir = pf64
                    .GetChildDirectoryWithName("Rockstar Games")
                    .GetChildDirectoryWithName("Grand Theft Auto V");
                if (dir.Exists)
                    return dir;
            }
            dir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                .ToAbsoluteDirectoryPath()
                .GetChildDirectoryWithName("Rockstar Games")
                .GetChildDirectoryWithName("Grand Theft Auto V");
            if (dir.Exists)
                return dir;
            return null;
        }

        static IAbsoluteDirectoryPath GetProgramFiles64() {
            try {
                return
                    (RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                        .OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion")
                        .GetValue("ProgramFilesDir") as String)
                        .ToAbsoluteDirectoryPath();
            } catch (Exception) {
                return null;
            }
        }

        class RequirementsHandler
        {
            readonly OpenIvRequirement _openIvRequirement;
            readonly RequireScripthook _requireScripthook;

            public RequirementsHandler(GameInstalledState state) {
                _requireScripthook = new RequireScripthook(state, state.Directory.GetChildDirectoryWithName(ContentInstaller.SyncBackupDir));
                _openIvRequirement = new OpenIvRequirement(state);
            }

            public void ConfirmRequirements() {
                // TODO: Perhaps exceptions aren't the right approach for multiple missing entries? Perhaps should use a strategy pattern and collect the results etc, not as exceptions?
                var req = new List<GameRequirementMissingException>();
                try {
                    _openIvRequirement.RequireOpenIV();
                } catch (GameRequirementMissingException ex) {
                    req.Add(ex);
                }
                try {
                    _requireScripthook.RequireScriptHook();
                } catch (GameRequirementMissingException ex) {
                    req.Add(ex);
                }
                if (req.Any())
                    throw new MultiGameRequirementMissingException(req);

                _requireScripthook.BackupScriptHook();
            }

            class RequireScripthook
            {
                static readonly string[] scriptHookFiles = {"dinput8.dll", "ScriptHookV.dll"};
                readonly IAbsoluteDirectoryPath _gameLocalDataFolder;
                readonly GameInstalledState _state;

                public RequireScripthook(GameInstalledState state, IAbsoluteDirectoryPath gameLocalDataFolder) {
                    _state = state;
                    _gameLocalDataFolder = gameLocalDataFolder;
                }

                public void BackupScriptHook() {
                    _gameLocalDataFolder.MakeSurePathExists();
                    foreach (var f in scriptHookFiles)
                        _state.Directory.GetChildFileWithName(f).Copy(_gameLocalDataFolder.GetChildFileWithName(f));
                }

                public void RequireScriptHook() {
                    foreach (
                        var asiFile in
                            scriptHookFiles.Select(asi => _state.Directory.GetChildFileWithName(asi))
                                .Where(asiFile => !asiFile.Exists))
                        TryInstallScriptHook();
                }

                void TryInstallScriptHook() {
                    if (!_gameLocalDataFolder.Exists)
                        throw new ScriptHookMissingException();

                    foreach (var f in scriptHookFiles) {
                        var file = _gameLocalDataFolder.GetChildFileWithName(f);
                        if (!file.Exists)
                            throw new ScriptHookMissingException();
                        Tools.FileUtil.Ops.Copy(file, _state.Directory.GetChildFileWithName(f));
                    }
                }
            }

            class OpenIvRequirement
            {
                static readonly string[] openIvFiles = {"dinput8.dll", "OpenIV.asi"};
                readonly GameInstalledState _state;

                public OpenIvRequirement(GameInstalledState state) {
                    _state = state;
                }

                public void RequireOpenIV() {
                    foreach (var f in openIvFiles.Select(
                        f => _state.Directory.GetChildFileWithName(f)).Where(f => !f.Exists))
                        TryToInstallOpenIV();
                }

                void TryToInstallOpenIV() {
                    var localDataPath =
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                            .ToAbsoluteDirectoryPath()
                            .GetChildDirectoryWithName(@"New Technology Studio\Apps\OpenIV\Games\Five\x64");
                    if (!localDataPath.Exists)
                        throw new OpenIvMissingException();

                    foreach (var f in openIvFiles) {
                        Tools.FileUtil.Ops.Copy(localDataPath.GetChildFileWithName(f),
                            _state.Directory.GetChildFileWithName(f));
                    }
                }
            }
        }
    }

    public class OpenIvMissingException : GameRequirementMissingException {}

    public class ScriptHookMissingException : GameRequirementMissingException {}

    public class GTA5ContentCleaningAttribute : ContentCleaningAttribute
    {
        public override IReadOnlyCollection<IRelativePath> Exclusions => GameFiles();

        static IReadOnlyCollection<IRelativeFilePath> GameFiles() {
            return new[] {
                @"bink2w64.dll",
                @"commandline.txt",
                @"common.rpf",
                @"d3dcompiler_46.dll",
                @"d3dcsx_46.dll",
                @"GFSDK_ShadowLib.win64.dll",
                @"GFSDK_TXAA.win64.dll",
                @"GFSDK_TXAA_AlphaResolve.win64.dll",
                @"GPUPerfAPIDX11-x64.dll",
                @"GTA5.exe",
                @"GTAVLauncher.exe",
                @"NvPmApi.Core.win64.dll",
                @"PlayGTAV.exe",
                @"steam_api.dll",
                @"steam_api32.dll",
                @"steam_api64.dll",
                @"version.txt",
                @"x64a.rpf",
                @"x64b.rpf",
                @"x64c.rpf",
                @"x64d.rpf",
                @"x64e.rpf",
                @"x64f.rpf",
                @"x64g.rpf",
                @"x64h.rpf",
                @"x64i.rpf",
                @"x64j.rpf",
                @"x64k.rpf",
                @"x64l.rpf",
                @"x64m.rpf",
                @"x64n.rpf",
                @"x64o.rpf",
                @"x64p.rpf",
                @"x64q.rpf",
                @"x64r.rpf",
                @"x64s.rpf",
                @"x64t.rpf",
                @"x64u.rpf",
                @"x64v.rpf",
                @"x64w.rpf",
                @"ReadMe\Chinese\ReadMe.txt",
                @"ReadMe\English\ReadMe.txt",
                @"ReadMe\French\ReadMe.txt",
                @"ReadMe\German\ReadMe.txt",
                @"ReadMe\Italian\ReadMe.txt",
                @"ReadMe\Japanese\ReadMe.txt",
                @"ReadMe\Korean\ReadMe.txt",
                @"ReadMe\Mexican\Readme.txt",
                @"ReadMe\Polish\ReadMe.txt",
                @"ReadMe\Portuguese\ReadMe.txt",
                @"ReadMe\Russian\ReadMe.txt",
                @"ReadMe\Spanish\ReadMe.txt",
                @"update\update.rpf",
                @"update\ModdedUpdate\update.rpf",
                @"update\x64\metadata.dat",
                @"update\x64\data\errorcodes\american.txt",
                @"update\x64\data\errorcodes\chinese.txt",
                @"update\x64\data\errorcodes\french.txt",
                @"update\x64\data\errorcodes\german.txt",
                @"update\x64\data\errorcodes\italian.txt",
                @"update\x64\data\errorcodes\japanese.txt",
                @"update\x64\data\errorcodes\korean.txt",
                @"update\x64\data\errorcodes\mexican.txt",
                @"update\x64\data\errorcodes\polish.txt",
                @"update\x64\data\errorcodes\portuguese.txt",
                @"update\x64\data\errorcodes\russian.txt",
                @"update\x64\data\errorcodes\spanish.txt",
                @"update\x64\dlcpacks\mpchristmas2\dlc.rpf",
                @"update\x64\dlcpacks\mpheist\dlc.rpf",
                @"update\x64\dlcpacks\mpluxe\dlc.rpf",
                @"update\x64\dlcpacks\mpluxe2\dlc.rpf",
                @"update\x64\dlcpacks\mppatchesng\dlc.rpf",
                @"update\x64\dlcpacks\patchday1ng\dlc.rpf",
                @"update\x64\dlcpacks\patchday2bng\dlc.rpf",
                @"update\x64\dlcpacks\patchday2ng\dlc.rpf",
                @"update\x64\dlcpacks\patchday3ng\dlc.rpf",
                @"update\x64\dlcpacks\patchday4ng\dlc.rpf",
                @"update\x64\dlcpacks\patchday5ng\dlc.rpf",
                @"x64\metadata.dat",
                @"x64\audio\audio_rel.rpf",
                @"x64\audio\occlusion.rpf",
                @"x64\audio\sfx\ANIMALS.rpf",
                @"x64\audio\sfx\ANIMALS_FAR.rpf",
                @"x64\audio\sfx\ANIMALS_NEAR.rpf",
                @"x64\audio\sfx\CUTSCENE_MASTERED_ONLY.rpf",
                @"x64\audio\sfx\DLC_GTAO.rpf",
                @"x64\audio\sfx\INTERACTIVE_MUSIC.rpf",
                @"x64\audio\sfx\ONESHOT_AMBIENCE.rpf",
                @"x64\audio\sfx\PAIN.rpf",
                @"x64\audio\sfx\POLICE_SCANNER.rpf",
                @"x64\audio\sfx\PROLOGUE.rpf",
                @"x64\audio\sfx\RADIO_01_CLASS_ROCK.rpf",
                @"x64\audio\sfx\RADIO_02_POP.rpf",
                @"x64\audio\sfx\RADIO_03_HIPHOP_NEW.rpf",
                @"x64\audio\sfx\RADIO_04_PUNK.rpf",
                @"x64\audio\sfx\RADIO_05_TALK_01.rpf",
                @"x64\audio\sfx\RADIO_06_COUNTRY.rpf",
                @"x64\audio\sfx\RADIO_07_DANCE_01.rpf",
                @"x64\audio\sfx\RADIO_08_MEXICAN.rpf",
                @"x64\audio\sfx\RADIO_09_HIPHOP_OLD.rpf",
                @"x64\audio\sfx\RADIO_11_TALK_02.rpf",
                @"x64\audio\sfx\RADIO_12_REGGAE.rpf",
                @"x64\audio\sfx\RADIO_13_JAZZ.rpf",
                @"x64\audio\sfx\RADIO_14_DANCE_02.rpf",
                @"x64\audio\sfx\RADIO_15_MOTOWN.rpf",
                @"x64\audio\sfx\RADIO_16_SILVERLAKE.rpf",
                @"x64\audio\sfx\RADIO_17_FUNK.rpf",
                @"x64\audio\sfx\RADIO_18_90S_ROCK.rpf",
                @"x64\audio\sfx\RADIO_ADVERTS.rpf",
                @"x64\audio\sfx\RADIO_NEWS.rpf",
                @"x64\audio\sfx\RESIDENT.rpf",
                @"x64\audio\sfx\SCRIPT.rpf",
                @"x64\audio\sfx\SS_AC.rpf",
                @"x64\audio\sfx\SS_DE.rpf",
                @"x64\audio\sfx\SS_FF.rpf",
                @"x64\audio\sfx\SS_GM.rpf",
                @"x64\audio\sfx\SS_NP.rpf",
                @"x64\audio\sfx\SS_QR.rpf",
                @"x64\audio\sfx\SS_ST.rpf",
                @"x64\audio\sfx\SS_UZ.rpf",
                @"x64\audio\sfx\STREAMED_AMBIENCE.rpf",
                @"x64\audio\sfx\STREAMED_VEHICLES.rpf",
                @"x64\audio\sfx\STREAMED_VEHICLES_GRANULAR.rpf",
                @"x64\audio\sfx\STREAMED_VEHICLES_GRANULAR_NPC.rpf",
                @"x64\audio\sfx\STREAMED_VEHICLES_LOW_LATENCY.rpf",
                @"x64\audio\sfx\STREAMS.rpf",
                @"x64\audio\sfx\S_FULL_AMB_F.rpf",
                @"x64\audio\sfx\S_FULL_AMB_M.rpf",
                @"x64\audio\sfx\S_FULL_GAN.rpf",
                @"x64\audio\sfx\S_FULL_SER.rpf",
                @"x64\audio\sfx\S_MINI_AMB.rpf",
                @"x64\audio\sfx\S_MINI_GAN.rpf",
                @"x64\audio\sfx\S_MINI_SER.rpf",
                @"x64\audio\sfx\S_MISC.rpf",
                @"x64\audio\sfx\WEAPONS_PLAYER.rpf",
                @"x64\data\errorcodes\american.txt",
                @"x64\data\errorcodes\chinese.txt",
                @"x64\data\errorcodes\french.txt",
                @"x64\data\errorcodes\german.txt",
                @"x64\data\errorcodes\italian.txt",
                @"x64\data\errorcodes\japanese.txt",
                @"x64\data\errorcodes\korean.txt",
                @"x64\data\errorcodes\mexican.txt",
                @"x64\data\errorcodes\polish.txt",
                @"x64\data\errorcodes\portuguese.txt",
                @"x64\data\errorcodes\russian.txt",
                @"x64\data\errorcodes\spanish.txt"
            }.Select(x => (@".\" + x).ToRelativeFilePath()).ToList();
        }
    }
}