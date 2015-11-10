// <copyright company="SIX Networks GmbH" file="GetAbout.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.ViewModels.About;

namespace SN.withSIX.Mini.Applications.Usecases.Main
{
    public class GetAbout : IAsyncQuery<IAboutViewModel> {}


    public class GetAboutHandler : IAsyncRequestHandler<GetAbout, IAboutViewModel>
    {
        public Task<IAboutViewModel> HandleAsync(GetAbout request) {
            return
                Task.FromResult(
                    (IAboutViewModel)
                        new AboutViewModel {Version = Consts.ProductTitle + " " + Consts.ProductVersion});
        }
    }
}