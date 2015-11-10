// <copyright company="SIX Networks GmbH" file="SshKeyPair.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Sync.Core.Keys
{
    public class SshKeyPair : PropertyChangedBase
    {
        public const int DefaultBits = 2048;
        public const string DefaultType = "rsa";
        readonly string _privateFile;
        readonly string _publicFile;
        bool _isSelected;

        public SshKeyPair(string path) {
            Contract.Requires<ArgumentNullException>(path != null);
            Location = Path.GetDirectoryName(path);
            Name = Path.GetFileName(path);
            CreatedAt = File.GetCreationTime(path);
            _privateFile = path;
            _publicFile = path + ".pub";
            PublicKeyData = File.ReadAllText(_publicFile);
        }

        public DateTime CreatedAt { get; set; }
        public string Name { get; protected set; }
        public string Location { get; protected set; }
        public string PublicKeyData { get; protected set; }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
    }
}