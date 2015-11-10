// <copyright company="SIX Networks GmbH" file="WindowSettings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Play.Core.Options
{
    [DataContract(Name = "WindowSettings", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class WindowSettings
    {
        [DataMember]
        public double Top { get; set; }
        [DataMember]
        public double Left { get; set; }
        [DataMember]
        public double Height { get; set; }
        [DataMember]
        public double Width { get; set; }
        [DataMember]
        public bool IsMaximized { get; set; }
        [DataMember]
        public string WindowStateStr { get; set; }

        public static WindowSettings Create(IWindowScreen vm) {
            return new WindowSettings {
                Top = vm.Top,
                Left = vm.Left,
                Height = vm.Height,
                Width = vm.Width,
                WindowStateStr = vm.GetWindowState()
            };
        }

        public void Apply(IWindowScreen vm) {
            vm.Top = Top;
            vm.Left = Left;
            vm.Height = Height > 0.0 ? Height : 0;
            vm.Width = Width > 0.0 ? Width : 0;
            vm.SetWindowState(WindowStateStr);
        }
    }
}