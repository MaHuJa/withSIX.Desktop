// <copyright company="SIX Networks GmbH" file="RecentCollection.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DataContract(Name = "RecentModSet",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public class RecentCollection : PropertyChangedBase
    {
        [DataMember] readonly string _Name;
        [DataMember] readonly DateTime _on;
        Collection _collection;
        [DataMember] Guid _id;
        [Obsolete, DataMember] string _Uuid;

        public RecentCollection(Collection collection) {
            _collection = collection;
            _on = Tools.Generic.GetCurrentUtcDateTime;
            _id = collection.Id;
            _Name = collection.Name;
        }

        public Guid Id
        {
            get { return _id; }
        }
        public string Name
        {
            get { return _Name; }
        }
        public DateTime On
        {
            get { return _on; }
        }
        public Collection Collection
        {
            get { return _collection; }
            set { SetProperty(ref _collection, value); }
        }

        public bool Matches(Collection collection) {
            return collection != null && collection.Id == _id;
        }

        public string GetLaunchUrl() {
            return String.Format("pws://?mod_set={0}", _id);
        }

        [OnDeserialized]
        protected void OnDeserialized(StreamingContext context) {
            if (_Uuid != null) {
                Guid.TryParse(_Uuid, out _id);
                _Uuid = null;
            }
        }
    }
}