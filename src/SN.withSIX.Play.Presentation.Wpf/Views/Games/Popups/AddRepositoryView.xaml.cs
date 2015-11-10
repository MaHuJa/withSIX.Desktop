// <copyright company="SIX Networks GmbH" file="AddRepositoryView.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Linq;
using System.Windows;
using ReactiveUI;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Presentation.Wpf;
using SN.withSIX.Core.Presentation.Wpf.Views.Controls;
using SN.withSIX.Play.Applications.ViewModels.Games.Popups;
using SN.withSIX.Play.Applications.Views.Games.Popups;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Games.Popups
{
    public partial class AddRepositoryView : PopupControl, IAddRepositoryView
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof (IAddRepositoryViewModel),
                typeof (AddRepositoryView),
                new PropertyMetadata(null));
        readonly IDialogManager _dialogManager;

        public AddRepositoryView(IDialogManager dialogManager) {
            _dialogManager = dialogManager;
            InitializeComponent();
            this.WhenActivated(d => {
                d(UserError.RegisterHandler<RepositoryDownloadUserError>(x => UiRoot.Main.ErrorDialog(x)));
                d(this.WhenAnyValue(x => x.ViewModel).BindTo(this, v => v.DataContext));
                var obs = this.WhenAnyObservable(x => x.ViewModel.AddRepoCommand.IsExecuting);
                d(obs.BindTo(this, v => v.ProgressRing.IsActive));
                d(obs.Select(x => !x).BindTo(this, v => v.Url.IsEnabled));
                Url.Focus();
            });
        }

        public IAddRepositoryViewModel ViewModel
        {
            get { return (IAddRepositoryViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IAddRepositoryViewModel) value; }
        }
    }
}