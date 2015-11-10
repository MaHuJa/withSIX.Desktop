// <copyright company="SIX Networks GmbH" file="Mirror.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    [DoNotObfuscate]
    public class Mirror : SyncBase
    {
        Network _network;
        Guid _networkId;
        Uri _url;
        public Mirror(Guid id) : base(id) {}
        public Guid NetworkId
        {
            get { return _networkId; }
            set { SetProperty(ref _networkId, value); }
        }
        public Uri Url
        {
            get { return _url; }
            set
            {
                if (SetProperty(ref _url, value)) {
                    Address = value == null ? null : value.Host;
                    OnPropertyChanged("Address");
                }
            }
        }
        public string Address { get; private set; }
        public Network Network
        {
            get { return _network; }
            set
            {
                if (!SetProperty(ref _network, value))
                    return;
                NetworkId = value.Id;
            }
        }
    }
}