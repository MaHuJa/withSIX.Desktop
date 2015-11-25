using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NDepend.Path;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SN.withSIX.Api.Models;

namespace SN.withSIX.Mini.Presentation.Shell
{
    // Unregister with /unregister respectively
    // Caveats:
    // Needs restart of explorer.exe
    // Probably should be installed to a global path, and then only updated if the md5 of the dll doesnt match.
    // in that case, kill explorer.exe, unregister, update, register, restart explorer...
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Directory)]
    public class UploadShellExtension : SharpContextMenu
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
            //  We always show the menu.
            return SelectedItemPaths.Select(x => x.ToAbsoluteDirectoryPath()).All(Helper.TryIsNotKnownToSync);
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

            BuildUpload(menu);

            //  Return the menu.
            return menu;
        }

        void BuildUpload(ToolStrip menu) {
            var itemCountLines = new ToolStripMenuItem {
                Text = "Upload..."
                //Image = Properties.Resources.CountLines
            };
            itemCountLines.Click += (sender, args) => Upload();
            menu.Items.Add(itemCountLines);
        }

        void Upload() {
            //  Go through each file.
            foreach (var filePath in SelectedItemPaths.Select(x => x.ToAbsoluteDirectoryPath())) {
                // TODO: Whitelist the folders for Sync, or require confirmation from the user
                // TODO: slug for other games
                Process.Start("http://withsix.com/p/Arma-3/mods?upload=" + filePath); // TODO: UrlEncoding
            }
        }
    }
}