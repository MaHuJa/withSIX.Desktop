// <copyright company="SIX Networks GmbH" file="GameExceptionHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using ReactiveUI;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Core;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Services.ContentInstaller;
using SN.withSIX.Sync.Core.Packages;
using SN.withSIX.Sync.Core.Transfer.MirrorSelectors;

namespace SN.withSIX.Mini.Applications
{
    public class GameExceptionHandler : BasicExternalExceptionhandler
    {
        public override UserError HandleException(Exception ex, string action = "Action") {
            Contract.Requires<ArgumentNullException>(action != null);
            return Handle((dynamic) ex, action);
        }

        // TODO: Better handler where we guide the user to go to the settings, and configure the game, then retry?
        protected static InformationalUserError Handle(GameNotInstalledException ex, string action)
            => new ConfigureGameFirstUserError(ex, ex.Message, "Please configure the game first in the Settings");

        protected static InformationalUserError Handle(GameInstallationInProgressException ex, string action)
            => new InformationalUserError(ex, "Currently only one action per game is supported", "Unsupported");

        protected static InformationalUserError Handle(NoSourceFoundException ex, string action)
            =>
                new InformationalUserError(ex,
                    "We could not find this Content, perhaps it was removed or there is a network error",
                    "Could not find the desired content");

        protected static InformationalUserError Handle(NotFoundException ex, string action)
            =>
                new InformationalUserError(ex,
                    "We could not find this Content, perhaps it was removed or there is a network error",
                    "Could not find the desired content");

        protected static InformationalUserError Handle(HostListExhausted ex, string action)
            =>
                new InformationalUserError(ex,
                    "We were unable to download some or all of the content, perhaps it was removed or there is a network error, try again later?",
                    "Download error");
    }

    public class ConfigureGameFirstUserError : InformationalUserError
    {
        public ConfigureGameFirstUserError(Exception exception, string message, string title = null)
            : base(exception, message, title) {}
    }
}