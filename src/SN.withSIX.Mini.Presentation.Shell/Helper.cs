using System.IO;

namespace SN.withSIX.Mini.Presentation.Shell
{
    public class Helper
    {
        public static bool IsNotKnownToSync(string x) => !IsKnownToSync(x);
        // TODO: Probably should have a global registry per user account somewhere e.g in a json file instead?
        public static bool IsKnownToSync(string x) => File.Exists(Path.Combine(x, ".sync.txt"));
    }
}