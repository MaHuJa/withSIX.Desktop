// <copyright company="SIX Networks GmbH" file="RuntimeCheck.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SN.withSIX.Core.Presentation
{
    public class RuntimeCheck
    {
        static readonly Uri net452 = new Uri("http://www.microsoft.com/en-us/download/details.aspx?id=42643");

        public void Check() {
            CheckNet45();
            var legacyCheck = RuntimePolicyHelper.LegacyV2RuntimeEnabledSuccessfully;
        }

        void CheckNet45() {
            if (IsNet45OrNewer())
                return;

            if (!IsVistaOrNewer()) {
                FatalErrorMessage("Windows Vista or later is required due to .NET framework 4.5 support.",
                    "Windows Vista or later required");
                Environment.Exit(1);
            }

            if (FatalErrorMessage(
                ".NET framework 4.5.1 or later is required, but was not found.\n\nDo you want to install it now?",
                ".NET framework 4.5.1 or later required"))
                TryOpenNet45Url();
            Environment.Exit(1);
        }

        protected virtual bool FatalErrorMessage(string message, string caption) {
            Console.WriteLine(caption + ": " + message + "\nY/N");
            var key = Console.ReadKey();
            return key.Key.ToString().ToLower() == "y";
        }

        static void TryOpenNet45Url() {
            try {
                Process.Start(net452.ToString());
            } catch (Exception) {}
        }

        static bool IsVistaOrNewer() {
            return Environment.OSVersion.Version.Major >= 6;
        }

        static bool IsNet45OrNewer() {
            return Type.GetType("System.Reflection.ReflectionContext", false) != null
                   && Type.GetType("System.Collections.Generic.IReadOnlyCollection`1", false) != null;
        }

        static class RuntimePolicyHelper
        {
            static RuntimePolicyHelper() {
                var clrRuntimeInfo =
                    (ICLRRuntimeInfo) RuntimeEnvironment.GetRuntimeInterfaceAsObject(
                        Guid.Empty,
                        typeof (ICLRRuntimeInfo).GUID);
                TryGetRuntimePolicy(clrRuntimeInfo);
            }

            public static bool LegacyV2RuntimeEnabledSuccessfully { get; private set; }

            static void TryGetRuntimePolicy(ICLRRuntimeInfo clrRuntimeInfo) {
                try {
                    clrRuntimeInfo.BindAsLegacyV2Runtime();
                    LegacyV2RuntimeEnabledSuccessfully = true;
                } catch (COMException) {
                    // This occurs with an HRESULT meaning 
                    // "A different runtime was already bound to the legacy CLR version 2 activation policy."
                    LegacyV2RuntimeEnabledSuccessfully = false;
                }
            }

            [ComImport]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            [Guid("BD39D1D2-BA2F-486A-89B0-B4B0CB466891")]
            interface ICLRRuntimeInfo
            {
                void xGetVersionString();
                void xGetRuntimeDirectory();
                void xIsLoaded();
                void xIsLoadable();
                void xLoadErrorString();
                void xLoadLibrary();
                void xGetProcAddress();
                void xGetInterface();
                void xSetDefaultStartupFlags();
                void xGetDefaultStartupFlags();

                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                void BindAsLegacyV2Runtime();
            }
        }
    }
}