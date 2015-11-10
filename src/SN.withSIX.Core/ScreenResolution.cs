// <copyright company="SIX Networks GmbH" file="ScreenResolution.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core
{
    public struct ScreenResolution
    {
        public ScreenResolution(double width, double height) {
            Width = width;
            Height = height;
        }

        public double Width { get; }
        public double Height { get; }
    }
}