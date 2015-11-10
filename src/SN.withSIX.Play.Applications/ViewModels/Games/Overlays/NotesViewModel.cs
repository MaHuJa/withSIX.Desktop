// <copyright company="SIX Networks GmbH" file="NotesViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ReactiveUI.Legacy;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.ViewModels.Overlays;
using SN.withSIX.Play.Core;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Overlays
{
    [DoNotObfuscate]
    public class NotesViewModel : OverlayViewModelBase
    {
        IHaveNotes _item;

        public NotesViewModel(IHaveNotes item) {
            Item = item;
            DisplayName = "Notes";
            this.SetCommand(x => x.CloseNoteCommand).Subscribe(CloseNote);
        }

        public ReactiveCommand CloseNoteCommand { get; private set; }
        public IHaveNotes Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value); }
        }

        [ReportUsage]
        void CloseNote(object x) {
            TryClose(true);
        }
    }
}