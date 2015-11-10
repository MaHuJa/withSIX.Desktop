// <copyright company="SIX Networks GmbH" file="HostChecker.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SN.withSIX.Core.Services.Infrastructure;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Sync.Core.Legacy
{
    public interface IHostChecker
    {
        HostType GetHostType(Uri host);
        IEnumerable<Uri> SortAndValidateHosts(IEnumerable<Uri> hosts);
        bool ValidateHost(Uri host);
    }

    public interface IHostCheckerWithPing : IHostChecker {}

    public enum HostCheckerType
    {
        Default,
        WithPing
    }

    public class HostChecker : IHostChecker
    {
        static readonly Dictionary<string, HostType> protoStringToHostType =
            new Dictionary<string, HostType> {
                {"rsync", HostType.Rsync},
                {"zsync", HostType.Zsync},
                {"zsyncs", HostType.Zsync},
                {"http", HostType.Http},
                {"https", HostType.Http},
                {"ftp", HostType.Ftp},
                {"ssh", HostType.Ssh},
                {"file", HostType.Local}
                //{"torrent", HostType.Torrent}
            };
        readonly ProtocolPreference _protocolPreference;

        public HostChecker(Func<ProtocolPreference> protocolPreference) {
            _protocolPreference = protocolPreference();
        }

        public HostType GetHostType(Uri host) {
            return protoStringToHostType.ContainsKey(host.Scheme)
                ? protoStringToHostType[host.Scheme]
                : HostType.Unknown;
        }

        public IEnumerable<Uri> SortAndValidateHosts(IEnumerable<Uri> hosts) {
            return SortHosts(ValidateHosts(hosts.Distinct()));
        }

        public bool ValidateHost(Uri host) {
            return true; // Disabled for now..
            switch (_protocolPreference) {
            case ProtocolPreference.Any:
                return true;
            case ProtocolPreference.PreferRsync:
                return true;
            case ProtocolPreference.PreferZsync:
                return true;
            case ProtocolPreference.ZsyncOnly:
                return GetHostType(host) == HostType.Zsync
                       || GetHostType(host) == HostType.Http;
            case ProtocolPreference.RsyncOnly:
                return GetHostType(host) == HostType.Rsync;
            default:
                return false;
            }
        }

        IEnumerable<Uri> ValidateHosts(IEnumerable<Uri> hosts) {
            return hosts.Where(ValidateHost);
        }

        protected virtual IOrderedEnumerable<Uri> SortHosts(IEnumerable<Uri> hosts) {
            return SortHostsInternal(hosts).ThenByDescending(x => x.Host.Contains("-p.")); // premium first...
        }

        IOrderedEnumerable<Uri> SortHostsInternal(IEnumerable<Uri> hosts) {
            switch (_protocolPreference) {
            case ProtocolPreference.PreferRsync:
                return OrderByRsync(hosts);
            case ProtocolPreference.PreferZsync:
                return OrderByZsync(hosts);
            case ProtocolPreference.RsyncOnly:
                return OrderByRsync(hosts);
            case ProtocolPreference.ZsyncOnly:
                return OrderByZsync(hosts);
            default:
                return hosts.OrderBy(x => 0);
            }
        }

        IOrderedEnumerable<Uri> OrderByRsync(IEnumerable<Uri> hosts) {
            return hosts
                .OrderBy(x => GetHostType(x) == HostType.Rsync ? 0 : 1);
        }

        IOrderedEnumerable<Uri> OrderByZsync(IEnumerable<Uri> hosts) {
            return hosts
                .OrderBy(x => GetHostType(x) == HostType.Zsync || GetHostType(x) == HostType.Http ? 0 : 1);
        }
    }

    public class HostCheckerWithPing : HostChecker, IHostCheckerWithPing
    {
        readonly ConcurrentDictionary<string, long> _pingCache;
        readonly IPingProvider _pingProvider;

        public HostCheckerWithPing(Func<ProtocolPreference> protocolPreference, IPingProvider pingProvider)
            : base(protocolPreference) {
            _pingProvider = pingProvider;

            _pingCache = new ConcurrentDictionary<string, long>();
        }

        protected override IOrderedEnumerable<Uri> SortHosts(IEnumerable<Uri> hosts) {
            return base.SortHosts(hosts).ThenBy(Ping);
        }

        long Ping(Uri x) {
            if (_pingCache.ContainsKey(x.DnsSafeHost))
                return _pingCache[x.DnsSafeHost];

            return _pingCache[x.DnsSafeHost] = _pingProvider.Ping(x.DnsSafeHost);
        }
    }
}