// <copyright company="SIX Networks GmbH" file="DesiredImageSize.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Infra.Cache
{
    public struct DesiredImageSize
    {
        public float? Height;
        public float? Width;

        public DesiredImageSize(float? width, float? height) {
            Width = width;
            Height = height;
        }

        public override string ToString() {
            return Width + "x" + Height;
        }
    }
}