// <copyright company="SIX Networks GmbH" file="RecentInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Mini.Core.Games
{
    public interface IRecentInfo
    {
        DateTime CreatedAt { get; }
        DateTime LastUsed { get; }
        LaunchType LaunchType { get; }
    }

    [DataContract]
    public class RecentInfo : IRecentInfo
    {
        public RecentInfo(LaunchType launchType = LaunchType.Default) {
            LaunchType = launchType;
            CreatedAt = Tools.Generic.GetCurrentUtcDateTime;
            LastUsed = CreatedAt;
        }

        [DataMember]
        public DateTime CreatedAt { get; protected set; }
        [DataMember]
        public DateTime LastUsed { get; protected set; }
        [DataMember]
        public LaunchType LaunchType { get; protected set; }
    }

    public class InstallInfo : IInstallInfo
    {
        public InstallInfo() {
            CreatedAt = Tools.Generic.GetCurrentUtcDateTime;
            LastInstalled = CreatedAt;
        }

        [DataMember]
        public DateTime CreatedAt { get; protected set; }
        [DataMember]
        public DateTime LastInstalled { get; protected set; }
        [DataMember]
        public DateTime LastUpdated { get; protected set; }

        //[DataMember]
        //public SpecificVersion LastUpdatedTo { get; protected set; }

        public void Updated()
        { // SpecificVersion updatedTo
            //LastUpdatedTo = updatedTo;
            LastUpdated = Tools.Generic.GetCurrentUtcDateTime;
        }
    }

    public interface IInstallInfo {}
}