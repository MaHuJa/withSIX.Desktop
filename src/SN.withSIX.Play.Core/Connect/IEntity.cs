// <copyright company="SIX Networks GmbH" file="IEntity.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Core.Connect
{
    public interface IEntity : IContact
    {
        Uri Avatar { get; set; }
        string Slug { get; set; }
    }
}