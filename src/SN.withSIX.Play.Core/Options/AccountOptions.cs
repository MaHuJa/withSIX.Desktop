// <copyright company="SIX Networks GmbH" file="AccountOptions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Api.Models.Premium;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Connect.Events;

namespace SN.withSIX.Play.Core.Options
{
    [DataContract(Name = "AccountOptions", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class AccountOptions : OptionBase
    {
        [DataMember] Guid _accountId;
        public Guid AccountId
        {
            get { return _accountId; }
            set { _accountId = value; }
        }
        public UserInfo UserInfo => DomainEvilGlobal.SecretData.UserInfo;
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
        // For display to the user only..
        public bool IsPremium
        {
            get { return UserInfo.Token.IsPremium(); }
            set { OnPropertyChanged(); } // Tsk
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