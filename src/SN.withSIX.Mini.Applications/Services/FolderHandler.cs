using NDepend.Path;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Mini.Applications.Services
{
    public class FolderHandler : IApplicationService, IFolderHandler
    {
        public IAbsoluteDirectoryPath Folder { get; set; }
    }

    public interface IFolderHandler
    {
        IAbsoluteDirectoryPath Folder { get; set; }
    }
}