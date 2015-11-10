// <copyright company="SIX Networks GmbH" file="ModelBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace SN.withSIX.Core.Helpers
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public abstract class ModelBase : PropertyChangedBase
    {
        DateTime _createdAt;
        DateTime? _updatedAt;

        protected ModelBase() {
            CreatedAt = DateTime.UtcNow;
        }

        [DataMember]
        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set { SetProperty(ref _createdAt, value); }
        }
        [DataMember]
        public DateTime? UpdatedAt
        {
            get { return _updatedAt; }
            set { SetProperty(ref _updatedAt, value); }
        }
    }
}