// <copyright company="SIX Networks GmbH" file="MyFontFamily.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows.Media;
using Caliburn.Micro;
using SN.withSIX.Core.Presentation.Services;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public class MyFontFamily : FontFamily
    {
        // Pack Uri + font combo works at run-time.
        const string font = "./#Ubuntu";
        static readonly Uri uri = Execute.InDesignMode
            ? null
            : new Uri(ResourceService.ResourcePath + "/fonts/", UriKind.Absolute);
        // String path works at design-time.... 
        static readonly string fontPath = Execute.InDesignMode
            ? ResourceService.ComponentPath + "/fonts/#Ubuntu"
            : font;
        public MyFontFamily() : base(uri, fontPath) {}
        public MyFontFamily(string a) : this() {}
        public MyFontFamily(string a, Uri uri2) : this() {}
    }
}