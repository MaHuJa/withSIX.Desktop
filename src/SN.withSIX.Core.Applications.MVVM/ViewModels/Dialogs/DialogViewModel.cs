// <copyright company="SIX Networks GmbH" file="DialogViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Applications.MVVM.ViewModels.Dialogs
{
    /*public interface IDialogViewModel : IScreen
    {
        bool IsShowing { get; set; }
        void Yes();
        void No();
        void Cancel();

        UserDialog ShowDialog(string message, string title = null, MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.None);
    }

    public class DialogViewModel : ScreenBase<IShellViewModelBase>, IDialogViewModel
    {
        readonly IDialogManager _dm;
        bool _canCancel = true;
        bool _isShowing;
        UserDialog _model;

        public DialogViewModel(IDialogManager dm) {
            _dm = dm;
        }

        public bool CanCancel {
            get { return _canCancel; }
            set { SetProperty(ref _canCancel, value); }
        }

        public UserDialog Model {
            get { return _model; }
            set { SetProperty(ref _model, value); }
        }

        public bool IsShowing {
            get { return _isShowing; }
            set { SetProperty(ref _isShowing, value); }
        }

        public UserDialog ShowDialog(string message, string title = null, MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.None) {
            if (buttons == MessageBoxButton.OKCancel || buttons == MessageBoxButton.YesNoCancel)
                CanCancel = true;
            Model = new UserDialog(this, "Hey ima message box!", "and ima title of the box!", buttons, image);
            IsShowing = true;
            return Model;
        }

        [DoNotObfuscate]
        public void Cancel() {
            _dm.MsgLegacy(new MessageBoxDialogParams("Cancelled!"));
            IsShowing = false;
            TryClose(null);
        }

        [DoNotObfuscate]
        public void Yes() {
            TryClose(true);
            IsShowing = false;
        }

        [DoNotObfuscate]
        public void No() {
            TryClose(false);
            IsShowing = false;
        }
    }*/
}