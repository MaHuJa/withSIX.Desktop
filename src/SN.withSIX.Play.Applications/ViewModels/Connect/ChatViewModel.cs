// <copyright company="SIX Networks GmbH" file="ChatViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Applications.Extensions;
using SN.withSIX.Play.Applications.Services;
using SN.withSIX.Play.Core.Connect;
using ReactiveCommand = ReactiveUI.Legacy.ReactiveCommand;

namespace SN.withSIX.Play.Applications.ViewModels.Connect
{
    [DoNotObfuscate]
    public class ChatViewModel : ViewModelBase<IChat>, IDisposable
    {
        readonly object _chatMessagesLock = new object();
        readonly ContactList _contactList;
        ChatInput _chatMessageEditor;
        ICollectionView _chatMessagesView;
        ChatMessage _selectedChatMessage;

        public ChatViewModel(IChat model, ContactList contactList) : base(model) {
            _contactList = contactList;
            this.SetCommand(x => x.SendMessageCommand).RegisterAsyncTask(PostChatMessage).Subscribe();

            this.WhenAnyValue(x => x.SelectedChatMessage)
                .Where(x => x != null)
                .Subscribe(x => x.IsUnread = false);

            UiHelper.TryOnUiThread(() => {
                var cha = Model as Chat;
                if (cha != null) {
                    Messages = cha.Messages.CreateDerivedCollection(x => x);
                    Messages.EnableCollectionSynchronization(_chatMessagesLock);
                    ChatMessagesView =
                        ((IList) Messages).SetupDefaultCollectionView(new[]
                        {new SortDescription("CreatedAt", ListSortDirection.Ascending)});
                } else {
                    var cha2 = Model as PrivateChat;
                    if (cha2 != null) {
                        Messages = cha2.Messages.CreateDerivedCollection(x => (ChatMessage) x);
                        Messages.EnableCollectionSynchronization(_chatMessagesLock);
                        ChatMessagesView =
                            ((IList) Messages).SetupDefaultCollectionView(new[]
                            {new SortDescription("CreatedAt", ListSortDirection.Ascending)});
                    } else
                        throw new Exception("Unsupported chat type");
                }
            });
        }

        public IReactiveDerivedList<ChatMessage> Messages { get; private set; }
        public ICollectionView ChatMessagesView
        {
            get { return _chatMessagesView; }
            protected set { SetProperty(ref _chatMessagesView, value); }
        }
        public ChatMessage SelectedChatMessage
        {
            get { return _selectedChatMessage; }
            set { SetProperty(ref _selectedChatMessage, value); }
        }
        public ReactiveCommand SendMessageCommand { get; private set; }
        public ChatInput ChatMessageEditor
        {
            get { return _chatMessageEditor ?? (_chatMessageEditor = new ChatInput()); }
            set { SetProperty(ref _chatMessageEditor, value); }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ChatViewModel() {
            Dispose(false);
        }

        [ReportUsage]
        public Task PostChatMessage() {
            var cm = ChatMessageEditor;
            if (String.IsNullOrWhiteSpace(cm.Body))
                return null;
            ChatMessageEditor = new ChatInput();
            return _contactList.SendMessage(Model, cm);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing)
                Messages.Dispose();
            BindingOperations.DisableCollectionSynchronization(Messages);
        }
    }
}