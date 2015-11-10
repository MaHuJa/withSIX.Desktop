// <copyright company="SIX Networks GmbH" file="ModuleViewModelBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using Caliburn.Micro;
using ReactiveUI.Legacy;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Applications.ViewModels.Games;
using SN.withSIX.Play.Applications.ViewModels.Games.Overlays;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Core;

namespace SN.withSIX.Play.Applications.ViewModels
{
    public abstract class ModuleViewModelBase : ScreenLightViewModelBase<IContentViewModel>, IHaveOverlayConductor
    {
        IProgressState _progressState;

        protected ModuleViewModelBase() {
            this.SetCommand(x => x.SwitchEnabled).Subscribe(() => {
                if (IsActive)
                    TryClose(true);
                else
                    Open();
            });

            this.SetCommand(x => x.Show).Subscribe(Open);

            ProgressState = new ProgressState {IsIndeterminate = true, Active = true};
            Overlay = new OverlayConductor();
            Overlay.ConductWith(this);
        }

        public IProgressState ProgressState
        {
            get { return _progressState; }
            set { SetProperty(ref _progressState, value); }
        }
        [Browsable(false)]
        public ReactiveCommand Show { get; private set; }
        [Browsable(false)]
        public ReactiveCommand SwitchEnabled { get; protected set; }
        [Browsable(false)]
        public ControllerModules ModuleName { get; protected set; }
        public OverlayConductor Overlay { get; }

        protected void Open() {
            ParentShell.ActivateItem(this);
        }

        public void ShowOverlay(OverlayViewModelBase overlay) {
            ParentShell.CloseOverlay();
            Overlay.ActivateItem(overlay);
        }

        public void ShowNotes(IHaveNotes note) {
            ShowOverlay(new NotesViewModel(note));
        }
    }

    public interface IHaveOverlayConductor
    {
        OverlayConductor Overlay { get; }
    }

    public interface IHaveOverlay
    {
        OverlayViewModelBase Overlay { get; }
    }
}