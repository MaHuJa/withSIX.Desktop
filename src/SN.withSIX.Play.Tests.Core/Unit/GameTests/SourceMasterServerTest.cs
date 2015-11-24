// <copyright company="SIX Networks GmbH" file="SourceMasterServerTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests
{
    [TestFixture, Ignore("")]
    public class SourceMasterServerTest
    {
        [Test]
        public async Task ServerInfo() {
            var mq = new SourceMasterQuery("dayz");
            var servers = await mq.GetParsedServers(false, 200).ConfigureAwait(false);
            //TODO: not all servers returned will respond, allow timeout or verify good servers?
            servers = servers.OrderBy(x => Guid.NewGuid()).Take(3);
            foreach (var d in servers) {
                var split = d.Settings["address"].Split(':');
                var sq = new SourceServerQuery(new ServerAddress(IPAddress.Parse(split[0]), Convert.ToInt32(split[1])),
                    "dayz", new SourceQueryParser());
                var state = new ServerQueryState {Server = A.Fake<Server>()};
                await sq.UpdateAsync(state).ConfigureAwait(false);
                state.Exception.Should().BeNull();
                state.Success.Should().BeTrue();
            }
        }

        [Test]
        public async Task ServerList() {
            var mq = new SourceMasterQuery("");
            var result = await mq.GetParsedServers().ConfigureAwait(false);
            result = result.ToArray();
            result.Should().NotBeEmpty();
        }

/*        [Test]
        public async Task SixServerList() {
            var mq = new SixMasterQuery("arma2pc", A.Fake<IDataDownloader>());
            var result = await mq.GetParsedServers().ConfigureAwait(false);
            result = result.ToArray();
            result.Should().NotBeEmpty();
        }*/
    }
}