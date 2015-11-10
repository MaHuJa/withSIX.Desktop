// <copyright company="SIX Networks GmbH" file="Account.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Core.Connect
{
    public class Account : ConnectModelBase, IEntity
    {
        Uri _avatar;
        string _displayName;
        string _slug;
        public Account(Guid id) : base(id) {}
        public Uri Avatar
        {
            get { return _avatar; }
            set { SetProperty(ref _avatar, value); }
        }
        public string Slug
        {
            get { return _slug; }
            set { SetProperty(ref _slug, value); }
        }
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        public Uri GetUri() {
            return Tools.Transfer.JoinUri(CommonUrls.ConnectUrl, "profile", Slug);
        }

        public Uri GetOnlineConversationUrl() {
            return Tools.Transfer.JoinUri(GetUri(), "messages");
        }
    }
}