// <copyright company="SIX Networks GmbH" file="FileIcon.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

#region FileHeader

// withSIX Six.Core FileIcon.cs
// Copyright 2009-2013 SIX Networks GmbH
// Terms Of Service: http://www.withsix.com/tos

#endregion

// Borrowed from VbAccelerator.Components.Shell
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SN.withSIX.Core.Helpers
{
    public class FileIcon
    {
        #region Enumerations

        [Flags]
        public enum SHGetFileInfoConstants
        {
            SHGFI_ICON = 0x100, // get icon 
            SHGFI_DISPLAYNAME = 0x200, // get display name 
            SHGFI_TYPENAME = 0x400, // get type name 
            SHGFI_ATTRIBUTES = 0x800, // get attributes 
            SHGFI_ICONLOCATION = 0x1000, // get icon location 
            SHGFI_EXETYPE = 0x2000, // return exe type 
            SHGFI_SYSICONINDEX = 0x4000, // get system icon index 
            SHGFI_LINKOVERLAY = 0x8000, // put a link overlay on icon 
            SHGFI_SELECTED = 0x10000, // show icon in selected state 
            SHGFI_ATTR_SPECIFIED = 0x20000, // get only specified attributes 
            SHGFI_LARGEICON = 0x0, // get large icon 
            SHGFI_SMALLICON = 0x1, // get small icon 
            SHGFI_OPENICON = 0x2, // get open icon 
            SHGFI_SHELLICONSIZE = 0x4, // get shell size icon 
            //SHGFI_PIDL = 0x8,                  // pszPath is a pidl 
            SHGFI_USEFILEATTRIBUTES = 0x10, // use passed dwFileAttribute 
            SHGFI_ADDOVERLAYS = 0x000000020, // apply the appropriate overlays
            SHGFI_OVERLAYINDEX = 0x000000040 // Get the index of the overlay
        }

        #endregion

        #region UnmanagedCode

        const int MAX_PATH = 260;
        const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
        const int FORMAT_MESSAGE_FROM_STRING = 0x400;
        const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xFF;

        [DllImport("shell32")]
        static extern int SHGetFileInfo(
            string pszPath,
            int dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        [DllImport("user32.dll")]
        static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("kernel32")]
        static extern int FormatMessage(
            int dwFlags,
            IntPtr lpSource,
            int dwMessageId,
            int dwLanguageId,
            string lpBuffer,
            uint nSize,
            int argumentsLong);

        [DllImport("kernel32")]
        static extern int GetLastError();

        [StructLayout(LayoutKind.Sequential)]
        struct SHFILEINFO
        {
            public readonly IntPtr hIcon;
            readonly int iIcon;
            readonly int dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)] public readonly string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public readonly string szTypeName;
        }

        #endregion

        #region Member Variables

        #endregion

        #region Implementation

        public FileIcon() {
            Flags = SHGetFileInfoConstants.SHGFI_ICON |
                    SHGetFileInfoConstants.SHGFI_DISPLAYNAME |
                    SHGetFileInfoConstants.SHGFI_TYPENAME |
                    SHGetFileInfoConstants.SHGFI_ATTRIBUTES |
                    SHGetFileInfoConstants.SHGFI_EXETYPE;
        }

        public FileIcon(string fileName)
            : this() {
            FileName = fileName;
            GetInfo();
        }

        public FileIcon(string fileName, SHGetFileInfoConstants flags) {
            FileName = fileName;
            Flags = flags;
            GetInfo();
        }

        public SHGetFileInfoConstants Flags { get; set; }

        public string FileName { get; set; }

        public Icon ShellIcon { get; set; }

        public string DisplayName { get; set; }

        public string TypeName { get; set; }

        public void GetInfo() {
            ShellIcon = null;
            TypeName = String.Empty;
            DisplayName = String.Empty;

            var shfi = new SHFILEINFO();
            var shfiSize = (uint) Marshal.SizeOf(shfi.GetType());

            var ret = SHGetFileInfo(
                FileName, 0, ref shfi, shfiSize, (uint) (Flags));
            if (ret != 0) {
                if (shfi.hIcon != IntPtr.Zero) {
                    ShellIcon = Icon.FromHandle(shfi.hIcon);
                    // Now owned by the GDI+ object
                    //DestroyIcon(shfi.hIcon);
                }
                TypeName = shfi.szTypeName;
                DisplayName = shfi.szDisplayName;
            } else {
                var err = GetLastError();
                Console.WriteLine("Error {0}", err);
                var txtS = new string('\0', 256);
                var len = FormatMessage(
                    FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
                    IntPtr.Zero, err, 0, txtS, 256, 0);
                Console.WriteLine("Len {0} text {1}", len, txtS);

                // throw exception
            }
        }

        #endregion
    }
}