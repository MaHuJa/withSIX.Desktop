using System.Threading.Tasks;

namespace SN.withSIX.Mini.Presentation.Wpf.Node
{
    public class Teardown
    {
        public async Task<object> Invoke(object input) {
            Entrypoint.ExitForNode();
            return true;
        }
    }
}