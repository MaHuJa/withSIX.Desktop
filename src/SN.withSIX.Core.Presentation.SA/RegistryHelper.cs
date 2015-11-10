// <copyright company="SIX Networks GmbH" file="RegistryHelper.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using Microsoft.Win32;

namespace SN.withSIX.Core.Presentation.SA
{
    class RegistryHelper
    {
        const string REGISTRY_ROOT = @"SOFTWARE\RedGate\SmartAssembly";

        public static string TryReadHKLMRegistryString(string name) {
            try {
                return ReadHKLMRegistryString(name);
            } catch {
                return string.Empty;
            }
        }

        static string ReadHKLMRegistryString(string name) {
            using (var key = Registry.LocalMachine.OpenSubKey(REGISTRY_ROOT)) {
                if (key == null)
                    return string.Empty;

                var value = (string) key.GetValue(name, string.Empty);
                return value;
            }
        }

        public static void TrySaveHKLMRegistryString(string name, string value) {
            try {
                SaveHKLMRegistryString(name, value);
            } catch {}
        }

        static void SaveHKLMRegistryString(string name, string value) {
            using (var key = Registry.LocalMachine.OpenSubKey(REGISTRY_ROOT, true)) {
                if (key == null) {
                    using (var key2 = Registry.LocalMachine.CreateSubKey(REGISTRY_ROOT))
                        key2.SetValue(name, value);
                    return;
                }
                key.SetValue(name, value);
            }
        }
    }
}