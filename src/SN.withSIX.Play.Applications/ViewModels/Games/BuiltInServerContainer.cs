// <copyright company="SIX Networks GmbH" file="BuiltInServerContainer.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public class BuiltInServerContainer : BuiltInContainer<Server>
    {
        public BuiltInServerContainer(string name) : base(name) {}
    }
}