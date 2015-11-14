using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace SN.withSIX.Mini.Presentation.Shell
{
    // Register with admin rights:
    // "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm"  /register /codebase "E:\projects\SN\withSIX.Desktop\src\SN.withSIX.Mini.Presentation.Shell\bin\Debug\SN.withSIX.Mini.Presentation.Shell.dll"
    // "C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm"  /register /codebase "E:\projects\SN\withSIX.Desktop\src\SN.withSIX.Mini.Presentation.Shell\bin\Debug\SN.withSIX.Mini.Presentation.Shell.dll"
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Directory)]
    public class UploadShellExtension : SharpContextMenu
    {
        /// <summary>
        /// Determines whether this instance can a shell
        /// context show menu, given the specified selected file list.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance should show a shell context
        /// menu for the specified file list; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanShowMenu()
        {
            //  We always show the menu.
            return SelectedItemPaths.All(IsNotKnownToSync);
        }

        /// <summary>
        /// Creates the context menu. This can be a single menu item or a tree of them.
        /// </summary>
        /// <returns>
        /// The context menu for the shell context menu.
        /// </returns>
        protected override ContextMenuStrip CreateMenu()
        {
            //  Create the menu strip.
            var menu = new ContextMenuStrip();

            BuildUpload(menu);

            //  Return the menu.
            return menu;
        }

        static bool IsNotKnownToSync(string x) => !IsKnownToSync(x);
        static bool IsKnownToSync(string x) => File.Exists(Path.Combine(x, ".sync.txt"));

        void BuildSync(ToolStrip menu) {
            var itemCountLines = new ToolStripMenuItem {
                Text = "Sync...",
                //Image = Properties.Resources.CountLines
            };
            itemCountLines.Click += (sender, args) => Sync();
            menu.Items.Add(itemCountLines);
        }


        void BuildUpload(ToolStrip menu)
        {
            var itemCountLines = new ToolStripMenuItem
            {
                Text = "Upload...",
                //Image = Properties.Resources.CountLines
            };
            itemCountLines.Click += (sender, args) => Upload();
            menu.Items.Add(itemCountLines);
        }

        private void Sync()
        {
            //  Builder for the output.
            var builder = new StringBuilder();

            //  Go through each file.
            foreach (var filePath in SelectedItemPaths)
            {
                //  Count the lines.
                builder.AppendLine(string.Format("{0} - {1} Characters (Sync) {2}",
                  Path.GetFileName(filePath), filePath.Length, IsKnownToSync(filePath)));
            }

            //  Show the ouput.
            MessageBox.Show(builder.ToString());
        }

        private void Upload()
        {
            //  Builder for the output.
            var builder = new StringBuilder();

            //  Go through each file.
            foreach (var filePath in SelectedItemPaths)
            {
                //  Count the lines.
                builder.AppendLine(string.Format("{0} - {1} Characters (Upload) {2}",
                  Path.GetFileName(filePath), filePath.Length, IsKnownToSync(filePath)));
            }

            //  Show the ouput.
            MessageBox.Show(builder.ToString());
        }
    }
}
