// <copyright company="SIX Networks GmbH" file="SyncBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public interface ISyncBase : IHaveId<Guid>
    {
        Guid Id { get; }
        [DataMember]
        DateTime CreatedAt { get; set; }
        [DataMember]
        DateTime? UpdatedAt { get; set; }
        bool ComparePK(object obj);
        bool ComparePK(SyncBase other);
        event PropertyChangedEventHandler PropertyChanged;
        void Refresh();
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public abstract class SyncBase : ModelBase, IHaveTimestamps, IComparePK<SyncBase>, ISyncBase
    {
        [DataMember] Guid _id;
        [Obsolete, DataMember] protected string _Uuid;

        protected SyncBase(Guid id) {
            _id = id;
        }

        public virtual bool ComparePK(object obj) {
            var emp = obj as SyncBase;
            return emp != null && ComparePK(emp);
        }

        public bool ComparePK(SyncBase other) {
            if (other == null)
                return false;
            if (ReferenceEquals(other, this))
                return true;

            if (other.Id == Guid.Empty || Id == Guid.Empty)
                return false;

            return other.Id == Id;
        }

        // BWC
        public Guid Id
        {
            get { return _id; }
        }

        protected static string GetApiPath(string type) {
            return Tools.Transfer.JoinPaths("api/v1", type);
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            if (_Uuid != null) {
                Guid.TryParse(_Uuid, out _id);
                _Uuid = null;
            }
            if (_id == Guid.Empty)
                _id = Guid.NewGuid();
        }
    }
}