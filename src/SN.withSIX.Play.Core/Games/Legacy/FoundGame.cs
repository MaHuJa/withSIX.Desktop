// <copyright company="SIX Networks GmbH" file="FoundGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.IO;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public class FoundGame : PropertyChangedBase, IComparePK<FoundGame>
    {
        bool _isSelected;

        public FoundGame(string path) {
            Path = path;
            FileName = System.IO.Path.GetFileNameWithoutExtension(path);
            CreatedAt = File.GetCreationTime(path);
        }

        public string FileName { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public string Path { get; protected set; }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public bool ComparePK(object other) {
            var o = other as FoundGame;
            return o != null && ComparePK(o);
        }

        public bool ComparePK(FoundGame other) {
            return other != null && Path.Equals(other.Path);
        }

        public override string ToString() {
            return Path;
        }
    }
}