using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NDepend.Path;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Infra.Data.Services;

namespace SN.withSIX.Mini.Presentation.Shell
{
    public class Helper
    {
        public static bool IsNotKnownToSync(IAbsoluteDirectoryPath x) => !IsKnownToSync(x);

        public static bool TryIsNotKnownToSync(IAbsoluteDirectoryPath x) => !TryIsKnownToSync(x);
        // TODO: Probably should have a global registry per user account somewhere e.g in a json file instead?
        public static bool IsKnownToSync(IAbsoluteDirectoryPath x) => GetFolderInfo(x).Result != null;

        public static bool TryIsKnownToSync(IAbsoluteDirectoryPath x) {
            try {
                return IsKnownToSync(x);
            } catch {
                return false;
            }
        }

        public static async Task<FolderInfo> GetFolderInfo(IAbsoluteDirectoryPath path) {
            var d = await GetFolderInfo().ConfigureAwait(false);
            return d.Infos.FirstOrDefault(x => x.Path.Equals(path));
        }

        // TODO: Cache for a minute or so? or somehow know when changes are available (file time?)
        private static async Task<ContentFolderLink> GetFolderInfo() {
            var ctx = new ContentFolderLinkContext("".ToAbsoluteFilePath());
            var d = await ctx.Load().ConfigureAwait(false);
            return d;
        }
    }
}