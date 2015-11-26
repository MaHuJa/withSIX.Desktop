// <copyright company="SIX Networks GmbH" file="SecretData.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SN.withSIX.Api.Models.Premium;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Core.Options
{
    public class SecretData
    {
        public AuthenticationData Authentication { get; set; }
        public UserInfo UserInfo { get; set; }
        public Func<Task> Save { get; set; }
    }

    public class AuthenticationData
    {
        public AuthenticationData() {
            AuthCache = new ConcurrentDictionary<string, AuthInfo>();
        }

        public ConcurrentDictionary<string, AuthInfo> AuthCache { get; set; }
    }

    public class PremiumAccessToken : PremiumAccessTokenV1, IEquatable<PremiumAccessToken>
    {
        public PremiumAccessToken(string accessToken, string premiumKey) : base(accessToken, premiumKey) {}

        public bool Equals(PremiumAccessToken other) {
            if (ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;
            return other.AccessToken == AccessToken && other.PremiumKey == PremiumKey;
        }

        public override int GetHashCode() {
            return (AccessToken == null ? 0 : AccessToken.GetHashCode()) ^
                   (PremiumKey == null ? 0 : PremiumKey.GetHashCode());
        }

        public override bool Equals(object obj) {
            return Equals(obj as PremiumAccessToken);
        }
    }

    public class UserInfo
    {
        public Guid ClientId { get; set; }
        public PremiumAccessToken Token { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public AccountInfo Account { get; set; }
    }

    public class AccountInfo
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string AvatarURL { get; set; }
        public bool HasAvatar { get; set; }
        public string EmailMd5 { get; set; }
        public List<string> Roles { get; set; }
        public long? AvatarUpdatedAt { get; set; }
    }
}