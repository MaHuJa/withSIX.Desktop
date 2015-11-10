// <copyright company="SIX Networks GmbH" file="ObjectSaveBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DataContract]
    public abstract class ObjectSaveBase<T> where T : class, IObjectTag
    {
        [DataMember] readonly string _key;

        protected ObjectSaveBase(T obj) {
            Contract.Requires<ArgumentNullException>(obj != null);
            Contract.Requires<ArgumentNullException>(obj.ObjectTag != null);
            _key = obj.ObjectTag;
        }

        internal string Key
        {
            get { return _key; }
        }
        // public T Obj { get; set; }

        public bool Matches(T obj) {
            return obj != null && _key.Equals(obj.ObjectTag, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}