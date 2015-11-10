// <copyright company="SIX Networks GmbH" file="SourceServerQueryTest.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Net;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Games.Entities.RealVirtuality;
using SN.withSIX.Play.Core.Games.Legacy.ServerQuery;
using SN.withSIX.Play.Tests.Core.Support;

namespace SN.withSIX.Play.Tests.Core.Unit.GameTests
{
    [TestFixture]
    public class SourceServerQueryTest
    {
        [Test, Category("Integration")]
        public async Task Test1() {
            SharedSupport.Init();
            var serverAddress = new ServerAddress(IPAddress.Parse("37.220.18.218"), 27016);
            var query = new SourceServerQuery(serverAddress, "dayz",
                new SourceQueryParser());
            await
                query.UpdateAsync(new ServerQueryState {
                    Server = new ArmaServer(A.Fake<Arma3Game>(), serverAddress)
                });
        }
    }
}