// <copyright company="SIX Networks GmbH" file="ServerMapper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using AutoMapper;
using AutoMapper.Mappers;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public class ServerMapper
    {
        [Obsolete("Evil global")] public static readonly ServerMapper Instance = new ServerMapper();
        static readonly ConcurrentDictionary<string, Regex> rxCache = new ConcurrentDictionary<string, Regex>();

        ServerMapper() {
            MappingEngine = GetMapper();
        }

        MappingEngine MappingEngine { get; }

        public void Map(ServerQueryResult queryResult, Server existing) {
            MappingEngine.Map(queryResult, existing);
        }

        static MappingEngine GetMapper() {
            var mapConfig = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            mapConfig.SetupConverters();

            mapConfig.CreateMap<ServerQueryResult, Server>()
                .Include<GamespyServerQueryResult, Server>()
                .Include<SourceServerQueryResult, Server>()
                .Include<SourceMasterServerQueryResult, Server>()
                .ForSourceMember(x => x.Settings, opt => opt.Ignore())
                .ForMember(x => x.Ping, opt => opt.Ignore());

            SetupGamespy(mapConfig);
            SetupSource(mapConfig);

            mapConfig.Seal();

            return new MappingEngine(mapConfig);
        }

        static void SetupGamespy(ConfigurationStore mapConfig) {
            mapConfig.CreateMap<GamespyServerQueryResult, Server>()
                .ForMember(x => x.IsDedicated,
                    opt => opt.ResolveUsing(src => GetValueWithFallback(src, "dedicated", "ds").TryInt() == 1))
                .ForMember(x => x.NumPlayers,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("numplayers").TryInt()))
                .ForMember(x => x.MaxPlayers,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("maxplayers").TryInt()))
                .ForMember(x => x.Difficulty,
                    opt => opt.ResolveUsing(src => GetValueWithFallback(src, "difficulty", "diff").TryInt()))
                .ForMember(x => x.GameState,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("gameState").TryInt()))
                .ForMember(x => x.VerifySignatures,
                    opt => opt.ResolveUsing(src => GetValueWithFallback(src, "verifySignatures", "verSig").TryInt()))
                .ForMember(x => x.SvBattleye,
                    opt => opt.ResolveUsing(src => GetValueWithFallback(src, "sv_battleye", "be").TryInt()))
                .ForMember(x => x.ReqBuild,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("reqBuild").TryIntNullable()))
                .ForMember(x => x.PasswordRequired,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("password").TryInt() > 0))
                .ForMember(x => x.Mission, opt => opt.ResolveUsing(src => src.GetSettingOrDefault("mission")))
                .ForMember(x => x.Island,
                    opt => opt.ResolveUsing(src => CapitalizeString(src.GetSettingOrDefault("mapname"))))
                .ForMember(x => x.GameType, opt => opt.ResolveUsing(src => {
                    var str = CapitalizeString(src.GetSettingOrDefault("gametype"));
                    return string.IsNullOrWhiteSpace(str) ? "Unknown" : str;
                }))
                .ForMember(x => x.GameName, opt => opt.ResolveUsing(src => src.GetSettingOrDefault("gamename")))
                .ForMember(x => x.GameVer,
                    opt => opt.ResolveUsing(src => GetVersion(src.GetSettingOrDefault("gamever"))))
                .ForMember(x => x.Signatures,
                    opt => opt.ResolveUsing(src => GetValueWithFallback(src, "signatures", "sig").TrySplit(';')))
                .AfterMap(GamespyAfterMap);
        }

        static string GetValueWithFallback(GamespyServerQueryResult src, params string[] possibilities) {
            return possibilities.Select(src.GetSettingOrDefault).FirstOrDefault(setting => setting != null);
        }

        static void SetupSource(ConfigurationStore mapConfig) {
            mapConfig.CreateMap<SourceServerQueryResult, Server>()
                .ForMember(x => x.NumPlayers,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("playerCount").TryInt()))
                .ForMember(x => x.MaxPlayers,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("playerMax").TryInt()))
                .ForMember(x => x.PasswordRequired,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("visibility").TryInt() > 0))
                .ForMember(x => x.Mission, opt => opt.ResolveUsing(src => src.GetSettingOrDefault("game")))
                .ForMember(x => x.Island,
                    opt => opt.ResolveUsing(src => CapitalizeString(src.GetSettingOrDefault("map"))))
                .ForMember(x => x.GameName, opt => opt.ResolveUsing(src => src.GetSettingOrDefault("folder")))
                .ForMember(x => x.GameVer,
                    opt => opt.ResolveUsing(src => GetVersion(src.GetSettingOrDefault("version"))))
                .AfterMap(SourceAfterMap);

            mapConfig.CreateMap<SourceMasterServerQueryResult, Server>()
                .ForMember(x => x.GameName, opt => opt.ResolveUsing(src => src.GetSettingOrDefault("folder")))
                .AfterMap(SourceAfterMap);
        }

        static void SetupSourceOld(ConfigurationStore mapConfig) {
            mapConfig.CreateMap<SourceServerQueryResult, Server>()
                .ForMember(x => x.NumPlayers,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("playerCount").TryInt()))
                .ForMember(x => x.MaxPlayers,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("playerMax").TryInt()))
                .ForMember(x => x.Difficulty,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("difficulty").TryInt()))
                //TODO: not in steam
                .ForMember(x => x.GameState,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("gameState").TryInt()))
                //TODO: not in steam
                .ForMember(x => x.VerifySignatures,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("verifySignatures").TryInt()))
                //TODO: not in steam
                .ForMember(x => x.SvBattleye,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("sv_battleye").TryInt()))
                //TODO: not in steam
                .ForMember(x => x.ReqBuild,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("requiredBuild").TryIntNullable()))
                .ForMember(x => x.PasswordRequired,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("visibility").TryInt() > 0))
                .ForMember(x => x.Mission, opt => opt.ResolveUsing(src => src.GetSettingOrDefault("map")))
                .ForMember(x => x.Island,
                    opt => opt.ResolveUsing(src => CapitalizeString(src.GetSettingOrDefault("island"))))
                .ForMember(x => x.GameType, opt => opt.ResolveUsing(src => {
                    var str = CapitalizeString(src.GetSettingOrDefault("gametype"));
                    return string.IsNullOrWhiteSpace(str) ? "Unknown" : str;
                }))
                //TODO: not in steam
                .ForMember(x => x.GameName, opt => opt.ResolveUsing(src => src.GetSettingOrDefault("folder")))
                .ForMember(x => x.GameVer,
                    opt => opt.ResolveUsing(src => GetVersion(src.GetSettingOrDefault("version"))))
                .ForMember(x => x.Signatures,
                    opt => opt.ResolveUsing(src => src.GetSettingOrDefault("keywords").TrySplit(',')))
                .AfterMap(SourceAfterMap);
        }

        static string CapitalizeString(string str) {
            return str == null ? null : str.Trim().UppercaseFirst();
        }

        static void GamespyAfterMap(ServerQueryResult src, Server dst) {
            dst.QueryMode = ServerQueryMode.Gamespy;
            if (!dst.ForceServerName || string.IsNullOrWhiteSpace(dst.Name))
                dst.Name = src.GetSettingOrDefault("hostname");

            dst.UpdatePing(src.Ping);
            dst.UpdateModInfo(src.GetSettingOrDefault("mod"));
            dst.HasBasicInfo = true;
        }

        static void SourceAfterMap(ServerQueryResult src, Server dst) {
            dst.QueryMode = ServerQueryMode.Steam;

            var port = src.GetSettingOrDefault("port").TryInt();
            if (port > 0 && port < IPEndPoint.MaxPort)
                dst.SetServerAddress(port);

            if (src.IsMasterResult)
                return;

            if (!dst.ForceServerName || string.IsNullOrWhiteSpace(dst.Name))
                dst.Name = src.GetSettingOrDefault("name");

            dst.UpdatePing(src.Ping);

            var tags = src.GetSettingOrDefault("keywords");
            if (tags != null)
                new SourceTagParser(tags, dst).HandleTags();

            dst.Signatures = GetList(src.Settings, "sigNames").ToArray();
            dst.UpdateModInfo(GetList(src.Settings, "modNames").ToArray());
            dst.HasBasicInfo = true;
        }

        static IEnumerable<string> GetList(IEnumerable<KeyValuePair<string, string>> dict, string keyWord) {
            var rx = GetRx(keyWord);
            return string.Join("", (from kvp in dict.Where(x => x.Key.StartsWith(keyWord))
                let w = rx.Match(kvp.Key)
                where w.Success
                select new {Index = w.Groups[1].Value.TryInt(), Total = w.Groups[2].Value.TryInt(), kvp.Value})
                .OrderBy(x => x.Index).SelectMany(x => x.Value))
                .Split(';')
                .Where(x => !string.IsNullOrWhiteSpace(x));
        }

        static Regex GetRx(string keyWord) {
            Regex rx;
            if (rxCache.TryGetValue(keyWord, out rx))
                return rx;
            return
                rxCache[keyWord] =
                    new Regex(@"^" + keyWord + @":([0-9]+)\-([0-9]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        static Version GetVersion(string version) {
            return version == null ? null : version.TryParseVersion();
        }

        class SourceTagParser
        {
            /*
Value in the string	 identifier	 value	 meaning
bt,	 b	 true	 BattleEye 
r120,	 r	 1.20	 RequiredVersion
n0,	 n	 0	 RequiredBuildNo
s1,	 s	 1	 ServerState
i2,	 i	 2	 Difficulty
mf,	 m	 false	 EqualModRequired
lf,	 l	 false	 Lock
vt,	 v	 true	 VerifySignatures
dt,	 d	 true	 Dedicated
ttdm	 t	 tdm	 GameType
g65545,	 g	 65545	 Language
c0-52,	 c	 long.=0 lat.=52	 LongLat
pw	 p	 Windows	 Platform
Example
gameTags = bt,r120,n0,s1,i2,mf,lf,vt,dt,ttdm,g65545,c0-52,pw,
*/
            readonly Server _server;
            readonly Dictionary<string, string> _settings;

            public SourceTagParser(string tags, Server server) {
                _settings = new Dictionary<string, string>();
                foreach (var t in tags.Split(',')) {
                    var key = JoinChar(t.Take(1));
                    _settings.Add(key, JoinChar(t.Skip(1)));
                    // TODO: HACK: workaround t game mode issue; tcti,coop,dm,ctf,ff,scont,hold,unknown,a&d,aas,c&h,rpg,tdm,tvt,ans,ie&e,hunt,koth,obj,rc,vip
                    if (key == "t")
                        break;
                }
                _server = server;
            }

            static string JoinChar(IEnumerable<char> enumerable) {
                return string.Join("", enumerable);
            }

            bool ParseBool(string key) {
                return _settings.ContainsKey(key) && _settings[key] == "t";
            }

            string ParseString(string key) {
                return _settings.ContainsKey(key) ? _settings[key] : null;
            }

            int? ParseInt(string key) {
                return _settings.ContainsKey(key) ? _settings[key].TryIntNullable() : null;
            }

            double? ParseDouble(string key) {
                return _settings.ContainsKey(key) ? _settings[key].TryDouble() : (double?) null;
            }

            public void HandleTags() {
                _server.VerifySignatures = ParseBool("v") ? 2 : 0;
                _server.SvBattleye = ParseBool("b") ? 1 : 0;
                _server.ReqBuild = ParseInt("n");
                _server.Difficulty = ParseInt("i").GetValueOrDefault(0);
                _server.IsDedicated = ParseBool("d");
                _server.PasswordRequired = ParseBool("l");
                _server.GameState = ParseInt("s").GetValueOrDefault(0);
                _server.GameType = ParseString("t");
                _server.ServerPlatform = ParseString("p") == "w" ? ServerPlatform.Windows : ServerPlatform.Linux;
                _server.RequiredVersion = ParseInt("r");
                _server.Language = ParseInt("g");
                _server.Coordinates = ParseCoordinates(ParseString("c"));
            }

            static Coordinates ParseCoordinates(string coordinates) {
                if (coordinates == null)
                    return null;
                var split = coordinates.Split('-');
                return new Coordinates(split[0].TryDouble(), split[1].TryDouble());
            }
        }
    }
}