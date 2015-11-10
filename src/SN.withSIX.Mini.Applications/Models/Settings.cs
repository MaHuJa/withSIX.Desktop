// <copyright company="SIX Networks GmbH" file="Settings.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using SN.withSIX.Api.Models.Content;
using SN.withSIX.Api.Models.Premium;
using SN.withSIX.Core;

namespace SN.withSIX.Mini.Applications.Models
{
    public class Settings
    {
        public RoamingSettings Roaming { get; set; } = new RoamingSettings();
        public SecureSettings Secure { get; set; } = new SecureSettings();
        public LocalSettings Local { get; set; } = new LocalSettings();
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

    public class LoginInfo
    {
        public static readonly Uri DefaultAvatarUrl =
            new Uri("http://www.vacul.org/extension/site/design/site/images/anonymous-user.png");
        public static readonly LoginInfo Default = new LoginInfo();

        LoginInfo() : this(new AccountInfo(), new AuthenticationInfo()) {}

        protected LoginInfo(AccountInfo accountInfo, AuthenticationInfo authInfo) {
            Account = accountInfo;
            Authentication = authInfo;
        }

        public bool IsPremium => Authentication.PremiumToken.IsPremium();
        public AccountInfo Account { get; set; }
        public AuthenticationInfo Authentication { get; set; }
        public virtual bool IsLoggedIn { get; set; } = false;
    }

    // TODO: This is not properly serialized as LoggedInInfo, thats why the Overlay doesnt work properly
    // thats why we set the IsLoggedIN and have a setter :(
    public class LoggedInInfo : LoginInfo
    {
        public LoggedInInfo(AccountInfo accountInfo, AuthenticationInfo authInfo) : base(accountInfo, authInfo) {}
        public override bool IsLoggedIn { get; set; } = true;
    }

    public class SecureSettings
    {
        public LoginInfo Login { get; set; }
        public Guid ClientId { get; set; } = Guid.NewGuid();
    }

    public class RoamingSettings {}

    public class LocalSettings
    {
        public Guid SelectedGameId { get; set; }
        public DateTime LastSync { get; set; }
        public bool OptOutReporting { get; set; }
        public bool ShowDesktopNotifications { get; set; } = true;
        public bool StartWithWindows { get; set; } = true;
        public string CurrentVersion { get; set; }
        public int PlayWithSixImportVersion { get; set; }
        public bool DeclinedPlaywithSixImport { get; set; }
        public ApiHashes ApiHashes { get; set; }
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

    public class AuthenticationInfo
    {
        public PremiumAccessToken PremiumToken { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
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