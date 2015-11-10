// <copyright company="SIX Networks GmbH" file="IWindowScreen.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Play.Core.Options
{
    public interface IWindowScreen
    {
        double Left { get; set; }
        double Top { get; set; }
        double Height { get; set; }
        double Width { get; set; }
        void SetWindowState(string state);
        string GetWindowState();
    }
}