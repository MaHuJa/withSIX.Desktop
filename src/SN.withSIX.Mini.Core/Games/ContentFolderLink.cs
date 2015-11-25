using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NDepend.Path;

namespace SN.withSIX.Mini.Core.Games
{
    public class ContentInfo
    {
        public ContentInfo(Guid userId, Guid gameId, Guid contentId) {
            Contract.Requires<ArgumentOutOfRangeException>(userId != Guid.Empty);
            Contract.Requires<ArgumentOutOfRangeException>(gameId != Guid.Empty);
            Contract.Requires<ArgumentOutOfRangeException>(contentId != Guid.Empty);
            UserId = userId;
            GameId = gameId;
            ContentId = contentId;
        }

        public Guid UserId { get; }
        public Guid GameId { get; }
        public Guid ContentId { get; }
    }

    public class FolderInfo
    {
        public FolderInfo(IAbsoluteDirectoryPath path, ContentInfo contentInfo) {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentNullException>(contentInfo != null);
            Path = path;
            ContentInfo = contentInfo;
        }

        public IAbsoluteDirectoryPath Path { get; }
        public ContentInfo ContentInfo { get; set; }
    }

    public class ContentFolderLink
    {
        public ContentFolderLink(List<FolderInfo> info) {
            Contract.Requires<ArgumentNullException>(info != null);
            Infos = info;
        }

        public List<FolderInfo> Infos { get; }
    }
}