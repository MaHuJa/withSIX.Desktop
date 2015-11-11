using System;
using System.Threading.Tasks;

namespace SN.withSIX.Mini.Presentation.Wpf.Node
{
    public class Startup
    {
        public async Task<object> Invoke(object input) {
            // doing this will stop the return of the promise...
            //await Task.Run(() => ByOther());
            ByOther();
            return true;
        }

        public void ByDomain(string path) {
            var dom = AppDomain.CreateDomain("MyDomain", null, path, String.Empty, false);
            dom.ExecuteAssemblyByName("Sync");
        }

        public void ByOther() {
            Entrypoint.MainForNode();
        }
    }
}