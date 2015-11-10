// <copyright company="SIX Networks GmbH" file="Player.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Games.Legacy.Servers
{
    public abstract class Player : PropertyChangedBase, IComparePK<Player>
    {
        protected Player(Server server, string name) {
            Server = server;
            Name = name;
        }

        public Server Server { get; }
        public string Name { get; }

        public virtual bool ComparePK(object obj) {
            var emp = obj as Player;
            if (emp != null)
                return ComparePK(emp);
            return false;
        }

        public bool ComparePK(Player other) {
            if (other == null)
                return false;
            if (ReferenceEquals(other, this))
                return true;

            // Otherwise we break syncing based on gamespy vs source
            if (other.GetType() != GetType())
                return false;

            return other.Name != null && other.Name.Equals(Name);
        }
    }
}