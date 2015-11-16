using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace SN.withSIX.Mini.Presentation.Shell
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Directory)]
    public class SyncShellExtension : SharpContextMenu
    {
        /// <summary>
        ///     Determines whether this instance can a shell
        ///     context show menu, given the specified selected file list.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance should show a shell context
        ///     menu for the specified file list; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanShowMenu() {
            return SelectedItemPaths.All(Helper.IsKnownToSync);
        }

        /// <summary>
        ///     Creates the context menu. This can be a single menu item or a tree of them.
        /// </summary>
        /// <returns>
        ///     The context menu for the shell context menu.
        /// </returns>
        protected override ContextMenuStrip CreateMenu() {
            //  Create the menu strip.
            var menu = new ContextMenuStrip();

            BuildSync(menu);

            //  Return the menu.
            return menu;
        }

        void BuildSync(ToolStrip menu) {
            var itemCountLines = new ToolStripMenuItem {
                Text = "Sync..."
                //Image = Properties.Resources.CountLines
            };
            itemCountLines.Click += (sender, args) => Sync();
            menu.Items.Add(itemCountLines);
        }

        void Sync() {
            //  Builder for the output.
            var builder = new StringBuilder();

            //  Go through each file.
            foreach (var filePath in SelectedItemPaths) {
                //  Count the lines.
                builder.AppendLine(
                    $"{Path.GetFileName(filePath)} - {filePath.Length} Characters (Sync) {Helper.IsKnownToSync(filePath)}");
            }

            //  Show the ouput.
            MessageBox.Show(builder.ToString());
        }
    }
}