// <copyright company="SIX Networks GmbH" file="SourceServerQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SN.withSIX.Api.Models;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Play.Core.Games.Legacy.ServerQuery
{
    public class SourceServerQuery : ServerQueryBase, IServerQuery, IEnableLogging
    {
        static readonly byte[] emptyChallenge = {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
        static readonly byte[] a2SPlayerRequestH = {0xFF, 0xFF, 0xFF, 0xFF, 0x55};
        static readonly byte[] a2SRulesRequestH = {0xFF, 0xFF, 0xFF, 0xFF, 0x56};
        static readonly byte[] a2SInfoRequest = {
            0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20,
            0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00
        };
        readonly ServerAddress _address;
        readonly IDictionary<int, byte[][]> _bzipDict;
        //static readonly string[] altGames = {}; //FUTURE: some games may need special processing
        readonly IServerQueryParser _parser;
        readonly IDictionary<int, byte[][]> _plainDict;
        IPEndPoint _remoteEp;
        ServerQueryState _state;

        public SourceServerQuery(ServerAddress address, string serverBrowserTag, IServerQueryParser parser) {
            Contract.Requires<ArgumentNullException>(address != null);
            Contract.Requires<ArgumentNullException>(serverBrowserTag != null);
            Contract.Requires<ArgumentNullException>(parser != null);

            _address = address;
            _parser = parser;
            _plainDict = new Dictionary<int, byte[][]>(10);
            _bzipDict = new Dictionary<int, byte[][]>(10);
        }

        public Task UpdateAsync(ServerQueryState state) {
            state.MaxPackets = 3;
            state.Client = Connect(state.EndPoint);
            _state = state;

            return FetchAsync();
        }

        UdpClient Connect(IPEndPoint e) {
            var udpClient = new UdpClient(e) {
                Client = {ReceiveTimeout = DefaultReceiveTimeout, SendTimeout = DefaultSendTimeout}
            };
            //udpClient.Client.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.Linger, new LingerOption(true, 0));
            _remoteEp = new IPEndPoint(_address.IP, _address.Port);
            udpClient.Connect(_remoteEp);
            return udpClient;
        }

        async Task FetchAsync() {
            try {
                await ProcessAllPackets().ConfigureAwait(false);

                _state.ProcessParser(_parser);
                if (_state.Success)
                    _state.Server.UpdateInfoFromResult(_state.Result);
            } catch (TimeoutException e) {
                /*
#if DEBUG
                this.Logger().FormattedDebugException(e, "Swallowed");
#endif
*/
                _state.Exception = e;
                _state.Cancel();
            } catch (ObjectDisposedException e) {
                /*
#if DEBUG
                this.Logger().FormattedDebugException(e, "Swallowed");
#endif
*/
                _state.Exception = e;
                _state.Cancel();
            } catch (Exception e) {
                _state.Exception = e;
                _state.Cancel();
                throw;
            } finally {
                _state.StopSW();
            }
        }

        async Task ProcessAllPackets() {
            _state.StartSW();
            _state.ReceivedPackets[0] = await GetInfo().ConfigureAwait(false);
            _state.StartSW();
            _state.ReceivedPackets[1] = await GetPlayers().ConfigureAwait(false);
            _state.StartSW();
            _state.ReceivedPackets[2] = await GetRules().ConfigureAwait(false);
        }

        async Task<byte[]> GetInfo() {
            await SendPacket(a2SInfoRequest).ConfigureAwait(false);
            var response = await ReceiveTillEnd().ConfigureAwait(false);
            if (response[4] != 0x49)
                throw new Exception(String.Format("Wrong packet tag: 0x{0:X} Expected: 0x49.", response[4]));
            return response;
        }

        async Task<byte[]> GetPlayers() {
            var response = await GetChallengeResponse(0x55).ConfigureAwait(false);
            switch (response[4]) {
            case 0x41: //challenge
                await SendPacket(GetPlayerRequestPacket(response.Skip(5).ToArray())).ConfigureAwait(false);
                return await ReceiveTillEnd().ConfigureAwait(false);
            case 0x44: //no challenge needed info
                return response;
            default:
                throw new Exception(String.Format("Wrong packet tag: 0x{0:X} Expected: 0x41 or 0x44.", response[4]));
            }
        }

        static byte[] GetPlayerRequestPacket(IEnumerable<byte> challengeBytes) {
            return a2SPlayerRequestH.Concat(challengeBytes).ToArray();
        }

        async Task<byte[]> GetRules() {
            var response = await GetChallengeResponse(0x56).ConfigureAwait(false);
            switch (response[4]) {
            case 0x41: //challenge
                await SendPacket(GetRulesRequestPacket(response.Skip(5).ToArray())).ConfigureAwait(false);
                return await ReceiveTillEnd().ConfigureAwait(false);
            case 0x45: //no challenge needed info
                return response;
            default:
                throw new Exception(String.Format("Wrong packet tag: 0x{0:X} Expected: 0x41 or 0x45.", response[4]));
            }
        }

        static byte[] GetRulesRequestPacket(IEnumerable<byte> challengeBytes) {
            return a2SRulesRequestH.Concat(challengeBytes).ToArray();
        }

        async Task<byte[]> GetChallengeResponse(byte b) {
            emptyChallenge[4] = b;
            await SendPacket(emptyChallenge).ConfigureAwait(false);
            var response = await _state.Client.ReceiveWithTimeoutAfter(DefaultReceiveTimeout).ConfigureAwait(false);
            _state.StopSW();
            return response.Buffer;
        }

        Task SendPacket(byte[] msg) {
            return _state.Client.SendWithTimeoutAfter(msg, DefaultSendTimeout);
        }

        async Task<byte[]> ReceiveTillEnd() {
            // TODO: Keep looping for multi packets (process each header to find if there are more)
            // and concatenate them in the right order
            var response = await _state.Client.ReceiveWithTimeoutAfter(DefaultReceiveTimeout).ConfigureAwait(false);
            _state.StopSW();
            return ProcessPacketHeader(response.Buffer) ?? await ReceiveTillEnd().ConfigureAwait(false);
        }

        byte[] ProcessPacketHeader(byte[] reply) {
            Contract.Requires<ArgumentNullException>(reply != null);

            var pos = 0;
            byte[] header = {0xFF, 0xFF, 0xFF};
            var match = header.All(b => reply[pos++] == b);
            if (!match)
                return null;

            if (reply[pos] == 0xFF)
                return reply;
            if (reply[pos] == 0xFE)
                return CollectPacket(reply, ++pos);
            return null;
        }

        byte[] CollectPacket(byte[] reply, int pos) {
            //TODO: test if this works on old source server or mockup
            var id = BitConverter.ToInt32(reply, pos);
            pos += 4;
            var bzipped = (id & 0x80000000) != 0;
            var total = reply[pos++];
            var number = reply[pos++];
            pos += 2; //ignoring next short
            if (bzipped) {
                throw new Exception("bzip2 compression not implemented");
                /*
                if (!_bzipDict.ContainsKey(id))
                {
                    _bzipDict[id] = new byte[total + 2][];
                    _bzipDict[id][total] = new byte[1];
                }
                if (number == 0)
                {
                    _bzipDict[id][total + 1] = reply.Skip(pos).Take(4).ToArray();
                    pos += 4;
                }
                _bzipDict[id][number] = reply.Skip(pos).ToArray();
                var runTotal = _bzipDict[id][total][0]++;
                if (runTotal == total) JoinPackets(id, total, true);
                 */
            }
            //else
            if (!_plainDict.ContainsKey(id)) {
                _plainDict[id] = new byte[total + 1][];
                _plainDict[id][total] = new byte[1];
            }
            _plainDict[id][number] = reply.Skip(pos).ToArray();
            var runTotal = _plainDict[id][total][0]++;
            if (runTotal == total)
                return JoinPackets(id, total, false);

            return null;
        }

        byte[] JoinPackets(int id, byte total, bool bzipped) {
            var multiPkt = bzipped ? _bzipDict[id] : _plainDict[id];
            var len = multiPkt.Sum(pkt => pkt.Length);
            for (var i = total; i < multiPkt.Length; i++)
                len -= multiPkt[i].Length;
            var replyPkt = new byte[len];
            var pos = 0;
            for (var i = 0; i < total; i++) {
                Buffer.BlockCopy(multiPkt[i], 0, replyPkt, pos, multiPkt[i].Length);
                pos += multiPkt[i].Length;
            }
            if (bzipped) {
                //var input = new MemoryStream(replyPkt);
                //var output = new MemoryStream();
                //BZip2.Decompress(input, output, true);
                //replyPkt = output.ToArray();
            }
            var valid = true;
            if (total + 1 < multiPkt.Length) {
                //var size = BitConverter.ToInt32(multiPkt[total + 1], 0);
                //var crc = BitConverter.ToInt32(multiPkt[total + 1], 4);
                //if (replyPkt.Length * 8 != size) valid = false;
                //var checkCrc = new Crc32();
                //checkCrc.Update(replyPkt);
                //if (checkCrc.Value != crc) valid = false;
            }

            if (!valid) {
                throw new Exception("split packet not decompressed properly");
                //TODO: check if at least header is intact so query can be retried
            }
            return replyPkt;
        }
    }
}