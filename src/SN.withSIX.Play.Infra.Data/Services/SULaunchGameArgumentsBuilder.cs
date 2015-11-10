// <copyright company="SIX Networks GmbH" file="SULaunchGameArgumentsBuilder.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Services;

namespace SN.withSIX.Play.Infra.Data.Services
{
    abstract class SULaunchGameArgumentsBuilder
    {
        protected readonly LaunchGameInfoBase Spec;

        protected SULaunchGameArgumentsBuilder(LaunchGameInfoBase spec) {
            Contract.Requires<ArgumentNullException>(spec != null);
            Contract.Requires<ArgumentNullException>(spec.LaunchExecutable != null);
            Contract.Requires<ArgumentNullException>(spec.WorkingDirectory != null);
            Contract.Requires<ArgumentNullException>(spec.StartupParameters != null);
            Spec = spec;
        }

        protected string GetStartupParameters() {
            return "--arguments=" + Spec.StartupParameters.CombineParameters();
        }

        public virtual IEnumerable<string> Build() {
            return new[] {
                UpdaterCommands.LaunchGame,
                "--gamePath=" + Spec.LaunchExecutable,
                "--workingDirectory=" + Spec.WorkingDirectory,
                GetPriority(),
                GetAffinity(),
                //GetRealExe(),
                GetStartupParameters()
            }.Where(x => x != null);
        }

        protected string GetAffinity() {
            return Spec.Affinity != null && Spec.Affinity.Any()
                ? "--affinity=" + string.Join(",", Spec.Affinity)
                : null;
        }

        protected string GetPriority() {
            return "--priority=" + Spec.Priority;
        }

        protected string GetRealExe() {
            if (Spec.LaunchExecutable != Spec.ExpectedExecutable)
                return "--realExe=" + Spec.ExpectedExecutable;
            return null;
        }
    }

    class SULaunchDefaultGameArgumentsBuilder : SULaunchGameArgumentsBuilder
    {
        public SULaunchDefaultGameArgumentsBuilder(LaunchGameInfo spec) : base(spec) {}

        public override IEnumerable<string> Build() {
            return new[] {
                UpdaterCommands.LaunchGame,
                "--gamePath=" + Spec.LaunchExecutable,
                "--workingDirectory=" + Spec.WorkingDirectory,
                GetPriority(),
                GetAffinity(),
                //GetRealExe(),
                GetStartupParameters()
            }.Where(x => x != null);
        }
    }

    class SULaunchGameJavaArgumentsBuilder : SULaunchGameArgumentsBuilder
    {
        readonly IAbsoluteDirectoryPath _javaPath;

        public SULaunchGameJavaArgumentsBuilder(LaunchGameWithJavaInfo spec, IAbsoluteDirectoryPath javaPath)
            : base(spec) {
            Contract.Requires<ArgumentNullException>(javaPath != null);
            _javaPath = javaPath;
        }

        public override IEnumerable<string> Build() {
            throw new NotImplementedException();
        }
    }

    class SULaunchGameSteamArgumentsBuilder : SULaunchGameArgumentsBuilder
    {
        readonly LaunchGameWithSteamInfo _spec;
        readonly IAbsoluteDirectoryPath _steamPath;

        public SULaunchGameSteamArgumentsBuilder(LaunchGameWithSteamInfo spec, IAbsoluteDirectoryPath steamPath)
            : base(spec) {
            Contract.Requires<ArgumentNullException>(steamPath != null);
            Contract.Requires<ArgumentNullException>(spec.SteamAppId != -1);
            _steamPath = steamPath;
            _spec = spec;
        }

        protected string GetSteamAppId() {
            return "--steamID=" + _spec.SteamAppId;
        }

        protected string GetSteamPath() {
            return "--steamPath=" + _steamPath;
        }

        public override IEnumerable<string> Build() {
            return new[] {
                UpdaterCommands.LaunchGame,
                "--gamePath=" + Spec.LaunchExecutable,
                "--workingDirectory=" + Spec.WorkingDirectory,
                GetSteamDRM(),
                GetSteamAppId(),
                GetSteamPath(),
                GetPriority(),
                GetAffinity(),
                //GetRealExe(),
                GetStartupParameters()
            }.Where(x => x != null);
        }

        string GetSteamDRM() {
            return _spec.SteamDRM ? "--steamDRM" : null;
        }
    }

    class SULaunchGameSteamLegacyArgumentsBuilder : SULaunchGameSteamArgumentsBuilder
    {
        public SULaunchGameSteamLegacyArgumentsBuilder(LaunchGameWithSteamLegacyInfo spec,
            IAbsoluteDirectoryPath steamPath)
            : base(spec, steamPath) {}

        public override IEnumerable<string> Build() {
            return new[] {
                UpdaterCommands.LaunchGame,
                "--gamePath=" + Spec.LaunchExecutable,
                "--workingDirectory=" + Spec.WorkingDirectory,
                "--legacyLaunch",
                GetSteamAppId(),
                GetSteamPath(),
                GetPriority(),
                GetAffinity(),
                //GetRealExe(),
                GetStartupParameters()
            }.Where(x => x != null);
        }
    }
}