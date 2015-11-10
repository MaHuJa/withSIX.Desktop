// <copyright company="SIX Networks GmbH" file="DomainEvilGlobal.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Infrastructure;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Core
{
    public static class DomainEvilGlobal
    {
        static readonly Lazy<LocalMachineInfo> localMachineInfo = new Lazy<LocalMachineInfo>();
        [Obsolete("Move storage concerns into Application")]
        public static INoteStorage NoteStorage { get; set; }
        [Obsolete("Destroy")]
        public static EvilGlobalSelectedGame SelectedGame { get; set; }
        [Obsolete("Global singleton; instead import UserSettings through constructor")]
        public static UserSettings Settings { get; set; }
        [Obsolete("Cleanup")]
        public static SecretData SecretData { get; set; }
        [Obsolete("Global singleton; instead import LocalMachineInfo through constructor")]
        public static LocalMachineInfo LocalMachineInfo
        {
            get { return localMachineInfo.Value; }
        }
    }
}