// <copyright company="SIX Networks GmbH" file="FavoriteCollection.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DataContract(Name = "FavoriteModSet",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class FavoriteCollection
    {
        [DataMember] readonly string _name;

        public FavoriteCollection(Collection collection) {
            _name = collection.Name;
        }

        public Collection Collection { get; private set; }

        public bool Matches(Collection collection) {
            return collection != null && collection.Name == _name;
        }
    }
}