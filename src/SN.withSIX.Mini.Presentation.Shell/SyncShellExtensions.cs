using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NDepend.Path;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SN.withSIX.Api.Models;

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
            return SelectedItemPaths.Select(x => x.ToAbsoluteDirectoryPath()).All(Helper.TryIsKnownToSync);
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

        async void Sync() {
            //  Go through each file.
            foreach (var filePath in SelectedItemPaths.Select(x => x.ToAbsoluteDirectoryPath())) {
                var info = await Helper.GetFolderInfo(filePath).ConfigureAwait(false);
                // TODO: slug for other games
                Process.Start("http://withsix.com/p/Arma-3/mods/" + new ShortGuid(info.ContentInfo.ContentId) +
                              "?upload=1");
            }
        }
    }
}