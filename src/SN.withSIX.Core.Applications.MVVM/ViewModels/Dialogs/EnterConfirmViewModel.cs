// <copyright company="SIX Networks GmbH" file="EnterConfirmViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ReactiveUI.Legacy;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels.Dialogs
{
    public interface IEnterConfirmViewModel {}

    [DoNotObfuscate]
    public class EnterConfirmViewModel : DialogBase, IEnterConfirmViewModel
    {
        bool _canceled;
        bool _Closed;
        string _Input;
        bool _isMultiline;
        string _Message;
        bool? _rememberedState;

        public EnterConfirmViewModel() {
            this.SetCommand(x => x.OKCommand).Subscribe(x => OK());
            this.SetCommand(x => x.CancelCommand).Subscribe(x => Cancel());
            DisplayName = "Please input requested information";
        }

        public ReactiveCommand OKCommand { get; private set; }
        public ReactiveCommand CancelCommand { get; private set; }
        public bool Canceled
        {
            get { return _canceled; }
            set { SetProperty(ref _canceled, value); }
        }
        public bool Closed
        {
            get { return _Closed; }
            set { SetProperty(ref _Closed, value); }
        }
        public string Input
        {
            get { return _Input; }
            set { SetProperty(ref _Input, value); }
        }
        public string Message
        {
            get { return _Message; }
            set { SetProperty(ref _Message, value); }
        }
        public bool? RememberedState
        {
            get { return _rememberedState; }
            set { SetProperty(ref _rememberedState, value); }
        }
        public bool IsMultiline
        {
            get { return _isMultiline; }
            set { SetProperty(ref _isMultiline, value); }
        }

        [ReportUsage]
        public void OK() {
            Closed = true;
            TryClose();
        }

        [ReportUsage]
        public void Cancel() {
            Canceled = true;
            OK();
        }
    }
}