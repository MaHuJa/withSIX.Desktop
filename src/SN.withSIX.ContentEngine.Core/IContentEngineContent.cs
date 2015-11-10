// <copyright company="SIX Networks GmbH" file="IContentEngineContent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using NDepend.Path;
using Newtonsoft.Json;

namespace SN.withSIX.ContentEngine.Core
{
    public interface IContentEngineContent
    {
        Guid NetworkId { get; }
        Guid Id { get; }
        bool IsInstalled { get; }
        IAbsoluteDirectoryPath PathInternal { get; }
        string Path { get; }
        Guid GameId { get; }
    }

    public class ContentEngineContent : IContentEngineContent
    {
        public ContentEngineContent(Guid networkId, Guid id, bool isInstalled, IAbsoluteDirectoryPath path, Guid gameId) {
            NetworkId = networkId;
            Id = id;
            IsInstalled = isInstalled;
            PathInternal = path;
            GameId = gameId;
        }

        public Guid NetworkId { get; }
        public Guid Id { get; }
        public bool IsInstalled { get; }
        [JsonIgnore]
        public IAbsoluteDirectoryPath PathInternal { get; }
        // JSON 7.0.1 workaround :S
        // TODO: We can remove this again once we drop the Mediator being used for Factoring CE services...
        public string Path => PathInternal?.ToString();
        public Guid GameId { get; }
    }
}