// <copyright company="SIX Networks GmbH" file="CustomThemeManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Windows;
using MahApps.Metro;

namespace SN.withSIX.Core.Presentation.Wpf.Services
{
    public class CustomThemeManager
    {
        static readonly string baseP;

        static CustomThemeManager() {
            baseP = "pack://application:,,," +
                    (Common.Flags.Merged
                        ? "/" + CommonBase.AssemblyLoader.GetEntryAssemblyName() + ";component"
                        : "/SN.withSIX.Core.Presentation.Wpf;component");
        }

        public static void ThemeManagerOnIsThemeChanged(object sender, OnThemeChangedEventArgs onThemeChangedEventArgs) {
            ReplaceTheme(GetInverseThemeName(onThemeChangedEventArgs.AppTheme.Name),
                onThemeChangedEventArgs.AppTheme.Name);
            ReplaceTheme("BaseOverrides", "BaseOverrides");
        }

        static void ReplaceTheme(string previousThemeName, string nextThemeName) {
            var previousTheme = GetTheme(previousThemeName).ToLower();
            var rsc =
                Application.Current.Resources.MergedDictionaries.Where(x => x.Source != null)
                    .FirstOrDefault(x => x.Source.ToString().ToLower() == previousTheme);
            if (rsc == null)
                return;
            Application.Current.Resources.MergedDictionaries.Add(CreateResourceDictionary(nextThemeName));
            Application.Current.Resources.MergedDictionaries.Remove(rsc);
        }

        static string GetTheme(string themeName) {
            return (baseP + "/Styles/Accents/" + themeName + ".xaml");
        }

        static string GetInverseThemeName(string themeName) {
            return themeName == "BaseLight" ? "BaseDark" : "BaseLight";
        }

        static ResourceDictionary CreateResourceDictionary(string themeName) {
            return new ResourceDictionary {
                Source = new Uri(GetTheme(themeName))
            };
        }
    }
}