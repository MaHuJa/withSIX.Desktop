// <copyright company="SIX Networks GmbH" file="ShellLink.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

#region FileHeader

// withSIX Six.Core ShellLink.cs
// Copyright 2009-2013 SIX Networks GmbH
// Terms Of Service: http://www.withsix.com/tos

#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace SN.withSIX.Core.Helpers
{
    // Borrowed from VbAccelerator.Components.Shell

    #region ShellLink Object

    public class ShellLink : IDisposable
    {
        #region ComInterop for IShellLink

        #region IPersist Interface

        [ComImport]
        [Guid("0000010C-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IPersist
        {
            [PreserveSig]
            //[helpstring("Returns the class identifier for the component object")]
            void GetClassID(out Guid pClassID);
        }

        #endregion

        #region IPersistFile Interface

        [ComImport]
        [Guid("0000010B-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IPersistFile
        {
            // can't get this to go if I extend IPersist, so put it here:
            [PreserveSig]
            void GetClassID(out Guid pClassID);

            //[helpstring("Checks for changes since last file write")]      
            void IsDirty();

            //[helpstring("Opens the specified file and initializes the object from its contents")]      
            void Load(
                [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
                uint dwMode);

            //[helpstring("Saves the object into the specified file")]      
            void Save(
                [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
                [MarshalAs(UnmanagedType.Bool)] bool fRemember);

            //[helpstring("Notifies the object that save is completed")]      
            void SaveCompleted(
                [MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

            //[helpstring("Gets the current name of the file associated with the object")]      
            void GetCurFile(
                [MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }

        #endregion

        #region IShellLink Interface

        #region Nested type: IShellLinkA

        [ComImport]
        [Guid("000214EE-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IShellLinkA
        {
            //[helpstring("Retrieves the path and filename of a shell link object")]
            void GetPath(
                [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile,
                int cchMaxPath,
                ref _WIN32_FIND_DATAA pfd,
                uint fFlags);

            //[helpstring("Retrieves the list of shell link item identifiers")]
            void GetIDList(out IntPtr ppidl);

            //[helpstring("Sets the list of shell link item identifiers")]
            void SetIDList(IntPtr pidl);

            //[helpstring("Retrieves the shell link description string")]
            void GetDescription(
                [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile,
                int cchMaxName);

            //[helpstring("Sets the shell link description string")]
            void SetDescription(
                [MarshalAs(UnmanagedType.LPStr)] string pszName);

            //[helpstring("Retrieves the name of the shell link working          directory")]
            void GetWorkingDirectory(
                [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir,
                int cchMaxPath);

            //[helpstring("Sets the name of the shell link working directory")]
            void SetWorkingDirectory(
                [MarshalAs(UnmanagedType.LPStr)] string pszDir);

            //[helpstring("Retrieves the shell link command-line arguments")]
            void GetArguments(
                [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs,
                int cchMaxPath);

            //[helpstring("Sets the shell link command-line arguments")]
            void SetArguments(
                [MarshalAs(UnmanagedType.LPStr)] string pszArgs);

            //[propget, helpstring("Retrieves or sets the shell link hot key")]
            void GetHotkey(out short pwHotkey);

            //[propput, helpstring("Retrieves or sets the shell link hot key")]
            void SetHotkey(short pwHotkey);

            //[propget, helpstring("Retrieves or sets the shell link show command")]
            void GetShowCmd(out uint piShowCmd);

            //[propput, helpstring("Retrieves or sets the shell link show command")]
            void SetShowCmd(uint piShowCmd);

            //[helpstring("Retrieves the location (path and index) of the shell link icon")]
            void GetIconLocation(
                [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath,
                int cchIconPath,
                out int piIcon);

            //[helpstring("Sets the location (path and index) of the shell link icon")]
            void SetIconLocation(
                [MarshalAs(UnmanagedType.LPStr)] string pszIconPath,
                int iIcon);

            //[helpstring("Sets the shell link relative path")]
            void SetRelativePath(
                [MarshalAs(UnmanagedType.LPStr)] string pszPathRel,
                uint dwReserved);

            //[helpstring("Resolves a shell link. The system searches for the shell link object and updates the shell link path and its list of identifiers (if necessary)")]
            void Resolve(
                IntPtr hWnd,
                uint fFlags);

            //[helpstring("Sets the shell link path and filename")]
            void SetPath(
                [MarshalAs(UnmanagedType.LPStr)] string pszFile);
        }

        #endregion

        #region Nested type: IShellLinkW

        [ComImport]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IShellLinkW
        {
            //[helpstring("Retrieves the path and filename of a shell link          object")]
            void GetPath(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
                int cchMaxPath,
                ref _WIN32_FIND_DATAW pfd,
                uint fFlags);

            //[helpstring("Retrieves the list of shell link item identifiers")]
            void GetIDList(out IntPtr ppidl);

            //[helpstring("Sets the list of shell link item identifiers")]
            void SetIDList(IntPtr pidl);

            //[helpstring("Retrieves the shell link description string")]
            void GetDescription(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
                int cchMaxName);

            //[helpstring("Sets the shell link description string")]
            void SetDescription(
                [MarshalAs(UnmanagedType.LPWStr)] string pszName);

            //[helpstring("Retrieves the name of the shell link working          directory")]
            void GetWorkingDirectory(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
                int cchMaxPath);

            //[helpstring("Sets the name of the shell link working directory")]
            void SetWorkingDirectory(
                [MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            //[helpstring("Retrieves the shell link command-line arguments")]
            void GetArguments(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
                int cchMaxPath);

            //[helpstring("Sets the shell link command-line arguments")]
            void SetArguments(
                [MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            //[propget, helpstring("Retrieves or sets the shell link hot key")]
            void GetHotkey(out short pwHotkey);

            //[propput, helpstring("Retrieves or sets the shell link hot key")]
            void SetHotkey(short pwHotkey);

            //[propget, helpstring("Retrieves or sets the shell link show          command")]
            void GetShowCmd(out uint piShowCmd);

            //[propput, helpstring("Retrieves or sets the shell link show          command")]
            void SetShowCmd(uint piShowCmd);

            //[helpstring("Retrieves the location (path and index) of the shell link icon")]
            void GetIconLocation(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
                int cchIconPath,
                out int piIcon);

            //[helpstring("Sets the location (path and index) of the shell link icon")]
            void SetIconLocation(
                [MarshalAs(UnmanagedType.LPWStr)] string pszIconPath,
                int iIcon);

            //[helpstring("Sets the shell link relative path")]
            void SetRelativePath(
                [MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,
                uint dwReserved);

            //[helpstring("Resolves a shell link. The system searches for the shell link object and updates the shell link path and its list of identifiers (if necessary)")]
            void Resolve(
                IntPtr hWnd,
                uint fFlags);

            //[helpstring("Sets the shell link path and filename")]
            void SetPath(
                [MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        #endregion

        #endregion

        #region ShellLinkCoClass

        [Guid("00021401-0000-0000-C000-000000000046")]
        [ClassInterface(ClassInterfaceType.None)]
        [ComImport]
        class CShellLink {}

        #endregion

        #region Nested type: EShellLinkGP

        enum EShellLinkGP : uint
        {
            SLGP_SHORTPATH = 1,
            SLGP_UNCPRIORITY = 2
        }

        #endregion

        #region Nested type: EShowWindowFlags

        [Flags]
        enum EShowWindowFlags : uint
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }

        #endregion

        #region IShellLink Private structs

        #region Nested type: _FILETIME

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0)]
        struct _FILETIME
        {
            public readonly uint dwLowDateTime;
            public readonly uint dwHighDateTime;
        }

        #endregion

        #region Nested type: _WIN32_FIND_DATAA

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0,
            CharSet = CharSet.Ansi)]
        struct _WIN32_FIND_DATAA
        {
            public readonly uint dwFileAttributes;
            public readonly _FILETIME ftCreationTime;
            public readonly _FILETIME ftLastAccessTime;
            public readonly _FILETIME ftLastWriteTime;
            public readonly uint nFileSizeHigh;
            public readonly uint nFileSizeLow;
            public readonly uint dwReserved0;
            public readonly uint dwReserved1;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
            public readonly string cFileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)] public readonly string cAlternateFileName;
        }

        #endregion

        #region Nested type: _WIN32_FIND_DATAW

        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0,
            CharSet = CharSet.Unicode)]
        struct _WIN32_FIND_DATAW
        {
            public readonly uint dwFileAttributes;
            public readonly _FILETIME ftCreationTime;
            public readonly _FILETIME ftLastAccessTime;
            public readonly _FILETIME ftLastWriteTime;
            public readonly uint nFileSizeHigh;
            public readonly uint nFileSizeLow;
            public readonly uint dwReserved0;
            public readonly uint dwReserved1;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] // MAX_PATH
            public readonly string cFileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)] public readonly string cAlternateFileName;
        }

        #endregion

        #endregion

        #region UnManaged Methods

        class UnManagedMethods
        {
            [DllImport("Shell32", CharSet = CharSet.Auto)]
            internal static extern int ExtractIconEx(
                [MarshalAs(UnmanagedType.LPTStr)] string lpszFile,
                int nIconIndex,
                IntPtr[] phIconLarge,
                IntPtr[] phIconSmall,
                int nIcons);

            [DllImport("user32")]
            internal static extern int DestroyIcon(IntPtr hIcon);
        }

        #endregion

        #endregion

        #region Enumerations

        #region EShellLinkResolveFlags enum

        [Flags]
        public enum EShellLinkResolveFlags : uint
        {
            SLR_ANY_MATCH = 0x2,
            SLR_INVOKE_MSI = 0x80,
            SLR_NOLINKINFO = 0x40,
            SLR_NO_UI = 0x1,
            SLR_NO_UI_WITH_MSG_PUMP = 0x101,
            SLR_NOUPDATE = 0x8,
            SLR_NOSEARCH = 0x10,
            SLR_NOTRACK = 0x20,
            SLR_UPDATE = 0x4
        }

        #endregion

        #region LinkDisplayMode enum

        public enum LinkDisplayMode : uint
        {
            edmNormal = EShowWindowFlags.SW_NORMAL,
            edmMinimized = EShowWindowFlags.SW_SHOWMINNOACTIVE,
            edmMaximized = EShowWindowFlags.SW_MAXIMIZE
        }

        #endregion

        #endregion

        #region Member Variables

        // Use Unicode (W) under NT, otherwise use ANSI      
        IShellLinkA linkA;
        IShellLinkW linkW;

        #endregion

        #region Constructor

        public ShellLink() {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                linkW = (IShellLinkW) new CShellLink();
            else
                linkA = (IShellLinkA) new CShellLink();
        }

        public ShellLink(string linkFile)
            : this() {
            Open(linkFile);
        }

        #endregion

        #region Destructor and Dispose

        public void Dispose() {
            if (linkW != null) {
                Marshal.ReleaseComObject(linkW);
                linkW = null;
            }
            if (linkA != null) {
                Marshal.ReleaseComObject(linkA);
                linkA = null;
            }
        }

        ~ShellLink() {
            Dispose();
        }

        #endregion

        #region Implementation

        public string ShortCutFile { get; set; } = String.Empty;

        public Icon LargeIcon
        {
            get { return getIcon(true); }
        }

        public Icon SmallIcon
        {
            get { return getIcon(false); }
        }

        public string IconPath
        {
            get
            {
                var iconPath = new StringBuilder(260, 260);
                var iconIndex = 0;
                if (linkA == null) {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out
                        iconIndex);
                } else {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out
                        iconIndex);
                }
                return iconPath.ToString();
            }
            set
            {
                var iconPath = new StringBuilder(260, 260);
                var iconIndex = 0;
                if (linkA == null) {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out
                        iconIndex);
                } else {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out
                        iconIndex);
                }
                if (linkA == null)
                    linkW.SetIconLocation(value, iconIndex);
                else
                    linkA.SetIconLocation(value, iconIndex);
            }
        }

        public int IconIndex
        {
            get
            {
                var iconPath = new StringBuilder(260, 260);
                var iconIndex = 0;
                if (linkA == null) {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out
                        iconIndex);
                } else {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out
                        iconIndex);
                }
                return iconIndex;
            }
            set
            {
                var iconPath = new StringBuilder(260, 260);
                var iconIndex = 0;
                if (linkA == null) {
                    linkW.GetIconLocation(iconPath, iconPath.Capacity, out
                        iconIndex);
                } else {
                    linkA.GetIconLocation(iconPath, iconPath.Capacity, out
                        iconIndex);
                }
                if (linkA == null)
                    linkW.SetIconLocation(iconPath.ToString(), value);
                else
                    linkA.SetIconLocation(iconPath.ToString(), value);
            }
        }

        public string Target
        {
            get
            {
                var target = new StringBuilder(260, 260);
                if (linkA == null) {
                    var fd = new _WIN32_FIND_DATAW();
                    linkW.GetPath(target, target.Capacity, ref fd,
                        (uint) EShellLinkGP.SLGP_UNCPRIORITY);
                } else {
                    var fd = new _WIN32_FIND_DATAA();
                    linkA.GetPath(target, target.Capacity, ref fd,
                        (uint) EShellLinkGP.SLGP_UNCPRIORITY);
                }
                return target.ToString();
            }
            set
            {
                if (linkA == null)
                    linkW.SetPath(value);
                else
                    linkA.SetPath(value);
            }
        }

        public string WorkingDirectory
        {
            get
            {
                var path = new StringBuilder(260, 260);
                if (linkA == null)
                    linkW.GetWorkingDirectory(path, path.Capacity);
                else
                    linkA.GetWorkingDirectory(path, path.Capacity);
                return path.ToString();
            }
            set
            {
                if (linkA == null)
                    linkW.SetWorkingDirectory(value);
                else
                    linkA.SetWorkingDirectory(value);
            }
        }

        public string Description
        {
            get
            {
                var description = new StringBuilder(1024, 1024);
                if (linkA == null)
                    linkW.GetDescription(description, description.Capacity);
                else
                    linkA.GetDescription(description, description.Capacity);
                return description.ToString();
            }
            set
            {
                if (linkA == null)
                    linkW.SetDescription(value);
                else
                    linkA.SetDescription(value);
            }
        }

        public string Arguments
        {
            get
            {
                var arguments = new StringBuilder(260, 260);
                if (linkA == null)
                    linkW.GetArguments(arguments, arguments.Capacity);
                else
                    linkA.GetArguments(arguments, arguments.Capacity);
                return arguments.ToString();
            }
            set
            {
                if (linkA == null)
                    linkW.SetArguments(value);
                else
                    linkA.SetArguments(value);
            }
        }

        public LinkDisplayMode DisplayMode
        {
            get
            {
                uint cmd = 0;
                if (linkA == null)
                    linkW.GetShowCmd(out cmd);
                else
                    linkA.GetShowCmd(out cmd);
                return (LinkDisplayMode) cmd;
            }
            set
            {
                if (linkA == null)
                    linkW.SetShowCmd((uint) value);
                else
                    linkA.SetShowCmd((uint) value);
            }
        }

        Icon getIcon(bool large) {
            // Get icon index and path:
            var iconIndex = 0;
            var iconPath = new StringBuilder(260, 260);
            if (linkA == null)
                linkW.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
            else
                linkA.GetIconLocation(iconPath, iconPath.Capacity, out iconIndex);
            var iconFile = iconPath.ToString();

            // If there are no details set for the icon, then we must use
            // the shell to get the icon for the target:
            if (iconFile.Length == 0) {
                // Use the FileIcon object to get the icon:
                var flags =
                    FileIcon.SHGetFileInfoConstants.SHGFI_ICON |
                    FileIcon.SHGetFileInfoConstants.SHGFI_ATTRIBUTES;
                if (large)
                    flags = flags | FileIcon.SHGetFileInfoConstants.SHGFI_LARGEICON;
                else
                    flags = flags | FileIcon.SHGetFileInfoConstants.SHGFI_SMALLICON;
                var fileIcon = new FileIcon(Target, flags);
                return fileIcon.ShellIcon;
            }
            // Use ExtractIconEx to get the icon:
            var hIconEx = new IntPtr[1] {IntPtr.Zero};
            var iconCount = 0;
            if (large) {
                iconCount = UnManagedMethods.ExtractIconEx(
                    iconFile,
                    iconIndex,
                    hIconEx,
                    null,
                    1);
            } else {
                iconCount = UnManagedMethods.ExtractIconEx(
                    iconFile,
                    iconIndex,
                    null,
                    hIconEx,
                    1);
            }
            // If success then return as a GDI+ object
            Icon icon = null;
            if (hIconEx[0] != IntPtr.Zero) {
                icon = Icon.FromHandle(hIconEx[0]);
                //UnManagedMethods.DestroyIcon(hIconEx[0]);
            }
            return icon;
        }

        /*
        public Keys HotKey
        {
            get
            {
                short key = 0;
                if (linkA == null)
                {
                    linkW.GetHotkey(out key);
                }
                else
                {
                    linkA.GetHotkey(out key);
                }
                return (Keys)key;
            }
            set
            {
                if (linkA == null)
                {
                    linkW.SetHotkey((short)value);
                }
                else
                {
                    linkA.SetHotkey((short)value);
                }
            }
        }
*/

        public void Save() {
            Save(ShortCutFile);
        }

        public void Save(
            string linkFile
            ) {
            // Save the object to disk
            if (linkA == null) {
                ((IPersistFile) linkW).Save(linkFile, true);
                ShortCutFile = linkFile;
            } else {
                ((IPersistFile) linkA).Save(linkFile, true);
                ShortCutFile = linkFile;
            }
        }

        public void Open(
            string linkFile
            ) {
            Open(linkFile,
                IntPtr.Zero,
                (EShellLinkResolveFlags.SLR_ANY_MATCH |
                 EShellLinkResolveFlags.SLR_NO_UI),
                1);
        }

        public void Open(
            string linkFile,
            IntPtr hWnd,
            EShellLinkResolveFlags resolveFlags
            ) {
            Open(linkFile,
                hWnd,
                resolveFlags,
                1);
        }

        public void Open(
            string linkFile,
            IntPtr hWnd,
            EShellLinkResolveFlags resolveFlags,
            ushort timeOut
            ) {
            uint flags;

            if ((resolveFlags & EShellLinkResolveFlags.SLR_NO_UI)
                == EShellLinkResolveFlags.SLR_NO_UI)
                flags = (uint) ((int) resolveFlags | (timeOut << 16));
            else
                flags = (uint) resolveFlags;

            if (linkA == null) {
                ((IPersistFile) linkW).Load(linkFile, 0); //STGM_DIRECT)
                linkW.Resolve(hWnd, flags);
                ShortCutFile = linkFile;
            } else {
                ((IPersistFile) linkA).Load(linkFile, 0); //STGM_DIRECT)
                linkA.Resolve(hWnd, flags);
                ShortCutFile = linkFile;
            }
        }

        #endregion
    }

    #endregion
}