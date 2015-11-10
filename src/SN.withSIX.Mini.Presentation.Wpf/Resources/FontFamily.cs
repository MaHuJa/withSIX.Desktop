// <copyright company="SIX Networks GmbH" file="FontFamily.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows.Media;
using SN.withSIX.Mini.Applications;

namespace SN.withSIX.Mini.Presentation.Wpf.Resources
{
    public class UbuntuFontFamily : FontFamily
    {
        // Pack Uri + font combo works at run-time.
        const string font = "./#Ubuntu";
        internal static string ResourcePath = "pack://application:,,,/SN.withSIX.Core.Presentation.Resources;component";
        static readonly Uri uri = Cheat.PlatformProvider.InDesignMode
            ? null
            : new Uri(ResourcePath + "/fonts/", UriKind.Absolute);
        // String path works at design-time.... 
        static readonly string fontPath = Cheat.PlatformProvider.InDesignMode
            ? ResourcePath + "/fonts/#Ubuntu"
            : font;
        public UbuntuFontFamily() : base(uri, fontPath) {}
        public UbuntuFontFamily(string a) : this() {}
        public UbuntuFontFamily(string a, Uri uri2) : this() {}
    }

    public class UbuntuLightFontFamily : FontFamily
    {
        // Pack Uri + font combo works at run-time.
        const string font = "./#Ubuntu Light";
        static readonly Uri uri = Cheat.PlatformProvider.InDesignMode
            ? null
            : new Uri(UbuntuFontFamily.ResourcePath + "/fonts/", UriKind.Absolute);
        // String path works at design-time.... 
        static readonly string fontPath = Cheat.PlatformProvider.InDesignMode
            ? UbuntuFontFamily.ResourcePath + "/fonts/#Ubuntu Light"
            : font;
        public UbuntuLightFontFamily() : base(uri, fontPath) {}
        public UbuntuLightFontFamily(string a) : this() {}
        public UbuntuLightFontFamily(string a, Uri uri2) : this() {}
    }

    public class IconFontFamily : FontFamily
    {
        // Pack Uri + font combo works at run-time.
        const string font = "./#icons-withSIX";
        static readonly Uri uri = Cheat.PlatformProvider.InDesignMode
            ? null
            : new Uri(UbuntuFontFamily.ResourcePath + "/fonts/", UriKind.Absolute);
        // String path works at design-time.... 
        static readonly string fontPath = Cheat.PlatformProvider.InDesignMode
            ? UbuntuFontFamily.ResourcePath + "/fonts/#icons-withSIX"
            : font;
        public IconFontFamily() : base(uri, fontPath) {}
        public IconFontFamily(string a) : this() {}
        public IconFontFamily(string a, Uri uri2) : this() {}
    }
}