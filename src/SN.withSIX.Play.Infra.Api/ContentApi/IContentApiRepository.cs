// <copyright company="SIX Networks GmbH" file="IContentApiRepository.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections;
using System.Threading.Tasks;

namespace SN.withSIX.Play.Infra.Api.ContentApi
{
    interface IContentApiRepository
    {
        string Hash { get; }
        Task<bool> TryLoadFromDisk();
        Task LoadFromApi(string hash);
        IEnumerable GetValues();
    }
}