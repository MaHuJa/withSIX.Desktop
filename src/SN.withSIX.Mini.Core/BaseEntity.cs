// <copyright company="SIX Networks GmbH" file="BaseEntity.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Core;
using SN.withSIX.Core.Services.Infrastructure;

namespace SN.withSIX.Mini.Core
{
    [DataContract]
    public abstract class BaseEntity
    {
        [IgnoreDataMember] readonly IDomainEventHandler _domainEventHandler = CoreCheat.EventGrabber.Get();

        protected void PrepareEvent(IDomainEvent evt) {
            _domainEventHandler.PrepareEvent(this, evt);
        }
    }

    [DataContract]
    public abstract class BaseEntity<T> : BaseEntity, IHaveId<T>
    {
        [DataMember]
        public T Id { get; set; }
    }

    [DataContract]
    public abstract class BaseEntityGuidId : BaseEntity, IHaveId<Guid>
    {
        protected BaseEntityGuidId() {
            Id = Guid.NewGuid();
        }

        [DataMember]
        public Guid Id { get; protected set; }
    }
}