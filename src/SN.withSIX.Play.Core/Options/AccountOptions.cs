// <copyright company="SIX Networks GmbH" file="AccountOptions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Api.Models.Premium;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Connect.Events;
using SN.withSIX.Play.Core.Options.Filters;

namespace SN.withSIX.Play.Core.Options
{
    [DataContract(Name = "AccountOptions", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class AccountOptions : OptionBase
    {
        [DataMember] Guid _accountId;
        [DataMember] ContactFilter _Filter = new ContactFilter();
        public Guid AccountId
        {
            get { return _accountId; }
            set { _accountId = value; }
        }
        public UserInfo UserInfo
        {
            get { return DomainEvilGlobal.SecretData.UserInfo; }
        }
        public string AccessToken
        {
            get { return UserInfo.AccessToken; }
            set
            {
                if (UserInfo.AccessToken == value)
                    return;
                UserInfo.AccessToken = value;
                if (value == null)
                    UserInfo.RefreshToken = null;
                LegacyApiKey = null;
                Common.App.PublishEvent(new ApiKeyUpdated(value));
            }
        }
        [Obsolete("Replaced by AccessToken")]
        public string LegacyApiKey
        {
            get { return UserInfo.ApiKey; }
            set { UserInfo.ApiKey = value; }
        }
        public ContactFilter Filter
        {
            get { return _Filter; }
            set { _Filter = value; }
        }
        // For display to the user only..
        public bool IsPremium
        {
            get { return UserInfo.Token.IsPremium(); }
            set { OnPropertyChanged(); } // Tsk
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            if (_Filter == null)
                _Filter = new ContactFilter();
        }

        public void SetP(bool val) {
            IsPremium = val; // for display to user
        }
    }

    public static class PremiumTokenExtensions
    {
        public static bool IsPremium(this PremiumAccessTokenV1 token) {
            return token != null &&
                   token.PremiumUntil > Tools.Generic.GetCurrentUtcDateTime;
        }

        public static bool IsValidInNearFuture(this PremiumAccessTokenV1 token) {
            return token != null && token.ValidUntil > Tools.Generic.GetCurrentUtcDateTime.AddHours(6);
        }
    }
}