// <copyright company="SIX Networks GmbH" file="RecentBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DataContract]
    public abstract class RecentBase<T> : ObjectSaveBase<T> where T : class, IRecent
    {
        [DataMember] readonly DateTime _on;

        protected RecentBase(T obj)
            : base(obj) {
            _on = Tools.Generic.GetCurrentUtcDateTime;
        }

        public DateTime On
        {
            get { return _on; }
        }
    }
}