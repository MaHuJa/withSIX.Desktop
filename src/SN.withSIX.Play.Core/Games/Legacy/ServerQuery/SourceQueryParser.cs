// <copyright company="SIX Networks GmbH" file="SourceQueryParser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SN.withSIX.Core.Services;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Servers;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public class SourceQueryParser : IServerQueryParser, IDomainService
    {
        public ServerQueryResult ParsePackets(ServerQueryState state) {
            if (state.ReceivedPackets.Count != 3)
                throw new Exception("Wrong number of packets");
            return ParseResponse(state.ReceivedPackets[0], state.ReceivedPackets[1], state.ReceivedPackets[2],
                Convert.ToInt64(state.Pings.Average()),
                state.Server);
        }

        static ServerQueryResult ParseResponse(IEnumerable<byte> info, IEnumerable<byte> player,
            IEnumerable<byte> rules, long ping, Server server) {
            IDictionary<string, string> settings = new Dictionary<string, string>();

            ParseSettings(settings, info.ToArray());
            ParseSettings2(settings, rules.ToArray());
            var players = ParsePlayers(server, player.ToArray());

            return new SourceServerQueryResult(settings) {
                Players = players,
                Ping = ping
            };
        }

        static void ParseSettings(IDictionary<string, string> settings, byte[] info) {
            var pos = 5; //skip header
            settings["protocol"] = info[pos++].ToString(CultureInfo.InvariantCulture);
            var start = pos;
            while (info[pos++] != 0x00) {}
            settings["name"] = Encoding.UTF8.GetString(info, start, pos - start - 1);
            start = pos;
            while (info[pos++] != 0x00) {}
            settings["map"] = Encoding.UTF8.GetString(info, start, pos - start - 1);
            start = pos;
            while (info[pos++] != 0x00) {}
            settings["folder"] = Encoding.UTF8.GetString(info, start, pos - start - 1);
            start = pos;
            while (info[pos++] != 0x00) {}
            settings["game"] = Encoding.UTF8.GetString(info, start, pos - start - 1);
            settings["appId"] = BitConverter.ToInt16(info, pos).ToString(CultureInfo.InvariantCulture);
            pos += 2;
            settings["playerCount"] = info[pos++].ToString(CultureInfo.InvariantCulture);
            settings["playerMax"] = info[pos++].ToString(CultureInfo.InvariantCulture);
            settings["botCount"] = info[pos++].ToString(CultureInfo.InvariantCulture);
            settings["serverType"] = info[pos++].ToString(CultureInfo.InvariantCulture);
            settings["environment"] = info[pos++].ToString(CultureInfo.InvariantCulture);
            settings["visibility"] = info[pos++].ToString(CultureInfo.InvariantCulture);
            settings["vac"] = info[pos++].ToString(CultureInfo.InvariantCulture);
            //if The Ship, additional fields:
            //_results.mode = reply[pos++];
            //_results.witnesses = reply[pos++];
            //_results.duration = reply[pos++];
            start = pos;
            while (info[pos++] != 0x00) {}
            settings["version"] = Encoding.UTF8.GetString(info, start, pos - start - 1);
            var edf = info[pos++];
            settings["edf"] = edf.ToString(CultureInfo.InvariantCulture);
            if ((edf & 0x80) != 0)
                settings["port"] = BitConverter.ToInt16(info, pos).ToString(CultureInfo.InvariantCulture);
            pos += 2;
            if ((edf & 0x10) != 0)
                settings["steamId"] = BitConverter.ToInt64(info, pos).ToString(CultureInfo.InvariantCulture);
            pos += 8;
            if ((edf & 0x40) != 0) {
                settings["tvPort"] = BitConverter.ToInt16(info, pos).ToString(CultureInfo.InvariantCulture);
                pos += 2;
                start = pos;
                while (info[pos++] != 0x00) {}
                settings["tvName"] = Encoding.UTF8.GetString(info, start, pos - start - 1);
            }
            if ((edf & 0x20) != 0) {
                start = pos;
                while (info[pos++] != 0x00) {}
                settings["keywords"] = Encoding.UTF8.GetString(info, start, pos - start - 1);
            }
            //gameID = full appID if was truncated in previous field (not always the case)
            if ((edf & 0x01) != 0)
                settings["gameId"] = BitConverter.ToInt64(info, pos).ToString(CultureInfo.InvariantCulture);
            //TODO: find out what the extra data in this packet is from DayZ source
        }

        static void ParseSettings2(IDictionary<string, string> settings, byte[] rules) {
            var sb = new StringBuilder();
            var pos = 5; //skip header
            if (rules.Length < 8) // toInt16 requires 2 bytes to work with (pos 5 = 6, + 2 = 8)
                throw new ArgumentOutOfRangeException("rules does not have the correct length: " + rules);
            var ruleCount = BitConverter.ToInt16(rules, pos);
            pos += 2;
            for (var i = 0; i < ruleCount; i++) {
                if (pos >= rules.Length)
                    break;
                var start = pos;
                while (rules[pos++] != 0x00) {}
                var rule = Encoding.UTF8.GetString(rules, start, pos - start - 1);
                sb.Append(rule + ",");
                start = pos;
                while (rules[pos++] != 0x00) {}
                var value = Encoding.UTF8.GetString(rules, start, pos - start - 1);
                settings[rule] = value;
            }
            settings["ruleNames"] = sb.Length > 0 ? sb.ToString(0, sb.Length - 1) : string.Empty;
        }

        static Player[] ParsePlayers(Server server, byte[] players) {
            //TODO: Player doesn't have same info for different game/server types
            var pos = 5; //skip header
            var cnt = players[pos++];
            var playerAr = new Player[cnt];
            for (var i = 0; i < cnt; i++) {
                if (pos >= players.Length)
                    break;
                // ReSharper disable once UnusedVariable
                var pnum = players[pos++];
                //TODO: index is always zero, why? Defined as "Index of player chunk starting from 0." should be usable below
                var start = pos;
                while (players[pos++] != 0x00) {}
                var name = Encoding.UTF8.GetString(players, start, pos - start - 1);
                var score = BitConverter.ToInt32(players, pos);
                pos += 4;
                var duration = (int) BitConverter.ToSingle(players, pos); //in seconds, dropped float precision
                pos += 4;
                playerAr[i] = new SourcePlayer(server, name, score, TimeSpan.FromSeconds(duration));
                //use i instead of pnum
            }

            return playerAr;
        }
    }
}