// <copyright company="SIX Networks GmbH" file="ChatReceivedDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Play.Applications.UseCases;

namespace SN.withSIX.Play.Applications.DataModels.Notifications
{
    public class ChatReceivedDataModel : NotificationBaseDataModel
    {
        public ChatReceivedDataModel(string fromUserName, Uri fromAvatar, string message)
            : base(SixIconFont.withSIX_icon_Chat_Message) {
            FromUserName = fromUserName;
            FromAvatar = fromAvatar;
            Message = message;
            OnClickDispatch = new DispatchCommand<ChatMessageRecievedCommand>(new ChatMessageRecievedCommand());
        }

        public string FromUserName { get; private set; }
        public Uri FromAvatar { get; private set; }
        public string Message { get; private set; }
    }
}