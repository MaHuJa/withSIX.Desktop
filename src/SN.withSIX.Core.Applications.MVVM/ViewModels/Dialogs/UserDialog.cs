// <copyright company="SIX Networks GmbH" file="UserDialog.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Applications.MVVM.ViewModels.Dialogs
{
    /*public class UserDialog : SelectionCollectionHelper<MenuItem>
    {
        readonly IDialogViewModel _dialogViewModel;
        string _message;
        string _title;

        public UserDialog(IDialogViewModel vm, string message, string title = null,
            MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None) {
            _dialogViewModel = vm;
            _message = message;
            _title = title;

            if (buttons == MessageBoxButton.YesNo || buttons == MessageBoxButton.YesNoCancel) {
                Items.Add(new MenuItem("yes", (Action) Yes));
                Items.Add(new MenuItem("no", (Action) No));
            } else
                Items.Add(new MenuItem("ok", (Action) Ok));
        }

        public string Message {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public string Title {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public void Cancel() {
            _dialogViewModel.Cancel();
        }

        void No() {
            _dialogViewModel.No();
        }

        void Yes() {
            _dialogViewModel.Yes();
        }

        void Ok() {
            _dialogViewModel.Yes();
        }
    }*/
}