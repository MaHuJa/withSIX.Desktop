// <copyright company="SIX Networks GmbH" file="GameServerQueryHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Services;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Core.Games.Services
{
    public interface IGameServerQueryHandler
    {
        Task<IEnumerable<ServerQueryResult>> Query(GamespyServersQuery query);
        Task<IEnumerable<ServerQueryResult>> Query(SourceServersQuery query);
    }

    public abstract class ServersQuery
    {
        protected ServersQuery(string tag) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(tag));
            Tag = tag;
        }

        public string Tag { get; }
    }

    public class SourceServersQuery : ServersQuery
    {
        public SourceServersQuery(string tag) : base(tag) {}

        [Obsolete("Bad hack")]
        public Task QueryServer(ServerQueryState state) {
            return new SourceServerQuery(state.Server.Address, Tag, new SourceQueryParser())
                .UpdateAsync(state);
        }
    }

    public class GamespyServersQuery : ServersQuery
    {
        public GamespyServersQuery(string tag) : base(tag) {}

        [Obsolete("Bad hack")]
        public Task QueryServer(ServerQueryState state) {
            return new GamespyServerQuery(state.Server.Address, Tag, new GamespyQueryParser())
                .UpdateAsync(state);
        }
    }

    public class GameServerQueryHandler : IGameServerQueryHandler, IDomainService
    {
        readonly IDataDownloader _downloader;

        public GameServerQueryHandler(IDataDownloader downloader) {
            _downloader = downloader;
        }

        [DoNotObfuscate]
        public Task<IEnumerable<ServerQueryResult>> Query(GamespyServersQuery query) {
            //var gamespyQuery = new SixMasterQuery(query.Tag, _downloader);
            //return gamespyQuery.GetParsedServers();
            return Task.FromResult(Enumerable.Empty<ServerQueryResult>());
        }

        [DoNotObfuscate]
        public Task<IEnumerable<ServerQueryResult>> Query(SourceServersQuery query) {
            var gamespyQuery = new SourceMasterQuery(query.Tag);
            return gamespyQuery.GetParsedServers();
        }
    }
}