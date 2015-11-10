// <copyright company="SIX Networks GmbH" file="UacHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace SN.withSIX.Core.Presentation.SA
{
    public class UacHelper
    {
        #region TOKEN_ELEVATION_TYPE enum

        public enum TOKEN_ELEVATION_TYPE
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull,
            TokenElevationTypeLimited
        }

        #endregion

        #region TOKEN_INFORMATION_CLASS enum

        public enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }

        #endregion

        const uint STANDARD_RIGHTS_READ = 0x00020000;
        const uint TOKEN_QUERY = 0x0008;
        const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        const string UacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
        const string UacRegistryValue = "EnableLUA";

        public bool IsUacEnabled() {
            using (var uacKey = Registry.LocalMachine.OpenSubKey(UacRegistryKey, false))
                return uacKey.GetValue(UacRegistryValue).Equals(1);
        }

        public bool IsProcessElevated() {
            if (!IsUacEnabled())
                return IsUserPartOfAdminGroup();

            IntPtr tokenHandle;
            if (!NativeMethods.OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_READ, out tokenHandle)) {
                throw new ApplicationException("Could not get process token.  Win32 Error Code: " +
                                               Marshal.GetLastWin32Error());
            }

            var elevationResult = TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault;

            var elevationResultSize = Marshal.SizeOf((int) elevationResult);
            uint returnedSize = 0;
            var elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);

            var success = NativeMethods.GetTokenInformation(tokenHandle,
                TOKEN_INFORMATION_CLASS.TokenElevationType,
                elevationTypePtr, (uint) elevationResultSize, out returnedSize);
            if (!success)
                throw new ApplicationException("Unable to determine the current elevation.");
            elevationResult = (TOKEN_ELEVATION_TYPE) Marshal.ReadInt32(elevationTypePtr);
            if (elevationResult == TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault)
                return IsUserPartOfAdminGroup();
            return elevationResult == TOKEN_ELEVATION_TYPE.TokenElevationTypeFull;
        }

        static bool IsUserPartOfAdminGroup() {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public bool CheckUac() {
            var enabled = false;
            try {
                enabled = IsUacEnabled();
            } catch (Exception e) {}

            if (!enabled)
                return false;

            var elevated = false;
            try {
                elevated = IsProcessElevated();
            } catch (Exception e) {}

            return !elevated;
        }

        static class NativeMethods
        {
            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess,
                out IntPtr TokenHandle);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool GetTokenInformation(IntPtr TokenHandle,
                TOKEN_INFORMATION_CLASS TokenInformationClass,
                IntPtr TokenInformation, uint TokenInformationLength,
                out uint ReturnLength);
        }
    }
}