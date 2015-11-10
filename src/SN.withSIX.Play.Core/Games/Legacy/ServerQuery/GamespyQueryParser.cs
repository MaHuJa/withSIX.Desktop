// <copyright company="SIX Networks GmbH" file="GamespyQueryParser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Services;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Servers;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public class GamespyQueryParser : IServerQueryParser, IDomainService
    {
        public ServerQueryResult ParsePackets(ServerQueryState state) {
            return ParseResponse(JoinPackets(state.ReceivedPackets), Convert.ToInt64(state.Pings.Average()),
                state.Server);
        }

        static ServerQueryResult ParseResponse(byte[] response, long ping, Server server) {
            var items = SplitResponse(response);
            var data = items.TakeWhile(x => !x.Contains("player_")).ToList();
            var settings = ParseSettings(data);
            var players = ParsePlayers(server, items);

            return new GamespyServerQueryResult(settings) {
                Players = players,
                Ping = ping
            };
        }

        static byte[] JoinPackets(IDictionary<int, IEnumerable<byte>> data) {
            var reply = new byte[] {};
            var indexes = data.Keys.ToList();
            indexes.Sort();

            reply = indexes.Aggregate(reply,
                (current, index) =>
                    current.Concat(
                        data[index].Skip(16)).ToArray());

            return reply;
        }

        static string[] SplitResponse(byte[] reply) {
            return Encoding.UTF8.GetString(reply).Split(new[] {"\0"}, StringSplitOptions.None);
        }

        static Dictionary<string, string> ParseSettings(IList<string> data) {
            var settings = new Dictionary<string, string>();
            for (var index = 0; index < data.Count - 1; index += 2)
                ParseSetting(data, index, settings);
            return settings;
        }

        static void ParseSetting(IList<string> data, int index, IDictionary<string, string> settings) {
            var name = data[index];
            var value = data[index + 1];

            //settings.Add(name, value);
            settings[name] = value;
        }

        static Player[] ParsePlayers(Server server, string[] items) {
            var names = items.SkipWhile(x => !x.Contains("player_"))
                .Skip(2)
                .TakeWhile(x => x != "team_" && x != String.Empty)
                .ToArray();

            var teams = items.SkipWhile(x => x != "team_")
                .Skip(2)
                .Take(names.Length)
                .ToArray();

            var scores = items.SkipWhile(x => x != "score_")
                .Skip(2)
                .Take(names.Length)
                .ToArray();

            var deaths = items.SkipWhile(x => x != "deaths_")
                .Skip(2)
                .Take(names.Length)
                .ToArray();

            return names.Select((x, index) => CreatePlayer(server, x, teams, index, scores, deaths))
                .ToArray<Player>();
        }

        static GamespyPlayer CreatePlayer(Server server, string x, ICollection<string> teams, int index,
            ICollection<string> scores, ICollection<string> deaths) {
            return new GamespyPlayer(
                server,
                x,
                teams.Count > index
                    ? teams.ElementAt(index)
                    : null,
                scores.Count > index
                    ? scores.ElementAt(index).TryInt()
                    : 0,
                deaths.Count > index
                    ? deaths.ElementAt(index).TryInt()
                    : 0
                );
        }
    }
}