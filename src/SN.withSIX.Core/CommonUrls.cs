// <copyright company="SIX Networks GmbH" file="CommonUrls.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace SN.withSIX.Core
{
    public static class Bla
    {
        public static CommonUrls.HostType GetHostType() {
            if (Common.Flags.Staging) {
#if DEBUG
                return CommonUrls.HostType.Local2;
#else
                return CommonUrls.HostType.Staging;
#endif
            }

            return CommonUrls.HostType.Production;
        }

        /// <summary>
        ///     Together with the AcceptAllCertifications method right
        ///     below this causes to bypass errors caused by SLL-Errors.
        /// </summary>
        public static void IgnoreBadCertificates() {
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
        }

        /// <summary>
        ///     In Short: the Method solves the Problem of broken Certificates.
        ///     Sometime when requesting Data and the sending Webserverconnection
        ///     is based on a SSL Connection, an Error is caused by Servers whoes
        ///     Certificate(s) have Errors. Like when the Cert is out of date
        ///     and much more... So at this point when calling the method,
        ///     this behaviour is prevented
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certification"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns>true</returns>
        static bool AcceptAllCertifications(object sender, X509Certificate certification, X509Chain chain,
            SslPolicyErrors sslPolicyErrors) {
            return true;
        }
    }

    public static class CommonUrls
    {
        public enum HostType
        {
            Production,
            Staging,
            Local2,
            Local
        }

        public static readonly Uri FlashUri = new Uri(
            "http://labsdownload.adobe.com/pub/labs/flashruntimes/flashplayer/flashplayer19_install_win_ppapi.exe");
        public static readonly string TwitterUrl = @"https://twitter.com/SixNetworks";
        public static readonly string FacebookUrl = @"https://www.facebook.com/withSIX";
        public static readonly string GplusUrl = @"https://plus.google.com/104785560298357015306";
        public static readonly Uri ApiCdnUrl = new Uri("http://" + Buckets.WithSixApi.CdnHostname);
        public static readonly Uri CdnUrl = new Uri("http://cdn.withsix.com");
        public static readonly Uri ImageCdn = new Uri("https://img-cdn.withsix.com");
        public static readonly Uri UsercontentCdn = new Uri("https://" + Buckets.WithSixUsercontent.CdnHostname);
        public static readonly Uri UsercontentCdnProduction =
            new Uri("https://" + Buckets.WthSiUsercontentProduction.CdnHostname);
        public static readonly Uri ConnectUrlHttp = new Uri(UrlBuilder.GetUri("http"), "me/");
        public static readonly Uri PlayUrl = new Uri(UrlBuilder.GetUri("http"), "p/");
        public static readonly Uri PublishApiUrl = new Uri(UrlBuilder.GetUrl("https", "publish-api"));
        public static readonly string ContentApiVersion = "2";
        static readonly string ApiHost = "api.withsix.com";
        public static readonly Uri ContentApiUrl = new Uri(@"https://" + ApiHost);
        public static readonly Uri ContentUrl = new Uri(@"http://" + ApiHost);
        public static readonly Uri CdnUrl2 = new Uri("http://cdn2.withsix.com");
        public static readonly string RemoteSoftwarePath = "software/withSIX";
        public static readonly Uri ConnectUrl = new Uri(UrlBuilder.GetUri("https"), "me/");
        public static readonly Uri MainUrl = new Uri(UrlBuilder.GetUrl("http", ""));
        public static readonly Uri SocialApiUrl =
            new Uri(UrlBuilder.GetUrl("https", UrlBuilder.Sites.Auth));
        public static readonly Uri LoginUrl = ConnectUrl;
        public static readonly Uri RegisterUrl = new Uri(MainUrl, @"register");
        public static readonly Uri AccountSettingsUrl = new Uri(ConnectUrl, @"settings");
        public static readonly Uri ClientUrl = new Uri(SocialApiUrl, @"client");
        public static readonly Uri SuUrl = MainUrl;
        public static readonly Uri BlogUrl = new Uri(MainUrl, "blog");
        public static readonly Uri SuAboutUrl = SuUrl;
        public static readonly Uri SuSupportUrl = new Uri(SuUrl, "support");
        public static readonly Uri SuclUrl = new Uri(SuUrl, "changelog");
        public static readonly Uri SoftwareUpdateUri = new Uri(CdnUrl2, RemoteSoftwarePath);
        public static readonly Uri Ws1Url = new Uri(UrlBuilder.GetUrl("https", "ws1"));
        public static readonly Uri SignalrApi = Environments.Environment == Environments.Production
            ? Ws1Url
            : SocialApiUrl;

        public static bool IsWithSixUrl(string url) {
            return IsWithSixUrl(new Uri(url));
        }

        public static bool IsWithSixUrl(Uri targetURL) {
            var host = targetURL.Host.ToLower();

            return host.EndsWith("withsix.com") || host.EndsWith("withsix.net")
#if DEBUG
                   || host.EndsWith("localhost")
#endif
                ;
        }

        public static class AuthorizationEndpoints
        {
            //public const string PwSClientName = "playwithsix";
            public const string SyncClientName = "mini";
            static readonly Uri baseAddress = UrlBuilder.GetUri("https", "auth");
            public static readonly Uri AuthorizeEndpoint = new Uri(baseAddress, "/identity/connect/authorize");
            public static readonly Uri LogoutEndpoint = new Uri(baseAddress, "/identity/connect/endsession");
            public static readonly Uri TokenEndpoint = new Uri(baseAddress, "/identity/connect/token");
            public static readonly Uri UserInfoEndpoint = new Uri(baseAddress, "/identity/connect/userinfo");
            public static readonly Uri IdentityTokenValidationEndpoint = new Uri(baseAddress,
                "/identity/connect/identitytokenvalidation");
            public static readonly Uri TokenRevocationEndpoint = new Uri(baseAddress, "/identity/connect/revocation");
            public static readonly Uri LocalCallback = new Uri("https://localhost/formsclient");
            public static readonly Uri LocalCallbackMini = new Uri("oob://localhost/wpfclient");
        }

        public static class UrlBuilder
        {
            public static readonly IDictionary<string, int> SiteHttpPortsLocalhost = new Dictionary<string, int> {
                {Sites.Auth, 80},
                {Sites.Admin, 9000},
                {Sites.Main, 9000},
                {Sites.Connect, 9000},
                {Sites.Play, 9000},
                {Sites.Develop, 9000},
                {"publish-api", 80},
                {"ws1", 80},
                {Sites.Api, 80},
                {Sites.Api2, 80}
            };
            public static readonly IDictionary<string, int> SiteHttpsPortsLocalhost = new Dictionary<string, int> {
                {Sites.Auth, 443},
                {Sites.Admin, 9001},
                {Sites.Main, 9001},
                {Sites.Connect, 9001},
                {Sites.Play, 9001},
                {Sites.Develop, 9001},
                {"publish-api", 443},
                {"ws1", 443},
                {Sites.Api, 443},
                {Sites.Api2, 443}
            };
            static readonly IDictionary<string, ConcurrentDictionary<string, Uri>> cache =
                new Dictionary<string, ConcurrentDictionary<string, Uri>> {
                    {"http", new ConcurrentDictionary<string, Uri>()},
                    {"https", new ConcurrentDictionary<string, Uri>()}
                };

            public static string GetUrl(string scheme, string site = "") {
                return GetUri(scheme, site).ToString().TrimEnd('/');
            }

            public static Uri GetLoginUri() {
                return new Uri(GetUri("https", "connect"), "/login");
            }

            public static string GetLoginUrl() {
                return GetLoginUri().ToString().TrimEnd('/');
            }

            public static Uri GetUri(string scheme, string site = "") {
                return cache[scheme.ToLower()].GetOrAdd(site, key => GetUriInternal(scheme, site));
            }

            static Uri GetUriInternal(string scheme, string site) {
                const string localhost = "localhost";
                return BuildUri(Environments.Host == localhost ? localhost : GetHostname(Environments.Host, site),
                    scheme,
                    GetPort(scheme, site));
            }

            static int GetPort(string scheme, string site = "") {
                switch (Environments.Environment) {
                case "staging":
                    return GetStagingPort(scheme, site);
                case "production":
                    return GetProductionPort(scheme, site);
                case "local2":
                    return GetLocalhostPort(scheme, site);
                default: {
                    return GetLocalhostPort(scheme, site);
                }
                }
            }

            static int GetLocalhostPort(string scheme, string site = "") {
                if (site == "www")
                    site = "";
                return scheme == "http"
                    ? SiteHttpPortsLocalhost[site]
                    : SiteHttpsPortsLocalhost[site];
            }

            static int GetStagingPort(string scheme, string site = "") {
                if (site == "www")
                    site = "";
                return scheme == "http"
                    ? 80
                    : 443;
            }

            static int GetProductionPort(string scheme, string site = "") {
                if (site == "www")
                    site = "";
                return scheme == "http"
                    ? 80
                    : 443;
            }

            static string GetHostname(string hostName, string site = "") {
                var sub = site + (site.Length > 0 ? "." : "");
                return sub + hostName;
            }

            static Uri BuildUri(string host, string scheme, int? portNumber = null) {
                return portNumber.HasValue
                    ? new UriBuilder(scheme, host, portNumber.Value).Uri
                    : new UriBuilder(scheme, host).Uri;
            }

            public static class Sites
            {
                public const string Connect = "connect";
                public const string Main = ""; // "www"
                public const string Play = "play";
                public const string Auth = "auth";
                public const string Api = "api";
                public const string Api2 = "api2";
                public const string Develop = "develop";
                public const string Admin = "admin";
            }
        }
    }

    public static class Buckets
    {
        public static Bucket WithSixApi { get; } = new AmazonBucket("withsix-api", Environments.Environment);
        public static Bucket WithSixUsercontent { get; } = new AmazonBucket("withsix-usercontent",
            Environments.Environment);
        public static Bucket WthSiUsercontentProduction { get; } = new AmazonBucket("withsix-usercontent",
            Environments.Production);
        public static Bucket[] All { get; } = {WithSixApi, WithSixUsercontent};
    }

    public class AmazonBucket : Bucket
    {
        internal AmazonBucket(string value, string environment) : base(value, "s3-eu-west-1.amazonaws.com", environment) {}
    }

    public class Bucket
    {
        internal Bucket(string value, string host, string environment) {
            Name = value;
            switch (environment) {
            case Environments.Local:
                value = value + "-dev";
                break;
            case Environments.Local2:
                value = value + "-dev";
                break;
            case Environments.Staging:
                value = value + "-staging";
                break;
            }

            Value = value;
            Hostname = value + "." + host;
            CdnHostname = environment == Environments.Production
                ? value + "-cdn.withsix.com"
                : Hostname;
        }

        public string Hostname { get; }
        public string CdnHostname { get; }
        public string Name { get; private set; }
        string Value { get; }

        public override string ToString() {
            return this;
        }

        public static implicit operator string(Bucket v) {
            return v.Value;
        }
    }

    public static class Environments
    {
        public const string Production = "production";
        public const string Staging = "staging";
        public const string Local = "local";
        public const string Local2 = "local2";
        public static readonly string Environment;
        static readonly Dictionary<string, string> usernameHostMapping = new Dictionary<string, string> {
            {@"SN\Patrick.Roza", "p"},
            {@"SN\Oliver.Baker", "o"}
        };
        static readonly string[] productionHosts = {
            "withsix.com", "www.withsix.com", "connect.withsix.com", "play.withsix.com", "admin.withsix.com",
            "auth.withsix.com"
        };
        static readonly string[] stagingHosts = {
            "staging.withsix.net", "www.staging.withsix.net", "connect.staging.withsix.net", "play.staging.withsix.net",
            "admin.staging.withsix.net", "auth.staging.withsix.net"
        };
        static readonly string[] localCloudHosts;
        public static readonly string[] Origins;
        public static readonly string RootPath;
        static readonly string[] localHosts = {
            "local.withsix.net", "www.local.withsix.net", "connect.local.withsix.net", "play.local.withsix.net",
            "admin.local.withsix.net", "auth.local.withsix.net"
        };
        static readonly string[] environments = {Production, Staging, Local, Local2};

        static Environments() {
            Environment = Environment = Bla.GetHostType().ToString().ToLower();
            if (!environments.Contains(Environment))
                throw new NotSupportedException("Unsupported environment: " + Environment);
            IsLocal = Environment == Local || Environment == Local2;
            localCloudHosts = GetLocalHosts();
            Host = GetHost();
            Origins = GetOrigins(Production).Concat(GetOrigins(Staging)).Concat(GetOrigins(Local)).ToArray();
            CdnUrl = GetCdnUrl();
            if (Common.Flags.Staging)
                Bla.IgnoreBadCertificates();
        }

        public static string Host { get; }
        public static bool IsLocal { get; set; }
        public static string CdnUrl { get; private set; }

        static string GetHost() {
            switch (Environment) {
            case "staging":
                return "staging.withsix.net";
            case "production":
                return "withsix.com";
            case "local2":
                return GetLocalHost();
            default: {
                return "localhost";
            }
            }
        }

        static string GetLocalHost() {
            const string host = "local.withsix.net";
            var userName = WindowsIdentity.GetCurrent().Name;
            return usernameHostMapping.ContainsKey(userName)
                ? JoinMapping(usernameHostMapping[userName], host)
                : host;
        }

        static string[] GetLocalHosts() {
            var userName = WindowsIdentity.GetCurrent().Name;
            return usernameHostMapping.ContainsKey(userName)
                ? localHosts.Select(x => JoinMapping(usernameHostMapping[userName], x)).ToArray()
                : localHosts;
        }

        static string JoinMapping(string mapping, string hostname) {
            var split = hostname.Split('.').ToList();
            split.Insert(split.Count == 3 ? 0 : 1, mapping);
            return string.Join(".", split);
        }

        static string GetCdnUrl() {
            switch (Environment) {
            case Local:
                // TODO: LocalHost + ports.. and use http?
                return ("//" + GetLocalHost() + "/cdn");
            case Local2:
                return ("//" + GetLocalHost() + "local.withsix.net/cdn");
            case Production:
                return "//az667488.vo.msecnd.net";
            case Staging:
                return "//az668256.vo.msecnd.net";
            default: {
                throw new NotSupportedException();
            }
            }
        }

        static IEnumerable<string> GetOrigins(string env) {
            switch (env) {
            case Local:
                return Enumerable.Repeat("http://localhost:9000", 1)
                    .Concat(
                        localCloudHosts.Select(x => "http://" + x + ":9000")
                            .Concat(localCloudHosts.Select(x => "https://" + x + ":9001")));
            case Local2:
                return localCloudHosts.Select(x => "http://" + x).Concat(localCloudHosts.Select(x => "https://" + x))
                    .Concat(
                        localCloudHosts.Select(x => "http://" + x + ":9000")
                            .Concat(localCloudHosts.Select(x => "https://" + x + ":9001")))
                    .Concat(Enumerable.Repeat("http://localhost:9000", 1));
            case Staging:
                return stagingHosts.Select(x => "http://" + x).Concat(stagingHosts.Select(x => "https://" + x));
            case Production:
                return productionHosts.Select(x => "http://" + x).Concat(productionHosts.Select(x => "https://" + x));
            default: {
                throw new NotSupportedException("Unsupported Origins environment: " + Environment);
            }
            }
        }
    }
}