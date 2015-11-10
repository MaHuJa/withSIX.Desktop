// <copyright company="SIX Networks GmbH" file="TransferBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using SN.withSIX.Sync.Core.Transfer.Protocols;

namespace SN.withSIX.Sync.Core.Transfer
{
    public abstract class TransferBase<T> where T : IProtocol
    {
        readonly Dictionary<string, T> _strategies = new Dictionary<string, T>();

        protected void RegisterProtocolStrategies(IEnumerable<T> strategies) {
            foreach (var strategy in strategies)
                RegisterProtocolStrategy(strategy);
        }

        protected void RegisterProtocolStrategy(T protocol) {
            foreach (var scheme in protocol.Schemes)
                AddOrUpdateStrategy(protocol, scheme);
        }

        void AddOrUpdateStrategy(T protocol, string scheme) {
            if (_strategies.ContainsKey(scheme))
                _strategies[scheme] = protocol;
            else
                _strategies.Add(scheme, protocol);
        }

        protected void ConfirmStrategySupported(Uri uri) {
            if (!_strategies.ContainsKey(uri.Scheme))
                throw new ProtocolNotSupported(uri.Scheme);
        }

        protected T GetStrategy(Uri uri) {
            return _strategies[uri.Scheme];
        }
    }
}