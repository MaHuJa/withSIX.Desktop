// <copyright company="SIX Networks GmbH" file="ISupportContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Core.Games.Entities
{
    public interface ISupportContent : IHaveInstalledState
    {
        ContentPaths PrimaryContentPath { get; }
    }
}