// <copyright company="SIX Networks GmbH" file="ConnectApiHandler.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Web;
using Amazon.S3.Util;
using AutoMapper;
using AutoMapper.Mappers;
using NDepend.Path;
using ReactiveUI;
using ShortBus;
using SN.withSIX.Api.Models;
using SN.withSIX.Api.Models.Blog.Post;
using SN.withSIX.Api.Models.Chat;
using SN.withSIX.Api.Models.Collections;
using SN.withSIX.Api.Models.Content;
using SN.withSIX.Api.Models.Content.Arma3;
using SN.withSIX.Api.Models.Context;
using SN.withSIX.Api.Models.Exceptions;
using SN.withSIX.Api.Models.Extensions;
using SN.withSIX.Api.Models.Shared;
using SN.withSIX.Api.Models.Social;
using SN.withSIX.Api.Models.Statistics.PlayedServers;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Errors;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Core.Connect;
using SN.withSIX.Play.Core.Connect.Infrastructure;
using SN.withSIX.Play.Core.Options;
using ServerAddress = SN.withSIX.Play.Core.ServerAddress;

namespace SN.withSIX.Play.Infra.Api.ConnectApi
{
    class AvatarCalc {
        public static string GetAvatarURL(AccountInfo account) {
            return GetAvatarURL(account, 72);
        }

        public static string GetAvatarURL(AccountInfo account, int size) {
            return account.AvatarURL == null
                ? GetGravatarUrl(account.EmailMd5, size)
                : GetCustomAvatarUrl(account.AvatarURL, account.AvatarUpdatedAt.GetValueOrDefault(0), size);
        }

        public static string GetAvatarURL(string avatarUrl, long? avatarUpdatedAt, string emailMd5, int size = 72) {
            return avatarUrl == null
                ? GetGravatarUrl(emailMd5, size)
                : GetCustomAvatarUrl(avatarUrl, avatarUpdatedAt.GetValueOrDefault(0), size);
        }

        static string GetGravatarUrl(string emailMd5, int size) {
            return "//www.gravatar.com/avatar/" + emailMd5 +
                   "?size=" + size + "&amp;d=%2f%2faz667488.vo.msecnd.net%2fimg%2favatar%2fnoava_" +
                   size + ".jpg";
        }

        static string GetCustomAvatarUrl(string avatarUrl, long avatarUpdatedAt, int size) {
            var v = "?v=" + avatarUpdatedAt;
            return avatarUrl + size + "x" + size + ".jpg" + v;
        }
    }

    class ConnectApiHandler : PropertyChangedBase, IInfrastructureService, IConnectApiHandler
    {
        static readonly string defaultAvaImg =
            HttpUtility.UrlEncode("http://withsix-assets.s3-eu-west-1.amazonaws.com/img/avatar/placeholder_40.png");
        readonly IConnectionManager _connectionManager;
        readonly MappingEngine _mappingEngine;
        readonly IMediator _mediator;
        readonly IExceptionHandler _exHandler;
        readonly ITokenRefresher _tokenRefresher;
        AccountConnectApiRepository _accountRepository;
        GroupChatConnectApiRepository _groupChatRepository;
        GroupConnectApiRepository _groupRepository;
        PrivateChatConnectApiRepository _privateChatApiRepository;
        PublicChatConnectApiRepository _publicChatRepository;
        ServerAddress _serverAddress;
        Guid _serverAddressSession;

        public ConnectApiHandler(IConnectionManager connectionManager, ITokenRefresher tokenRefresher,
            IMediator mediator, IExceptionHandler exHandler) {
            _connectionManager = connectionManager;
            _tokenRefresher = tokenRefresher;
            _mediator = mediator;
            _exHandler = exHandler;
            Me = new MyAccount();
            _mappingEngine = GetMapper();
            SetupRepositories();
            SetupListeners();
        }

        public IMessageBus MessageBus
        {
            get { return _connectionManager.MessageBus; }
        }

        public async Task SetServerAddress(ServerAddress serverAddress) {
            if (serverAddress == null) {
                _serverAddress = null;
                return;
            }

            ConfirmLoggedIn();

            if (_serverAddressSession != Guid.Empty && serverAddress.Equals(_serverAddress)) {
                await
                    _connectionManager.AccountHub.UpdateServerAddressSession(_serverAddressSession)
                        .ConfigureAwait(false);
            } else {
                _serverAddressSession =
                    await
                        _connectionManager.AccountHub.CreateServerAddressSession(new StartSessionInput {
                            IpAddress = serverAddress.ToString()
                        }).ConfigureAwait(false);
                _serverAddress = serverAddress;
            }
        }

        public Task Refresh(Group group) {
            return _groupRepository.RefreshAsync(group);
        }

        public async Task<IReadOnlyCollection<Account>> SearchUsers(string search, int page = 1) {
            ConfirmLoggedIn();
            var model = new AccountSearchInputModel {
                UserName = search,
                PageInfo = new PageInfo(page, 15)
            };
            ValidateObject(model);
            return
                _mappingEngine.Map<List<Account>>(
                    (await _connectionManager.AccountHub.SearchUsers(model).ConfigureAwait(false)).Items);
        }

        public async Task<InviteRequest> AddFriendshipRequest(Account user) {
            ConfirmLoggedIn();
            var friendshipRequestModel =
                await _connectionManager.AccountHub.RequestFriendship(user.Id).ConfigureAwait(false);
            return await MapFriendshipRequest(friendshipRequestModel).ConfigureAwait(false);
        }

        public Task RemoveFriend(Account user) {
            ConfirmLoggedIn();
            return _connectionManager.AccountHub.DeleteFriend(user.Id);
        }

        public async Task<Guid> CreateGroup(ICreateGroupInfo inputModel, IAbsoluteFilePath logoFileName,
            IAbsoluteFilePath backgroundFileName) {
            ConfirmLogoConstraints(logoFileName);
            ConfirmBackgroundCOnstraints(backgroundFileName);
            ConfirmLoggedIn();

            var model = Mapper.DynamicMap<CreateGroupInputModel>(inputModel);
            ValidateObject(model);
            var id = await _connectionManager.GroupHub.CreateGroup(model).ConfigureAwait(false);
            if (id == Guid.Empty)
                throw new Exception("Failed to Create Group");

            var group =
                await
                    MapGroup(await _connectionManager.GroupHub.GetGroup(id).ConfigureAwait(false)).ConfigureAwait(false);
            Me.Groups.Add(group);
            UploadGroupImages(logoFileName, backgroundFileName, @group);

            return id;
        }

        public async Task DeleteGroup(Group group) {
            ConfirmLoggedIn();
            await _connectionManager.GroupHub.DeleteGroup(group.Id).ConfigureAwait(false);
            LeftGroup(group);
        }

        public async Task LeaveGroup(Group group) {
            ConfirmLoggedIn();
            await _connectionManager.GroupHub.LeaveGroup(group.Id).ConfigureAwait(false);
            LeftGroup(group);
        }

        public Task<CollectionModel> GetCollection(Guid collectionId) {
            ConfirmConnected();
            return _connectionManager.CollectionsHub.GetCollection(collectionId);
        }

        public Task<CollectionVersionModel> GetCollectionVersion(Guid versionId) {
            ConfirmConnected();
            return _connectionManager.CollectionsHub.GetCollectionVersion(versionId);
        }

        public async Task<CollectionPublishInfo> PublishCollection(CreateCollectionModel model) {
            ValidateObject(model);

            var accountId = DomainEvilGlobal.SecretData.UserInfo.Account.Id;
            var id = await _connectionManager.CollectionsHub.CreateNewCollection(model).ConfigureAwait(false);
            return new CollectionPublishInfo(id, accountId);
        }

        public Task<Guid> PublishNewCollectionVersion(AddCollectionVersionModel model) {
            ValidateObject(model);
            return _connectionManager.CollectionsHub.AddCollectionVersion(model);
        }

        public Task<List<CollectionModel>> GetSubscribedCollections() {
            return _connectionManager.CollectionsHub.GetSubscribedCollections();
        }

        public Task<List<CollectionModel>> GetOwnedCollections() {
            return _connectionManager.CollectionsHub.GetOwnedCollections();
        }

        public Task UnsubscribeCollection(Guid collectionID) {
            return _connectionManager.CollectionsHub.Unsubscribe(collectionID);
        }

        public Task DeleteCollection(Guid collectionId) {
            return _connectionManager.CollectionsHub.Delete(collectionId);
        }

        public Task ChangeCollectionScope(Guid collectionId, CollectionScope scope) {
            return _connectionManager.CollectionsHub.ChangeScope(collectionId, scope);
        }

        public Task ChangeCollectionName(Guid collectionId, string name) {
            return _connectionManager.CollectionsHub.UpdateCollectionName(collectionId, name);
        }

        public Task<string> GenerateNewCollectionImage(Guid id) {
            return _connectionManager.CollectionsHub.GenerateNewAvatarImage(id);
        }

        public Task AddUserToGroup(Account user, Group group) {
            ConfirmLoggedIn();
            return _connectionManager.GroupHub.AddUserToGroup(group.Id, user.Id);
        }

        public async Task UploadMission(RequestMissionUploadModel model, IAbsoluteDirectoryPath path) {
            Contract.Requires<ArgumentNullException>(model != null);
            Contract.Requires<ArgumentNullException>(path != null);

            ValidateObject(model);
            var uploadRequest = await _connectionManager.MissionsHub.RequestMissionUpload(model).ConfigureAwait(false);
            await UploadFileToAWS(path.GetChildFileWithName(model.FileName), uploadRequest).ConfigureAwait(false);
            var uploadedModel = new MissionUploadedModel {
                GameSlug = model.GameSlug,
                Name = model.Name,
                UploadKey = uploadRequest.Key
            };
            ValidateObject(uploadedModel);
            await _connectionManager.MissionsHub.MissionUploadCompleted(uploadedModel).ConfigureAwait(false);
        }

        public Task<PageModel<MissionModel>> GetMyMissions(string type, int page) {
            return _connectionManager.MissionsHub.GetMyMissions(type, page);
        }

        public Task RemoveUserFromGroup(Account user, Group group) {
            ConfirmLoggedIn();
            return _connectionManager.GroupHub.RemoveUserFromGroup(group.Id, user.Id);
        }

        public async Task Initialize(string key) {
            if (!key.IsBlankOrWhiteSpace()) {
                try {
                    await TryConnect(key).ConfigureAwait(false);
                } catch (RefreshTokenInvalidException ex) {
                    throw new UnauthorizedException("Refresh token invalid", ex);
                }
            } else
                await TryDisconnect().ConfigureAwait(false);
        }

        public MyAccount Me { get; }

        public async Task<GroupChat> GetGroupChat(Group group) {
            ConfirmLoggedIn();
            var chat =
                _mappingEngine.Map<GroupChat>(await _connectionManager.ChatHub.GetChat(group.Id).ConfigureAwait(false));
            chat.Group = group;
            chat.UpdateOrAddMessages(await GetChatMessages(chat).ConfigureAwait(false));
            group.Chat = chat;

            return chat;
        }

        public async Task<PrivateChat> GetPrivateChat(Friend friend) {
            ConfirmLoggedIn();
            var chat = _mappingEngine.Map<PrivateChat>(new PrivateChatModel {Id = friend.Account.Id});
            // TODO: These should be handled through the PrivateChatModel?
            chat.Friend = friend;
            chat.User = friend.Account;
            chat.UpdateOrAddMessages(await GetPrivateChatMessages(friend.Account).ConfigureAwait(false));
            return chat;
        }

        public async Task<List<PrivateMessage>> GetPrivateChatMessages(Account user) {
            ConfirmLoggedIn();
            return
                await
                    MapChatMessages(
                        (await _connectionManager.ChatHub.GetPrivateChatMessages(user.Id, 1).ConfigureAwait(false))
                            .Items)
                        .ConfigureAwait(false);
        }

        public Task HandleAuthentication(string code, Uri localCallback) {
            return _tokenRefresher.HandleAuthentication(code, localCallback);
        }

        public async Task<PrivateMessage> SendPrivateChatMessage(ChatInput cm, PrivateChat privateChat) {
            ValidateObject(cm);
            ConfirmLoggedIn();
            return
                await
                    MapChatMessage(
                        await
                            _connectionManager.ChatHub.SendPrivateChatMessage(privateChat.Id, cm.Body)
                                .ConfigureAwait(false))
                        .ConfigureAwait(false);
        }

        public async Task<ChatMessage> SendPublicChatMessage(ChatInput cm, PublicChat publicChat) {
            ValidateObject(cm);
            ConfirmLoggedIn();
            return
                await
                    MapChatMessage(
                        await
                            _connectionManager.ChatHub.SendChatMessage(publicChat.ObjectUuid, cm.Body)
                                .ConfigureAwait(false))
                        .ConfigureAwait(false);
        }

        public async Task<ChatMessage> SendGroupChatMessage(ChatInput cm, GroupChat groupChat) {
            ValidateObject(cm);
            ConfirmLoggedIn();
            return
                await
                    MapChatMessage(
                        await
                            _connectionManager.ChatHub.SendGroupChatMessage(groupChat.Group.Id, cm.Body)
                                .ConfigureAwait(false))
                        .ConfigureAwait(false);
        }

        public async Task<Friend> ApproveFriend(InviteRequest request) {
            ConfirmLoggedIn();
            var friendshipModel =
                await _connectionManager.AccountHub.AcceptFriendship(request.Id).ConfigureAwait(false);
            return await CreateFriend(friendshipModel.Friend.Id).ConfigureAwait(false);
        }

        public Task DeclineFriend(InviteRequest request) {
            ConfirmLoggedIn();
            return _connectionManager.AccountHub.DeclineFriendship(request.Id);
        }

        public Task MarkAsRead(Account account) {
            ConfirmLoggedIn();
            return _connectionManager.ChatHub.MarkPrivateChatAsRead(account.Id);
        }

        public Task UploadGroupBackgroundPicture(IAbsoluteFilePath backgroundFileName, Guid groupId) {
            ConfirmLoggedIn();
            SizeExtensions.DefaultContentLengthRange.VerifySize(backgroundFileName.FileInfo.Length);
            return UploadGroupBackgroundPicture(File.ReadAllBytes(backgroundFileName.ToString()), groupId);
        }

        public Task UploadGroupLogoPicture(IAbsoluteFilePath logoFileName, Guid groupId) {
            ConfirmLoggedIn();
            SizeExtensions.DefaultContentLengthRange.VerifySize(logoFileName.FileInfo.Length);
            return UploadGroupLogoPicture(File.ReadAllBytes(logoFileName.ToString()), groupId);
        }

        public Task<Group> GetGroup(Guid groupId) {
            ConfirmLoggedIn();
            return _groupRepository.GetOrRetrieveAndAddAsync(groupId);
        }

        public Task<Account> GetAccount(Guid id) {
            ConfirmConnected();
            return _accountRepository.GetOrRetrieveAndAddAsync(id);
        }

        public void ConfirmLoggedIn() {
            ConfirmConnected();
            if (!_connectionManager.IsLoggedIn())
                throw new NotLoggedInException();
        }

        public void ConfirmConnected() {
            if (!_connectionManager.IsLoggedIn())
                throw new NotConnectedException();
        }

        public async Task<string> UploadCollectionAvatar(IAbsoluteFilePath imagePath, Guid collectionId) {
            var uploadRequest =
                await
                    _connectionManager.CollectionsHub.RequestAvatarUpload(imagePath.ToString(), collectionId)
                        .ConfigureAwait(false);
            await UploadFileToAWS(imagePath, uploadRequest);
            return await
                _connectionManager.CollectionsHub.AvatarUploadCompleted(collectionId, uploadRequest.Key)
                    .ConfigureAwait(false);
        }

        public async Task<List<ChatMessage>> GetChatMessages(GroupChat chat) {
            ConfirmLoggedIn();
            return await
                MapChatMessages(
                    (await _connectionManager.ChatHub.GetChatMessages(chat.Group.Id, 1).ConfigureAwait(false)).Items)
                    .ConfigureAwait(false);
        }

        public async Task<List<ChatMessage>> GetChatMessages(PublicChat chat) {
            ConfirmLoggedIn();
            return await
                MapChatMessages(
                    (await _connectionManager.ChatHub.GetChatMessages(chat.ObjectUuid, 1).ConfigureAwait(false)).Items)
                    .ConfigureAwait(false);
        }

        async Task TryDisconnect() {
            Exception e = null;
            try {
                await _connectionManager.Stop().ConfigureAwait(false);
            } catch (Exception ex) {
                e = ex;
            }
            await _tokenRefresher.Logout().ConfigureAwait(false);
            if (e != null)
                throw e; // pff
        }

        async Task TryConnect(string key) {
            ExceptionDispatchInfo e;
            try {
                await _connectionManager.Start(key).ConfigureAwait(false);
                UpdateAccount(await GetMyAccount().ConfigureAwait(false));
                Me.PublicChats.UpdateOrAdd(await GetPublicChat().ConfigureAwait(false));
                return;
            } catch (Exception ex) {
                // cannot await here...
                e = ExceptionDispatchInfo.Capture(ex);
            }
            _tokenRefresher.Logout().ConfigureAwait(false);
            e.Throw(); // pff
        }

        // ReSharper disable once InconsistentNaming
        Task UploadFileToAWS(IAbsoluteFilePath filePath, AWSUploadPolicy uploadRequest) {
            return UploadToAWS(uploadRequest, File.OpenRead(filePath.ToString()));
        }

        // ReSharper disable once InconsistentNaming
        async Task UploadToAWS(AWSUploadPolicy uploadRequest, FileStream inputStream) {
            var s3PostUploadSignedPolicy =
                S3PostUploadSignedPolicy.GetSignedPolicyFromJson(uploadRequest.EncryptedPolicy);
            s3PostUploadSignedPolicy.SecurityToken = uploadRequest.SecurityToken;
            var uploadResponse = await Task.Run(() => AmazonS3Util.PostUpload(new S3PostUploadRequest {
                Key = uploadRequest.Key,
                Bucket = uploadRequest.BucketName,
                CannedACL = uploadRequest.ACL,
                ContentType = uploadRequest.ContentType,
                SuccessActionRedirect = uploadRequest.CallbackUrl,
                InputStream = inputStream,
                SignedPolicy = s3PostUploadSignedPolicy
            })).ConfigureAwait(false);
            if (uploadResponse.StatusCode != HttpStatusCode.OK)
                throw new Exception("Amazon upload failed: " + uploadResponse.StatusCode);
        }

        static readonly DataAnnotationsValidator.DataAnnotationsValidator validator =
    new DataAnnotationsValidator.DataAnnotationsValidator();


        static void ValidateObject(object model) {
            validator.ValidateObject(model);
        }

        void ConfirmBackgroundCOnstraints(IAbsoluteFilePath backgroundFileName) {
            try {
                if (backgroundFileName != null)
                    CheckFileSize(backgroundFileName, 800);
            } catch (ArgumentOutOfRangeException e) {
                throw new ImageFileSizeTooLargeException(
                    "The background image file size exceeds the maximum of 800 KB", e);
            }
        }

        void ConfirmLogoConstraints(IAbsoluteFilePath logoFileName) {
            try {
                if (logoFileName != null)
                    CheckFileSize(logoFileName, 800);
            } catch (ArgumentOutOfRangeException e) {
                throw new ImageFileSizeTooLargeException("The logo image file size exceeds the maximum of 800 KB", e);
            }
        }

        Task UploadGroupLogoPicture(byte[] picture, Guid groupId) {
            return UploadPicture(Convert.ToBase64String(picture),
                (id, msg, partInfo) => _connectionManager.GroupHub.UploadGroupLogoPicture(id, groupId, msg, partInfo));
        }

        Task UploadCollectionPicture(byte[] picture, Guid groupId) {
            return UploadPicture(Convert.ToBase64String(picture),
                (id, msg, partInfo) => _connectionManager.CollectionsHub.UploadAvatar(id, groupId, msg, partInfo));
        }

        Task UploadGroupBackgroundPicture(byte[] picture, Guid groupId) {
            return UploadPicture(Convert.ToBase64String(picture),
                (id, msg, partInfo) =>
                    _connectionManager.GroupHub.UploadGroupBackgroundPicture(id, groupId, msg, partInfo));
        }

        static async Task UploadPicture(string picture, Func<Guid, string, Tuple<int, int>, Task> act) {
            var id = Guid.NewGuid();
            var msgArray = picture.SplitBySize(16000).ToArray();
            var i = 0;
            var numberOfParts = msgArray.Length;
            foreach (var msg in msgArray) {
                i++;
                await act(id, msg, new Tuple<int, int>(i, numberOfParts)).ConfigureAwait(false);
            }
        }

        void LeftGroup(Group group) {
            Me.Groups.Remove(group);
            Me.GroupChats.RemoveAll(x => x.Group == group);
        }

        void CheckFileSize(IAbsoluteFilePath fileName, int maxSizeInKB) {
            Contract.Requires<FileNotFoundException>(fileName.Exists);
            Contract.Requires<ArgumentOutOfRangeException>(maxSizeInKB > 0, "maxSize must be greater than 0 KB");
            var file = File.ReadAllBytes(fileName.ToString());
            if (file.Length > maxSizeInKB*1024) {
                throw new ArgumentOutOfRangeException("fileName", (file.Length/1024) + " KB",
                    "The Size of the file was larger than the maximum allowed file size.");
            }
        }

        async void UploadGroupImages(IAbsoluteFilePath logoFileName, IAbsoluteFilePath backgroundFileName, Group @group) {
            await @group.UploadAvatars(this, logoFileName, backgroundFileName).ConfigureAwait(false);
            if (!String.IsNullOrWhiteSpace(@group.UploadMessage))
                _mediator.Notify(new GroupImageUploadFailedEvent(@group, @group.UploadMessage));
        }

        async Task<MyAccountModel> GetMyAccount() {
            await _connectionManager.SetupContext().ConfigureAwait(false);
            await ImportFriendAccounts().ConfigureAwait(false);
            return await MapAccount(_connectionManager.Context()).ConfigureAwait(false);
        }

        void SetupListeners() {
            Listen<ApiHashes>(ApiHashesReceived);
            Listen<ChatMessageReceived>(ChatMessageReceived);
            Listen<PrivateMessageReceived>(PrivateMessageReceived);
            Listen<FriendshipRequestAccepted>(FriendshipAccepted);
            Listen<FriendshipDeleted>(FriendshipDeleted);
            Listen<FriendshipRequestReceived>(FriendshipRequestRecieved);
            Listen<AddedToGroup>(AddedToGroup);
            Listen<UserAddedToGroup>(UserAddedToGroup);
            Listen<UserRemovedFromGroup>(RemovedFromGroup);
            Listen<GroupUpdated>(GroupUpdated);
            Listen<AccountStatusChanged>(OnlineStatusChange);
            Listen<FriendshipRequestCancelled>(FriendshipRequestCancelled);
            Listen<FriendshipRequestDeclined>(FriendshipRequestDeclined);
            Listen<SubscribedToCollection>(SubscribedToCollection);
            Listen<UnsubscribedFromCollection>(UnsubscribeFromCollection);
            Listen<CollectionUpdated>(CollectionUpdated);
            Listen<CollectionVersionAdded>(CollectionVersionAdded);
        }

        async Task ApiHashesReceived(ApiHashes obj) {
            await _mediator.NotifyAsync(obj).ConfigureAwait(false);
        }

        void Listen<TEvt>(Func<TEvt, Task> action) {
            _connectionManager.MessageBus.Listen<TEvt>().Subscribe(x => HandleAction(action, x));
        }

        async void HandleAction<TEvt>(Func<TEvt, Task> action, TEvt x) {
            retry:
            try {
                await action(x).ConfigureAwait(false);
            } catch (Exception ex) {
                var r = await UserError.Throw(_exHandler.HandleException(ex, "API action"));
                if (r == RecoveryOptionResult.RetryOperation)
                    goto retry;
                if (r == RecoveryOptionResult.FailOperation)
                    throw;
            }
        }

        async Task CollectionUpdated(CollectionUpdated evt) {
            await _mediator.NotifyAsync(evt).ConfigureAwait(false);
        }

        async Task CollectionVersionAdded(CollectionVersionAdded evt) {
            await _mediator.NotifyAsync(evt).ConfigureAwait(false);
        }

        async Task UnsubscribeFromCollection(UnsubscribedFromCollection evt) {
            await _mediator.NotifyAsync(evt).ConfigureAwait(false);
        }

        async Task SubscribedToCollection(SubscribedToCollection evt) {
            await _mediator.NotifyAsync(evt).ConfigureAwait(false);
        }

        async Task OnlineStatusChange(AccountStatusChanged statusChange) {
            var friend = Me.Friends.FirstOrDefault(x => x.Id == statusChange.AccountId);
            //var account = _accountRepository.Get(statusChange.AccountId);
            if (friend != null)
                friend.Status = statusChange.Status;
        }

        async Task<PublicChat> GetPublicChat() {
            var chat =
                _mappingEngine.Map<PublicChat>(
                    await _connectionManager.ChatHub.GetChat(PublicChat.GlobalChatObjectId).ConfigureAwait(false));
            chat.UpdateOrAddMessages(await GetChatMessages(chat).ConfigureAwait(false));
            return chat;
        }

        async Task FriendshipAccepted(FriendshipRequestAccepted evt) {
            Me.InviteRequests.RemoveAll(x => x.Target.Id == evt.Id);
            Me.Friends.UpdateOrAdd(await CreateFriend(evt.Id).ConfigureAwait(false));
        }

        async Task FriendshipDeleted(FriendshipDeleted fs) {
            Me.Friends.RemoveAll(x => x.Account.Id == fs.Id);
        }

        async Task FriendshipRequestRecieved(FriendshipRequestReceived x) {
            var inviteRequest = await MapFriendshipRequest(x.Request).ConfigureAwait(false);
            Me.InviteRequests.UpdateOrAdd(inviteRequest);
        }

        async Task FriendshipRequestCancelled(FriendshipRequestCancelled friendshipRequest) {
            Me.InviteRequests.RemoveAll(x => x.Account.Id == friendshipRequest.Id);
        }

        async Task FriendshipRequestDeclined(FriendshipRequestDeclined friendshipRequest) {
            Me.InviteRequests.RemoveAll(x => x.Account.Id == friendshipRequest.Id);
        }

        async Task AddedToGroup(AddedToGroup x) {
            Me.Groups.Add(await GetGroup(x.Group.Id).ConfigureAwait(false));
        }

        async Task UserAddedToGroup(UserAddedToGroup x) {
            var group = _groupRepository.Get(x.Group);
            if (group != null)
                group.Members.Add(await _accountRepository.GetOrRetrieveAndAddAsync(x.User.Id));
        }

        async Task GroupUpdated(GroupUpdated x) {
            //TODO: Refresh Group Information (Marasmus)
            var updatedGroup = await _connectionManager.GroupHub.GetGroup(x.GroupId).ConfigureAwait(false);
            await
                _groupRepository.RefreshAsync(await MapGroup(updatedGroup).ConfigureAwait(false)).ConfigureAwait(false);
        }

        async Task RemovedFromGroup(UserRemovedFromGroup msg) {
            var group = _groupRepository.Get(msg.Group);
            if (msg.User == DomainEvilGlobal.SecretData.UserInfo.Account.Id) {
                Me.Groups.RemoveAll(x => x.Id == group.Id);
                Me.GroupChats.RemoveAll(x => x.Group == group);
                _groupRepository.Remove(group);
                return;
            }

            @group.Members.RemoveAll(acc => acc.Id == msg.User);
        }

        async Task PrivateMessageReceived(PrivateMessageReceived msg) {
            // TODO: We probably want to set unread status differently?
            Me.UnreadPrivateMessages += 1;
            var friend = Me.Friends.FirstOrDefault(y => y.Id == msg.Message.SenderId);
            if (friend != null)
                friend.UnreadPrivateMessages += 1;

            // TODO: This adds to the repository when private chat not yet open, but does not inform the app/domain
            var chat = _privateChatApiRepository.GetOrCreateAndAdd(msg.Id);
            chat.UpdateOrAddMessage(await MapChatMessage(msg.Message).ConfigureAwait(false));
        }

        async Task ChatMessageReceived(ChatMessageReceived x) {
            var publicChat = _publicChatRepository.Get(x.ObjectId);
            if (publicChat != null)
                publicChat.UpdateOrAddMessage(await MapChatMessage(x.Message).ConfigureAwait(false));
            var groupChat = _groupChatRepository.Get(x.ObjectId);
            if (groupChat != null)
                groupChat.UpdateOrAddMessage(await MapChatMessage(x.Message).ConfigureAwait(false));
        }

        void UpdateAccount(MyAccountModel myAccount) {
            Me.Account = myAccount.Account;
            myAccount.Friends.SyncCollectionPK(Me.Friends);
            myAccount.Groups.SyncCollectionPK(Me.Groups);
            myAccount.InviteRequests.SyncCollectionPK(Me.InviteRequests);
            myAccount.UnreadPrivateMessages = Me.UnreadPrivateMessages;
            foreach (var f in Me.Friends)
                _privateChatApiRepository.GetOrCreateAndAdd(f.Id);
        }

        async Task<List<ChatMessage>> MapChatMessages(IEnumerable<ChatMessageModel> chatMessages) {
            var messages = new List<ChatMessage>();
            foreach (var m in chatMessages)
                messages.Add(await MapChatMessage(m).ConfigureAwait(false));
            return messages;
        }

        async Task<List<PrivateMessage>> MapChatMessages(IEnumerable<PrivateMessageModel> privateMessages) {
            var messages = new List<PrivateMessage>();
            foreach (var m in privateMessages)
                messages.Add(await MapChatMessage(m).ConfigureAwait(false));
            return messages;
        }

        async Task<PrivateMessage> MapChatMessage(PrivateMessageModel i) {
            var pm = _mappingEngine.Map<PrivateMessage>(i);
            pm.Author =
                await _accountRepository.GetOrRetrieveAndAddAsync(i.SenderId).ConfigureAwait(false);
            pm.Receiver =
                await _accountRepository.GetOrRetrieveAndAddAsync(i.TargetId).ConfigureAwait(false);
            return pm;
        }

        async Task<Post> MapPost(PostViewModel i) {
            var p = _mappingEngine.Map<Post>(i);
            p.Author = await _accountRepository.GetOrRetrieveAndAddAsync(i.AuthorId).ConfigureAwait(false);
            return p;
        }

        async Task<MyAccountModel> MapAccount(ContextModel context) {
            var myAccountModel = new MyAccountModel {
                Account = _mappingEngine.Map<Account>(DomainEvilGlobal.SecretData.UserInfo.Account),
                Friends = await GetFriends(context).ConfigureAwait(false),
                Groups = await GetGroups().ConfigureAwait(false),
                InviteRequests = await GetInviteRequests().ConfigureAwait(false)
            };
            return myAccountModel;
        }

        async Task<ReactiveList<Friend>> GetFriends(ContextModel context) {
            var friends = new ReactiveList<Friend>();
            foreach (var f in context.FriendStatus) {
                var friend = await CreateFriend(f.FriendId).ConfigureAwait(false);
                _mappingEngine.Map(f, friend);
                friends.Add(friend);
            }
            return friends;
        }

        async Task<ReactiveList<Group>> GetGroups() {
            var groups = new ReactiveList<Group>();
            foreach (var g in await _connectionManager.GroupHub.GetUserGroups().ConfigureAwait(false))
                groups.Add(await MapGroup(g).ConfigureAwait(false));
            return groups;
        }

        async Task<ReactiveList<InviteRequest>> GetInviteRequests() {
            var inviteRequests = new ReactiveList<InviteRequest>();
            foreach (var r in await _connectionManager.AccountHub.GetFriendshipRequests().ConfigureAwait(false))
                inviteRequests.Add(await MapFriendshipRequest(r).ConfigureAwait(false));
            return inviteRequests;
        }

        async Task<Group> MapGroup(GroupModel groupModel) {
            var group = _mappingEngine.Map<Group>(groupModel);
            group.Owner = await _accountRepository.GetOrRetrieveAndAddAsync(groupModel.OwnerId).ConfigureAwait(false);
            group.IsMine = group.Owner.Id == DomainEvilGlobal.SecretData.UserInfo.Account.Id;
            var members = await _groupRepository.GetMembers(groupModel.Id).ConfigureAwait(false);
            members.SyncCollection(group.Members);
            return group;
        }

        async Task<InviteRequest> MapFriendshipRequest(FriendshipRequestModel friendshipRequestModel) {
            var inviteRequest = _mappingEngine.Map<InviteRequest>(friendshipRequestModel);
            inviteRequest.Account =
                await
                    _accountRepository.GetOrRetrieveAndAddAsync(friendshipRequestModel.AccountId)
                        .ConfigureAwait(false);
            inviteRequest.Target =
                await
                    _accountRepository.GetOrRetrieveAndAddAsync(friendshipRequestModel.TargetId).ConfigureAwait(false);
            return inviteRequest;
        }

        async Task<ChatMessage> MapChatMessage(ChatMessageModel chatMessageModel) {
            var chatMessage = _mappingEngine.Map<ChatMessage>(chatMessageModel);
            chatMessage.Author =
                await _accountRepository.GetOrRetrieveAndAddAsync(chatMessageModel.AuthorId).ConfigureAwait(false);
            return chatMessage;
        }

        async Task ImportFriendAccounts() {
            _accountRepository.Import(await GetMyFriends().ConfigureAwait(false));
        }

        async Task<IReadOnlyCollection<AccountModel>> GetMyFriends() {
            var friends =
                await
                    _connectionManager.AccountHub.GetFriends(DomainEvilGlobal.SecretData.UserInfo.Account.Id)
                        .ConfigureAwait(false);
            return friends.Select(x => x.Friend).ToArray();
        }

        void SetupRepositories() {
            // TODO: Why isn't this imported in the constructor?
            _accountRepository = new AccountConnectApiRepository(_connectionManager, _mappingEngine);
            _groupChatRepository = new GroupChatConnectApiRepository(_connectionManager, _mappingEngine);
            _groupRepository = new GroupConnectApiRepository(_connectionManager, _mappingEngine);
            _publicChatRepository = new PublicChatConnectApiRepository(_connectionManager, _mappingEngine);
            _privateChatApiRepository = new PrivateChatConnectApiRepository(_connectionManager, _mappingEngine);
        }

        MappingEngine GetMapper() {
            var mapConfig = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            mapConfig.SetupConverters();
            mapConfig.CreateMap<AccountModel, Account>()
                .ConstructUsing(input => _accountRepository.GetOrCreateAndAdd(input.Id))
                .ForMember(x => x.Avatar, opt => opt.ResolveUsing(GetAvatar));

            mapConfig.CreateMap<AccountInfo, Account>()
                .ForMember(x => x.Avatar,
                    opt => opt.MapFrom(src => new Uri("http:" + AvatarCalc.GetAvatarURL(src))))
                .ForMember(x => x.Slug, opt => opt.MapFrom(src => src.UserName.Sluggify()));

            mapConfig.CreateMap<FriendStatusModel, Friend>()
                .ConstructUsing(x => CreateFriend(x.FriendId).Result)
                .ForMember(x => x.PlayingOn,
                    opt =>
                        opt.ResolveUsing(
                            src => String.IsNullOrWhiteSpace(src.PlayingOn) ? null : new ServerAddress(src.PlayingOn)));

            mapConfig.CreateMap<FriendshipModel, Friend>()
                .ConstructUsing(input => {
                    _accountRepository.UpdateOrAdd(input.Friend);
                    return CreateFriend(input.Friend.Id).Result;
                });

            mapConfig.CreateMap<FriendshipRequestModel, InviteRequest>()
                .ConstructUsing(ConstructInviteRequest)
                .ForMember(x => x.CreatedAt, opt => opt.MapFrom(src => src.SendOn));

            mapConfig.CreateMap<GroupModel, Group>()
                .ConstructUsing(input => _groupRepository.GetOrCreateAndAdd(input.Id))
                .ForMember(x => x.Avatar, opt => opt.ResolveUsing(GetAvatar))
                .ForMember(x => x.BackgroundUrl, opt => opt.ResolveUsing(GetBackgroundUrl));

            mapConfig.CreateMap<PrivateChatModel, PrivateChat>()
                .ConstructUsing(x => _privateChatApiRepository.GetOrCreateAndAdd(x.Id))
                .ForMember(x => x.Messages, opt => opt.Ignore());

            mapConfig.CreateMap<ChatHubModel, GroupChat>()
                .ConstructUsing(input => _groupChatRepository.GetOrCreateAndAdd(input.Id))
                .ForMember(x => x.Messages, opt => opt.Ignore());

            mapConfig.CreateMap<ChatHubModel, PublicChat>()
                .ConstructUsing(input => _publicChatRepository.GetOrCreateAndAdd(input.Id))
                .ForMember(x => x.Messages, opt => opt.Ignore());

            mapConfig.CreateMap<ChatMessageModel, ChatMessage>()
                .AfterMap((src, dst) => {
                    try {
                        var acct = DomainEvilGlobal.SecretData.UserInfo.Account;
                        if (acct == null)
                            return;
                        dst.IsMyMessage = src.AuthorId == acct.Id;
                    } catch (NotLoggedInException) {}
                });

            mapConfig.CreateMap<PrivateMessageModel, PrivateMessage>()
                .ForMember(x => x.CreatedAt, opt => opt.MapFrom(src => src.SendAt))
                .ForMember(x => x.Body, opt => opt.MapFrom(src => src.Message))
                .AfterMap((src, dst) => {
                    try {
                        var acct = DomainEvilGlobal.SecretData.UserInfo.Account;
                        if (acct == null)
                            return;
                        dst.IsMyMessage = src.SenderId == acct.Id;
                    } catch (NotLoggedInException) {}
                });

            mapConfig.CreateMap<PostViewModel, Post>()
                .ForMember(x => x.CreatedAt, opt => opt.MapFrom(src => src.Created))
                .ForMember(x => x.UpdatedAt, opt => opt.MapFrom(src => src.Updated));

            mapConfig.CreateMap<PostHeaderViewModel, Post>()
                .ForMember(x => x.CreatedAt, opt => opt.MapFrom(src => src.Created))
                .ForMember(x => x.UpdatedAt, opt => opt.MapFrom(src => src.Updated));

            return new MappingEngine(mapConfig);
        }

        async Task<Friend> CreateFriend(Guid id) {
            return new Friend(await GetAccount(id).ConfigureAwait(false));
        }

        InviteRequest ConstructInviteRequest(FriendshipRequestModel src) {
            var isMe = src.AccountId == DomainEvilGlobal.SecretData.UserInfo.Account.Id;
            return new InviteRequest(isMe ? src.TargetId : src.AccountId, isMe);
        }

        static string GetAvatar(AccountModel src) {
            return String.IsNullOrWhiteSpace(src.AvatarUrl)
                ? GetGravatar(src.EmailMd5)
                : GetAvatarUrl(src.AvatarUrl, src.AvatarUpdatedAt);
        }

        static string GetAvatar(GroupModel src) {
            return String.IsNullOrWhiteSpace(src.AvatarUrl) || src.AvatarUrl.EndsWith("/")
                ? null
                : AddDefaultProtocol(src.AvatarUrl) + GetQs(src.AvatarUpdatedAt);
        }

        static string GetBackgroundUrl(GroupModel src) {
            return String.IsNullOrWhiteSpace(src.BackgroundUrl) || src.BackgroundUrl.EndsWith("/")
                ? null
                : AddDefaultProtocol(src.BackgroundUrl) + GetQs(src.BackgroundUpdatedAt);
        }

        static string GetAvatarUrl(string avatarUrl, DateTime? updatedAt) {
            return String.IsNullOrWhiteSpace(avatarUrl)
                ? null
                : (AddDefaultProtocol(avatarUrl) + "72x72.jpg" + GetQs(updatedAt));
        }

        static string GetQs(DateTime? updatedAt) {
            return "?v=" + updatedAt.GetValueOrDefault().GetStamp();
        }

        static string AddDefaultProtocol(string avatarUrl) {
            return "http:" + avatarUrl;
        }

        static string GetGravatar(string md5) {
            return "http://www.gravatar.com/avatar/" + md5 + "?size=72&d=" + defaultAvaImg;
        }
    }
}