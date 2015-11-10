// <copyright company="SIX Networks GmbH" file="Network.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    [DoNotObfuscate]
    public class Network : SyncBase
    {
        List<Mirror> _mirrors = new List<Mirror>();
        string _name;
        public Network(Guid id) : base(id) {}
        public List<Mirror> Mirrors
        {
            get { return _mirrors; }
            set { SetProperty(ref _mirrors, value); }
        }
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public int? MaxThreads { get; set; }
    }
}