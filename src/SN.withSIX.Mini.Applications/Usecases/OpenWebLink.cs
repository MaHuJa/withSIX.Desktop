// <copyright company="SIX Networks GmbH" file="OpenWebLink.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Mini.Applications.Usecases
{
    public class OpenWebLink : IAsyncVoidCommand
    {
        public OpenWebLink(ViewType type, string additional = null) {
            Type = type;
            Additional = additional;
        }

        public string Additional { get; }
        public ViewType Type { get; }
    }

    public class OpenArbWebLink : IAsyncVoidCommand
    {
        public OpenArbWebLink(Uri uri) {
            Uri = uri;
        }

        public Uri Uri { get; }
    }

    // TODO: Navigation is normally a Query, not a Command...
    // In that case it needs to return the ViewModel so that the caller can display it either by routing to it,
    // or by leveraging a DialogManager to create a Window/Popup/MessageBox etc?
    //
    // One issue with that is that we are normally supposed to pass back and forth data-containers without logic inside them..
    // Which is not the case with WPF ViewModels, as they include: Commands and methods that need to interact with the Mediator, and perhaps a DialogManager?
    // Idea: What if we do pass only data containers back, but then construct the ViewModels on the other side? Kind of like we do in Angular?
    public class OpenViewHandler : IAsyncVoidCommandHandler<OpenWebLink>, IAsyncVoidCommandHandler<OpenArbWebLink>
    {
        public Task<UnitType> HandleAsync(OpenArbWebLink request) {
            return UriOpener.OpenUri(request.Uri).Void();
        }

        public Task<UnitType> HandleAsync(OpenWebLink request) {
            // TODO: This makes most sense for Online links...
            // Less sense for LOCAL, as most if not all views require queries to be executed to fill the ViewModels...
            // Unless we want to perform those inside the factories, or in here - not too bad idea actually but seems like the enum is then less useful as we can achieve the same with proper query objects?
            return OpenUri(request).Void();
        }

        static Task OpenUri(OpenWebLink request) {
            switch (request.Type) {
            // Online
            case ViewType.Browse:
                return UriOpener.OpenUri(Urls.Play, request.Additional);
            case ViewType.Friends:
                return UriOpener.OpenUri(Urls.Connect, "me/friends");
            case ViewType.PremiumAccount:
                return UriOpener.OpenUri(Urls.Connect, "me/premium");

            case ViewType.GoPremium:
                return UriOpener.OpenUri(Urls.Main, "gopremium");

            case ViewType.Help:
                return UriOpener.OpenUri(new Uri("http://withsix.readthedocs.org"));

            case ViewType.Profile:
                return UriOpener.OpenUri(Urls.Connect, "me/content");

            case ViewType.Issues:
                return UriOpener.OpenUri(new Uri("https://trello.com/b/EQeUdFGd/withsix-report-issues"));
                    // Link to comments and feedback instead??

            case ViewType.Suggestions:
                return
                    UriOpener.OpenUri(new Uri("https://community.withsix.com/category/4/comments-feedback"));
            case ViewType.Community:
                return
                    UriOpener.OpenUri(new Uri("https://community.withsix.com"));
            case ViewType.License:
                return UriOpener.OpenUri(Urls.Main, "legal");

            case ViewType.Update:
                return UriOpener.OpenUri(Urls.Main, "update");

            default: {
                throw new NotSupportedException(request.Type + " Is not supported!");
            }
            }
        }
    }

    public enum ViewType
    {
        // Online
        Browse,
        Friends,
        GoPremium,
        PremiumAccount,
        Profile,
        Issues,
        License,
        Update,
        Suggestions,
        Community,
        Help
    }
}