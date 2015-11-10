// <copyright company="SIX Networks GmbH" file="WindowBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Applications.ViewModels
{
    public abstract class WindowBase : ScreenBase, IWindowScreen
    {
        double _height;
        double _left = 256;
        double _top;
        double _width;
        WindowState _windowState = WindowState.Normal;
        public virtual WindowState WindowState
        {
            get { return _windowState; }
            set { SetProperty(ref _windowState, value); }
        }
        public virtual double Left
        {
            get { return _left; }
            set { SetProperty(ref _left, value); }
        }
        public virtual double Top
        {
            get { return _top; }
            set { SetProperty(ref _top, value); }
        }
        public virtual double Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }
        public virtual double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        public void SetWindowState(string state) {
            if (state == null) {
                WindowState = WindowState.Normal;
                return;
            }
            WindowState = (WindowState) Enum.Parse(typeof (WindowState), state);
        }

        public string GetWindowState() {
            return WindowState.ToString();
        }
    }
}