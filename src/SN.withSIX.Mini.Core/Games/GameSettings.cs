// <copyright company="SIX Networks GmbH" file="GameSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using NDepend.Path;

namespace SN.withSIX.Mini.Core.Games
{
    [DataContract]
    public abstract class GameSettings
    {
        [DataMember]
        public GameStartupParameters StartupParameters { get; protected set; }
        public IAbsoluteDirectoryPath GameDirectory { get; set; }
        public IAbsoluteDirectoryPath RepoDirectory { get; set; }
        [DataMember]
        protected string RepoDirectoryInternal { get; set; }
        [DataMember]
        protected string GameDirectoryInternal { get; set; }
        // WOrkaround frigging json .net 7.0 converter issue/!?!
        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            RepoDirectory = RepoDirectoryInternal?.ToAbsoluteDirectoryPath();
            GameDirectory = GameDirectoryInternal?.ToAbsoluteDirectoryPath();
        }

        [OnSerializing]
        void OnSerializing(StreamingContext context) {
            RepoDirectoryInternal = RepoDirectory?.ToString();
            GameDirectoryInternal = GameDirectory?.ToString();
        }
    }

    public interface IHavePackageDirectory
    {
        IAbsoluteDirectoryPath PackageDirectory { get; set; }
    }

    [DataContract]
    public abstract class GameSettingsWithConfigurablePackageDirectory : GameSettings, IHavePackageDirectory
    {
        [DataMember]
        protected string PackageDirectoryInternal { get; set; }
        public IAbsoluteDirectoryPath PackageDirectory { get; set; }
        // Workaround frigging json .net 7.0 converter issue/!?!
        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            PackageDirectory = PackageDirectoryInternal?.ToAbsoluteDirectoryPath();
        }

        [OnSerializing]
        void OnSerializing(StreamingContext context) {
            PackageDirectoryInternal = PackageDirectory?.ToString();
        }
    }
}