// <copyright company="SIX Networks GmbH" file="ThemeInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using MahApps.Metro;

namespace SN.withSIX.Play.Applications.ViewModels
{
    // TODO: move to Presentation layer
    public static class ThemeInfo
    {
        static ThemeInfo() {
            // create accent color menu items for the demo
            Accents = ThemeManager.Accents
                .Select(
                    a =>
                        new AccentColorMenuDataBase {
                            Name = a.Name,
                            ColorBrush = a.Resources["AccentColorBrush"] as Brush
                        }).ToList();

            // create metro theme color menu items for the demo
            Themes = ThemeManager.AppThemes
                .Select(
                    a =>
                        new AccentColorMenuDataBase {
                            Name = a.Name,
                            BorderColorBrush = a.Resources["BlackColorBrush"] as Brush,
                            ColorBrush = a.Resources["WhiteColorBrush"] as Brush
                        }).ToList();
        }

        public static List<AccentColorMenuDataBase> Themes { get; set; }
        public static List<AccentColorMenuDataBase> Accents { get; set; }
    }
}