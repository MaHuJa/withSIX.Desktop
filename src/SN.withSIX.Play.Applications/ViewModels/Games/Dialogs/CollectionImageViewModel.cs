// <copyright company="SIX Networks GmbH" file="CollectionImageViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.Win32;
using NDepend.Path;
using ReactiveUI.Legacy;
using ShortBus;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Applications.DataModels;
using SN.withSIX.Play.Applications.UseCases;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Dialogs
{
    public interface ICollectionImageViewModel {}

    [DoNotObfuscate]
    public class CollectionImageViewModel : DialogBase, ICollectionImageViewModel
    {
        readonly IDialogManager _dialogManager;
        readonly IMediator _mediator;
        bool _generateExecuting;
        bool _isExecuting;
        IAbsoluteFilePath _localFile;
        bool _okExecuting;

        public CollectionImageViewModel(IMediator mediator, IDialogManager dialogManager) {
            DisplayName = "Change your Collection Image";
            _mediator = mediator;
            _dialogManager = dialogManager;
            this.SetCommand(x => x.OkCommand).RegisterAsyncTask(Close);
            this.SetCommand(x => x.GenerateImageCommand).RegisterAsyncTask(GenerateImage);
            this.SetCommand(x => x.SelectImageCommand).Subscribe(SelectImage);

            OkCommand.IsExecuting.Subscribe(x => {
                _okExecuting = x;
                IsExecuting = _okExecuting || _generateExecuting;
            });

            GenerateImageCommand.IsExecuting.Subscribe(x => {
                _generateExecuting = x;
                IsExecuting = _okExecuting || _generateExecuting;
            });
        }

        public CollectionImageDataModel Content { get; set; }
        public ReactiveCommand GenerateImageCommand { get; private set; }
        public ReactiveCommand SelectImageCommand { get; private set; }
        public ReactiveCommand OkCommand { get; private set; }
        public bool IsExecuting
        {
            get { return _isExecuting; }
            set { SetProperty(ref _isExecuting, value); }
        }

        async Task Close() {
            if (_localFile != null) {
                Exception e;
                try {
                    await
                        _mediator.RequestAsync(new UploadNewCollectionImageCommand(Content.Id,
                            _localFile)).ConfigureAwait(false);
                    TryClose(true);
                    return;
                } catch (SizeExtensions.UnsupportedFileSizeException ex) {
                    e = ex;
                }
                await
                    _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(e.Message,
                        "The file size is not supported"));
                return;
            }

            TryClose(true);
        }

        public void SetContent(CollectionImageDataModel content) {
            if (content.Image == null) {
                content.Image =
                    "pack://application:,,,/SN.withSIX.Core.Presentation.Resources;component/images/ModsPlaceholder-full232x112.png";
            }
            Content = content;
        }

        Task GenerateImage() {
            _localFile = null;
            return _mediator.RequestAsync(new GenerateNewCollectionImageCommand(Content.Id));
        }

        void SelectImage() {
            var fileDialog = new OpenFileDialog {Multiselect = false, Filter = "Images|*.png;*.jpg;*.jpeg"};
            if (!fileDialog.ShowDialog().GetValueOrDefault())
                return;
            _localFile = fileDialog.FileName.ToAbsoluteFilePath();
            Content.Image = fileDialog.FileName;
        }
    }

    public class CollectionImageDataModel : DataModelRequireId<Guid>
    {
        string _image;
        public CollectionImageDataModel(Guid id) : base(id) {}
        public string Name { get; set; }
        public string Image
        {
            get { return _image; }
            set { SetProperty(ref _image, value); }
        }
    }
}