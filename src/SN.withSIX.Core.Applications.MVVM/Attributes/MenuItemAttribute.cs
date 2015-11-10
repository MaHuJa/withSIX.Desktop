// <copyright company="SIX Networks GmbH" file="MenuItemAttribute.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Core.Applications.MVVM.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MenuItemAttribute : Attribute
    {
        public MenuItemAttribute(string displayName = null, Type type = null, string icon = null) {
            DisplayName = displayName;
            Type = type;
            Icon = icon;
        }

        public string DisplayName { get; set; }
        public string Icon { get; set; }
        public Type Type { get; set; }
        public bool IsSeparator { get; set; }
    }
}