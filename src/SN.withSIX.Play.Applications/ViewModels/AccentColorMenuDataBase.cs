// <copyright company="SIX Networks GmbH" file="AccentColorMenuDataBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows.Media;

namespace SN.withSIX.Play.Applications.ViewModels
{
    public class AccentColorMenuDataBase
    {
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }
    }
}