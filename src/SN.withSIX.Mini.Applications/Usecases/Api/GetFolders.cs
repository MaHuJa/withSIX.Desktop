using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Mini.Applications.Services.Infra;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    public class GetFolders : IAsyncRequest<List<FolderInfo>>
    {
        public GetFolders(List<string> folders) {
            Folders = folders;
        }

        public List<string> Folders { get; set; }
    }

    public class GetFoldersHandler : ApiDbQueryBase, IAsyncRequestHandler<GetFolders, List<FolderInfo>>
    {
        public GetFoldersHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<List<FolderInfo>> HandleAsync(GetFolders request) {
            var cl = await ContentLinkContext.Load().ConfigureAwait(false);
            return cl.Infos.Where(x => request.Folders.Contains(x.Path.ToString())).ToList();
        }
    }
}