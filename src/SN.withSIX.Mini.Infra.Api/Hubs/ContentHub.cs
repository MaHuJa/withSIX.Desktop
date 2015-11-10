// <copyright company="SIX Networks GmbH" file="ContentHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SN.withSIX.Mini.Applications.Usecases.Api;
using SN.withSIX.Mini.Applications.Usecases.Main.Games;
using SN.withSIX.Mini.Applications.Usecases.Main.Games.Installed;
using LaunchContent = SN.withSIX.Mini.Applications.Usecases.Api.LaunchContent;
using LaunchContents = SN.withSIX.Mini.Applications.Usecases.Api.LaunchContents;

namespace SN.withSIX.Mini.Infra.Api.Hubs
{
    // TODO: Error reporting to the Website client?
    // TODO: Error reporting to the Desktop client?
    public class ContentHub : HubBase<IContentClientHub>
    {
        public Task<ClientContentInfo2> GetContent(Guid gameId) {
            return RequestAsync(new GetContent(gameId));
        }

        public Task PlayContent(PlayContent command) {
            return RequestAsync(command);
        }

        public Task PlayContents(PlayContents command) {
            return RequestAsync(command);
        }

        public Task SyncCollections(SyncCollections command) {
            return RequestAsync(command);
        }

        public Task LaunchGame(LaunchGame command) {
            return RequestAsync(command);
        }

        public Task LaunchContent(LaunchContent command) {
            return RequestAsync(command);
        }

        public Task LaunchContents(LaunchContents command) {
            return RequestAsync(command);
        }

        public Task InstallContent(InstallContent command) {
            return RequestAsync(command);
        }

        public Task UninstallContent(UninstallInstalledItem command) {
            return RequestAsync(command);
        }

        public Task InstallCollection(InstallCollection command) {
            return RequestAsync(command);
        }

        public Task DeleteCollection(DeleteCollection command) {
            return RequestAsync(command);
        }

        public Task InstallContents(InstallContents command) {
            return RequestAsync(command);
        }

        public Task Abort(AbortCommand command) {
            return RequestAsync(command);
        }

        public Task AbortAll() {
            return RequestAsync(new AbortAllCommand());
        }
    }

    public interface IContentClientHub
    {
        Task ContentFavorited(Guid gameId, FavoriteContentModel favoriteContent);
        Task ContentUnfavorited(Guid gameId, Guid contentId);
        Task RecentItemAdded(Guid gameId, RecentContentModel recentItem);
        Task RecentItemUsed(Guid gameId, Guid recentItemId, DateTime playedAt);
        Task ContentInstalled(Guid gameId, InstalledContentModel installedContent);
    }
}