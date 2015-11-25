using System.Threading.Tasks;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Applications.Services.Infra
{
    public interface IContentFolderLinkContext {
        Task<ContentFolderLink> Load();
        Task Save();
    }
}