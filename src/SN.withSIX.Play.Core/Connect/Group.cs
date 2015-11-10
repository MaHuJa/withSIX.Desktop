// <copyright company="SIX Networks GmbH" file="Group.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using NDepend.Path;
using ReactiveUI;
using SN.withSIX.Core;
using SN.withSIX.Core.Logging;
using SN.withSIX.Play.Core.Connect.Infrastructure;

namespace SN.withSIX.Play.Core.Connect
{
    public class Group : ConnectModelBase, IEntity
    {
        Uri _avatar;
        Uri _backgroundUrl;
        GroupChat _chat;
        Uri _homepage;
        bool _isMine;
        bool _isUploading;
        ReactiveList<Account> _members;
        string _name;
        Account _owner;
        string _slug;
        string _uploadMessage;

        public Group(Guid id) : base(id) {
            _members = new ReactiveList<Account>();
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public ReactiveList<Account> Members
        {
            get { return _members; }
            set { SetProperty(ref _members, value); }
        }
        public Account Owner
        {
            get { return _owner; }
            set { SetProperty(ref _owner, value); }
        }
        public bool IsMine
        {
            get { return _isMine; }
            set { SetProperty(ref _isMine, value); }
        }
        public bool IsUploading
        {
            get { return _isUploading; }
            set { SetProperty(ref _isUploading, value); }
        }
        public string UploadMessage
        {
            get { return _uploadMessage; }
            set { SetProperty(ref _uploadMessage, value); }
        }
        public Uri BackgroundUrl
        {
            get { return _backgroundUrl; }
            set { SetProperty(ref _backgroundUrl, value); }
        }
        public Uri Homepage
        {
            get { return _homepage; }
            set { SetProperty(ref _homepage, value); }
        }
        public GroupChat Chat
        {
            get { return _chat; }
            set { SetProperty(ref _chat, value); }
        }
        public string Slug
        {
            get { return _slug; }
            set { SetProperty(ref _slug, value); }
        }
        public Uri Avatar
        {
            get { return _avatar; }
            set { SetProperty(ref _avatar, value); }
        }
        public string DisplayName
        {
            get { return Name; }
        }

        public Uri GetUri() {
            return Tools.Transfer.JoinUri(CommonUrls.ConnectUrl, "groups", Slug);
        }

        public Uri GetOnlineConversationUrl() {
            return Tools.Transfer.JoinUri(GetUri(), "messages");
        }

        public async Task UploadAvatars(IConnectApiHandler handler, IAbsoluteFilePath logoFileName,
            IAbsoluteFilePath backgroundFileName) {
            // TODO: Add progress reporting on group object, and make visible in the UI.
            IsUploading = true;
            try {
                if (logoFileName != null)
                    await UploadLogo(handler, logoFileName).ConfigureAwait(false);
                //UploadProgress = ...
                if (backgroundFileName != null)
                    await UploadBackground(handler, backgroundFileName).ConfigureAwait(false);
                //UploadProgress = ...
            } finally {
                IsUploading = false;
            }
        }

        async Task UploadLogo(IConnectApiHandler handler, IAbsoluteFilePath logoFileName) {
            try {
                await handler.UploadGroupLogoPicture(logoFileName, Id).ConfigureAwait(false);
            } catch (Exception e) {
                UploadMessage = UploadMessage + e.Message + "\n"; // TODO: Better?
                MainLog.Logger.FormattedWarnException(e, "Error during group logo upload");
            }
        }

        async Task UploadBackground(IConnectApiHandler handler, IAbsoluteFilePath backgroundFileName) {
            try {
                await handler.UploadGroupBackgroundPicture(backgroundFileName, Id).ConfigureAwait(false);
            } catch (Exception e) {
                UploadMessage = UploadMessage + e.Message + "\n"; // TODO: Better?
                MainLog.Logger.FormattedWarnException(e, "Error during group background upload");
            }
        }
    }

    public class GroupImageUploadFailedEvent
    {
        public GroupImageUploadFailedEvent(Group @group, string uploadMessage) {
            Group = @group;
            UploadMessage = uploadMessage;
        }

        public Group Group { get; private set; }
        public string UploadMessage { get; private set; }
    }

    public class ImageFileSizeTooLargeException : Exception
    {
        public ImageFileSizeTooLargeException(string message, Exception inner) : base(message, inner) {}
    }
}