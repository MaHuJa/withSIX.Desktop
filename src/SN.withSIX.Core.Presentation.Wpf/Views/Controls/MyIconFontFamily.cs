// <copyright company="SIX Networks GmbH" file="MyIconFontFamily.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows.Media;
using Caliburn.Micro;
using SN.withSIX.Core.Presentation.Services;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public class MyIconFontFamily : FontFamily
    {
        // Pack Uri + font combo works at run-time.
        const string font = "./#icons-withSIX";
        static readonly Uri uri = Execute.InDesignMode
            ? null
            : new Uri(ResourceService.ResourcePath + "/fonts/", UriKind.Absolute);
        // String path works at design-time.... 
        static readonly string fontPath = Execute.InDesignMode
            ? ResourceService.ComponentPath + "/fonts/#icons-withSIX"
            : font;
        public MyIconFontFamily() : base(uri, fontPath) {}
        public MyIconFontFamily(string a) : this() {}
        public MyIconFontFamily(string a, Uri uri2) : this() {}
    }
}