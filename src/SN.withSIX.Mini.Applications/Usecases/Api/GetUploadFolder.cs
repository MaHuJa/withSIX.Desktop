using System;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using ShortBus;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Mini.Applications.Attributes;
using SN.withSIX.Mini.Applications.Services.Infra;

namespace SN.withSIX.Mini.Applications.Usecases.Api
{
    [ApiUserAction]
    public class GetUploadFolder : IAsyncQuery<IAbsoluteDirectoryPath>
    {
        public GetUploadFolder(Guid contentId) {
            ContentId = contentId;
        }

        public Guid ContentId { get; }
    }

    public class GetUploadFolderHandler : ApiDbQueryBase, IAsyncRequestHandler<GetUploadFolder, IAbsoluteDirectoryPath>
    {
        public GetUploadFolderHandler(IDbContextLocator dbContextLocator) : base(dbContextLocator) {}

        public async Task<IAbsoluteDirectoryPath> HandleAsync(GetUploadFolder request) {
            var cl = await ContentLinkContext.Load().ConfigureAwait(false);
            // TODO: or throw NotFoundException ?
            return cl.Infos.FirstOrDefault(x => x.ContentInfo.ContentId == request.ContentId)?.Path;
        }
    }
}