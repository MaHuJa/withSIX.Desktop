// <copyright company="SIX Networks GmbH" file="OptionBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Options
{
    [DataContract]
    public abstract class OptionBase : PropertyChangedBase
    {
        protected static void SaveSettings() {
            DomainEvilGlobal.Settings.RaiseChanged();
        }
    }
}