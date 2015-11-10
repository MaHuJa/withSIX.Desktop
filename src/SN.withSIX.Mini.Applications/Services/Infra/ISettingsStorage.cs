// <copyright company="SIX Networks GmbH" file="ISettingsStorage.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using SN.withSIX.Mini.Applications.Models;

namespace SN.withSIX.Mini.Applications.Services.Infra
{
    public interface ISettingsStorageReadOnly
    {
        Settings Settings { get; }
    }

    public interface ISettingsStorage : ISettingsStorageReadOnly
    {
        Task SaveSettings();
    }
}