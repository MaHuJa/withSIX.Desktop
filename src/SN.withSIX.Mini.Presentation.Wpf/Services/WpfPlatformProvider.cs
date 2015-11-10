// <copyright company="SIX Networks GmbH" file="WpfPlatformProvider.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Presentation;
using SN.withSIX.Mini.Applications.Services;

namespace SN.withSIX.Mini.Presentation.Wpf.Services
{
    public class WpfPlatformProvider : IPlatformProvider, IPresentationService
    {
        public bool InDesignMode => UiConstants.InDesignMode;
    }
}