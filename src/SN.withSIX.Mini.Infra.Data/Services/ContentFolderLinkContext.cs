using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Core;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Infra.Data.Services
{
    public class ContentFolderLinkContext : IContentFolderLinkContext
    {
        private readonly IAbsoluteFilePath _path;
        private bool _loaded;

        public ContentFolderLinkContext(IAbsoluteFilePath path) {
            _path = path;
        }

        ContentFolderLink FolderLink { get; set; }

        public async Task<ContentFolderLink> Load() {
            if (_loaded)
                throw new InvalidOperationException("Already loaded");
            FolderLink = _path.Exists
                ? await LoadJsonFromFileAsync().ConfigureAwait(false)
                : new ContentFolderLink(new List<FolderInfo>());
            _loaded = true;
            return FolderLink;
        }

        public Task Save() {
            if (!_loaded)
                throw new InvalidOperationException("Should be loaded before saving..");
            var dto = new ContentFolderLinkDTO {
                Folders = FolderLink.Infos.ToDictionary(x => x.Path.ToString(), x => x.ContentInfo)
            };
            return Task.Run(() => Tools.Serialization.Json.SaveJsonToDiskThroughMemory(dto, _path));
        }

        private async Task<ContentFolderLink> LoadJsonFromFileAsync() {
            var dto =
                await Tools.Serialization.Json.LoadJsonFromFileAsync<ContentFolderLinkDTO>(_path).ConfigureAwait(false);
            return new ContentFolderLink(dto.Folders.Select(x => new FolderInfo(x.Key.ToAbsoluteDirectoryPath(), x.Value)).ToList());
        }
    }

    public class ContentFolderLinkDTO
    {
        public Dictionary<string, ContentInfo> Folders { get; set; }
        //public Dictionary<Guid, FolderInfo2> FolderInfo2 { get; set; } = new Dictionary<Guid, FolderInfo2>();
    }

    /*    public class FolderInfo2
        {
            public IAbsoluteDirectoryPath Folder { get; set; }
            public Guid UserId { get; set; }
            public Guid GameId { get; set; }
        }*/
}