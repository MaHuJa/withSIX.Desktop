// <copyright company="SIX Networks GmbH" file="IChatHub.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading.Tasks;
using SignalRNetClientProxyMapper;
using SmartAssembly.Attributes;
using SN.withSIX.Api.Models;
using SN.withSIX.Api.Models.Chat;
using SN.withSIX.Api.Models.Social;

namespace SN.withSIX.Play.Infra.Api.Hubs
{
    [DoNotObfuscateType]
    interface IChatHub : IClientHubProxyBase
    {
        Task<ChatMessageModel> SendGroupChatMessage(Guid chatObjectId, string body);
        Task<ChatMessageModel> SendChatMessage(Guid chatObjectId, string body);
        Task<PrivateMessageModel> SendPrivateChatMessage(Guid chatGuid, string body);
        Task MarkChatAsRead(Guid chatGuid);
        Task MarkPrivateChatAsRead(Guid chatGuid);
        Task<ChatHubModel> GetChat(Guid chatGuid);
        Task<PageModel<ChatMessageModel>> GetChatMessages(Guid chatGuid, int page);
        Task<PageModel<PrivateMessageModel>> GetPrivateChatMessages(Guid chatGuid, int page);

        [HubMethodName("SendPrivateMessage")]
        IDisposable PrivateMessageRecieved(Action<PrivateMessageReceived> action);

        [HubMethodName("SendChatMessage")]
        IDisposable ChatMessageReceived(Action<ChatMessageReceived> action);
    }
}