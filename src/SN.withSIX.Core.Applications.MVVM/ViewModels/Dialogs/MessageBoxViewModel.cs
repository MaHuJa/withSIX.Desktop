// <copyright company="SIX Networks GmbH" file="MessageBoxViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using ReactiveUI.Legacy;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels.Dialogs
{
    public interface IMessageBoxViewModel {}


    public interface IDontIC {}

    [DoNotObfuscate]
    public class MessageBoxViewModel : DialogBase, IMessageBoxViewModel, IDontIC
    {
        string _blueButtonContent;
        MessageBoxButton _buttons;
        string _greenButtonContent;
        string _message;
        string _redButtonContent;
        bool? _rememberedState;

        public MessageBoxViewModel(string message, string title = null, MessageBoxButton buttons = MessageBoxButton.OK,
            bool? rememberedState = null) {
            Message = message;
            DisplayName = title;
            Buttons = buttons;
            RememberedState = rememberedState;

            this.SetCommand(x => x.GreenCommand).Subscribe(x => {
                TryClose();
                if (Buttons == MessageBoxButton.YesNo || Buttons == MessageBoxButton.YesNoCancel) {
                    Result = RememberedState.HasValue && RememberedState.Value
                        ? SixMessageBoxResult.YesRemember
                        : SixMessageBoxResult.Yes;
                } else
                    Result = SixMessageBoxResult.OK;
            });

            this.SetCommand(x => x.BlueCommand).Subscribe(x => {
                TryClose();
                Result = SixMessageBoxResult.Cancel;
            });

            this.SetCommand(x => x.RedCommand).Subscribe(x => {
                TryClose();
                if (Buttons == MessageBoxButton.YesNo || Buttons == MessageBoxButton.YesNoCancel) {
                    Result = RememberedState.HasValue && RememberedState.Value
                        ? SixMessageBoxResult.NoRemember
                        : SixMessageBoxResult.No;
                } else
                    Result = SixMessageBoxResult.Cancel;
            });

            this.SetCommand(x => x.CancelCommand).Subscribe(x => {
                TryClose();
                Result = SixMessageBoxResult.Cancel;
            });
        }

        public SixMessageBoxResult Result { get; set; } = SixMessageBoxResult.None;
        public MessageBoxButton Buttons
        {
            get { return _buttons; }
            set
            {
                SetProperty(ref _buttons, value);
                SetupButtons(_buttons);
            }
        }
        public bool? RememberedState
        {
            get { return _rememberedState; }
            set { SetProperty(ref _rememberedState, value); }
        }
        public ReactiveCommand CancelCommand { get; protected set; }
        public ReactiveCommand GreenCommand { get; protected set; }
        public ReactiveCommand BlueCommand { get; protected set; }
        public ReactiveCommand RedCommand { get; protected set; }
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        public string GreenButtonContent
        {
            get { return _greenButtonContent; }
            set { SetProperty(ref _greenButtonContent, value); }
        }
        public string BlueButtonContent
        {
            get { return _blueButtonContent; }
            set { SetProperty(ref _blueButtonContent, value); }
        }
        public string RedButtonContent
        {
            get { return _redButtonContent; }
            set { SetProperty(ref _redButtonContent, value); }
        }

        void SetupButtons(MessageBoxButton buttons) {
            switch (buttons) {
            case MessageBoxButton.OK:
                GreenButtonContent = "ok";
                BlueButtonContent = null;
                RedButtonContent = null;
                break;
            case MessageBoxButton.OKCancel:
                GreenButtonContent = "ok";
                BlueButtonContent = null;
                RedButtonContent = "cancel";
                break;
            case MessageBoxButton.YesNo:
                GreenButtonContent = "yes";
                BlueButtonContent = null;
                RedButtonContent = "no";
                break;
            case MessageBoxButton.YesNoCancel:
                GreenButtonContent = "yes";
                BlueButtonContent = "cancel";
                RedButtonContent = "no";
                break;
            }
        }
    }

    [DoNotObfuscate]
    public class MetroMessageBoxViewModel : MetroDialogBase, IMessageBoxViewModel, IDontIC
    {
        string _blueButtonContent;
        MessageBoxButton _buttons;
        string _greenButtonContent;
        string _message;
        string _redButtonContent;
        bool? _rememberedState;

        public MetroMessageBoxViewModel(string message, string title = null,
            MessageBoxButton buttons = MessageBoxButton.OK,
            bool? rememberedState = null) {
            Message = message;
            DisplayName = title;
            Buttons = buttons;
            RememberedState = rememberedState;

            this.SetCommand(x => x.GreenCommand).Subscribe(x => {
                TryClose();
                if (Buttons == MessageBoxButton.YesNo || Buttons == MessageBoxButton.YesNoCancel) {
                    Result = RememberedState.HasValue && RememberedState.Value
                        ? SixMessageBoxResult.YesRemember
                        : SixMessageBoxResult.Yes;
                } else
                    Result = SixMessageBoxResult.OK;
            });

            this.SetCommand(x => x.BlueCommand).Subscribe(x => {
                TryClose();
                Result = SixMessageBoxResult.Cancel;
            });

            this.SetCommand(x => x.RedCommand).Subscribe(x => {
                TryClose();
                if (Buttons == MessageBoxButton.YesNo || Buttons == MessageBoxButton.YesNoCancel) {
                    Result = RememberedState.HasValue && RememberedState.Value
                        ? SixMessageBoxResult.NoRemember
                        : SixMessageBoxResult.No;
                } else
                    Result = SixMessageBoxResult.Cancel;
            });

            this.SetCommand(x => x.CancelCommand).Subscribe(x => {
                TryClose();
                Result = SixMessageBoxResult.Cancel;
            });
        }

        public SixMessageBoxResult Result { get; set; } = SixMessageBoxResult.None;
        public MessageBoxButton Buttons
        {
            get { return _buttons; }
            set
            {
                SetProperty(ref _buttons, value);
                SetupButtons(_buttons);
            }
        }
        public bool? RememberedState
        {
            get { return _rememberedState; }
            set { SetProperty(ref _rememberedState, value); }
        }
        public ReactiveCommand CancelCommand { get; protected set; }
        public ReactiveCommand GreenCommand { get; protected set; }
        public ReactiveCommand BlueCommand { get; protected set; }
        public ReactiveCommand RedCommand { get; protected set; }
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        public string GreenButtonContent
        {
            get { return _greenButtonContent; }
            set { SetProperty(ref _greenButtonContent, value); }
        }
        public string BlueButtonContent
        {
            get { return _blueButtonContent; }
            set { SetProperty(ref _blueButtonContent, value); }
        }
        public string RedButtonContent
        {
            get { return _redButtonContent; }
            set { SetProperty(ref _redButtonContent, value); }
        }

        void SetupButtons(MessageBoxButton buttons) {
            switch (buttons) {
            case MessageBoxButton.OK:
                GreenButtonContent = "ok";
                BlueButtonContent = null;
                RedButtonContent = null;
                break;
            case MessageBoxButton.OKCancel:
                GreenButtonContent = "ok";
                BlueButtonContent = null;
                RedButtonContent = "cancel";
                break;
            case MessageBoxButton.YesNo:
                GreenButtonContent = "yes";
                BlueButtonContent = null;
                RedButtonContent = "no";
                break;
            case MessageBoxButton.YesNoCancel:
                GreenButtonContent = "yes";
                BlueButtonContent = "cancel";
                RedButtonContent = "no";
                break;
            }
        }
    }
}