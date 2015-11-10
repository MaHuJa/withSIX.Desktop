// <copyright company="SIX Networks GmbH" file="ChangeGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.ViewModels.Main.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Main.Games
{
    public class ChangeGame : CompositeCommandBasic<IGameViewModel>
    {
        public ChangeGame(Guid id) : base(new GetGame(id), new SaveSelectedGame(id), new ScanForNewLocalContent(id)) {}
    }
}