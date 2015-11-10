// <copyright company="SIX Networks GmbH" file="About.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;

namespace SN.withSIX.Core.Applications.Services
{
    [DoNotObfuscate]
    public class About : IApplicationService
    {
        public About() {
            Disclaimer =
                @" - DISCLAIMER -
The author does not take any responsibility for damages or other negativities caused or indirectly related to this software.
See the online full disclaimer for details.
";
            Components =
                @".NET / C#:
- Caliburn.Micro (MIT)
- GeoIP by MaxMind
- MahApps.Metro (Ms-PL)
- MoreLINQ by Jon Skeet (Apache)
- NLog (BSD)
- NotifyIcon for WPF (Philipp Sumi)
- ReactiveUI (Ms-PL)
- RestSharp (Ms-PL)
- SharpCompress (Ms-PL)
- WPFToolkit.Extended (Ms-PL)
- Advice by DotJosh (Josh Schwartzberg)

Utilities:
- DSUtils by Bohemia Interactive
- GeoIP DB by MaxMind
- GSlist by Luigi Auriemma (GPL)
- lftp (GPL)
- PboDLL utilities by Mikero
- rsync (GPL)
- zsync (GPL)
";
        }

        public string Disclaimer { get; private set; }
        public string Components { get; private set; }
        public string ProductVersion
        {
            get { return Common.App.ProductVersion; }
        }
        public Version AppVersion
        {
            get { return Common.App.ApplicationVersion; }
        }
    }
}