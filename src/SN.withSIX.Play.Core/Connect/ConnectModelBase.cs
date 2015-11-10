// <copyright company="SIX Networks GmbH" file="ConnectModelBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Connect
{
    public abstract class ConnectModelBase : PropertyChangedBase, IHaveGuidId, IComparePK<IHaveGuidId>
    {
        protected ConnectModelBase(Guid id) {
            Id = id;
        }

        public bool ComparePK(object other) {
            var otherAs = other as IHaveGuidId;
            return otherAs != null && ComparePK(otherAs);
        }

        public bool ComparePK(IHaveGuidId other) {
            return Id.Equals(other.Id);
        }

        public Guid Id { get; }
    }
}