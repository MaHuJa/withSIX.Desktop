// <copyright company="SIX Networks GmbH" file="DesignTimeGamesViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using Caliburn.Micro;
using ShortBus;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Applications.Services.Infrastructure;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class DesignTimeGamesViewModel : GamesViewModel, IDesignTimeViewModel
    {
        public DesignTimeGamesViewModel()
            : base(IoC.Get<IMediator>(),
                IoC.Get<GameInfoOverlayViewModel>(),
                IoC.Get<IGameContext>(),
                IoC.Get<UserSettings>(),
                new Lazy<LaunchManager>()) {}
    }
}