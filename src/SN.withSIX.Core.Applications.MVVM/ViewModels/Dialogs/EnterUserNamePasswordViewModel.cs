// <copyright company="SIX Networks GmbH" file="EnterUserNamePasswordViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ReactiveUI.Legacy;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels.Dialogs
{
    public interface IEnterUserNamePasswordViewModel {}

    [DoNotObfuscate]
    public class EnterUserNamePasswordViewModel : DialogBase, IEnterUserNamePasswordViewModel
    {
        string _displayName = "Please enter username and password";
        string _location;
        string _password;
        string _username;

        public EnterUserNamePasswordViewModel() {
            this.SetCommand(x => x.CloseCommand).Subscribe(x => TryClose(true));
            this.SetCommand(x => x.CancelCommand).Subscribe(x => TryClose(false));
        }

        public ReactiveCommand CancelCommand { get; protected set; }
        public ReactiveCommand CloseCommand { get; protected set; }
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }
        public string Location
        {
            get { return _location; }
            set { SetProperty(ref _location, value); }
        }
    }
}