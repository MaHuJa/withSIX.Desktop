// <copyright company="SIX Networks GmbH" file="ChangelogParser.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NUnit.Framework;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Play.Core;
using SN.withSIX.Play.Infra.Data.Services;
using SN.withSIX.Play.Tests.Core.Support;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Tests.Core.Integration
{
    [TestFixture, Ignore(""), Category("Integration")]
    public class ChangelogParserTest
    {
        [SetUp]
        public void SetUp() {
            SharedSupport.Init();
            SharedSupport.HandleTools();
        }

        static string GetXml(List<Changelog> parsedOut) {
            return Encoding.Default.GetString(GetXmlMemoryStream(parsedOut).ToArray());
        }

        static MemoryStream GetXmlMemoryStream(List<Changelog> parsedOut) {
            var writer = new XmlSerializer(typeof (List<Changelog>));
            var ms = new MemoryStream();
            writer.Serialize(ms, parsedOut);
            return ms;
        }

        static List<Changelog> GetParsedOut() {
            var parser = new ChangelogParser(new Uri("http://buntu:3000/changelog"));
            var parsedOut = parser.Parse();
            return parsedOut;
        }

        [Test]
        public void ParseToJson() {
            Console.WriteLine(GetParsedOut().ToJson(true));
        }

        [Test]
        public void ParseToXml() {
            Console.WriteLine(GetXml(GetParsedOut()));
        }
    }

    public class ChangelogParser
    {
        static readonly char[] trimChars = {'\n', ' '};
        static readonly Regex rxChangelogVersion = new Regex(@"^([\d\.]+)( ([\w\-]+))?( \((.*)\))?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex rxChangelogEntry = new Regex(@"^(\w+): (.+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex rxChangelogEntry2 = new Regex(@"^\[(\w+)\] (.+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        readonly Uri _url;

        public ChangelogParser(Uri url) {
            _url = url;
        }

        public List<Changelog> Parse() {
            var document = new HtmlDocument();
            document.LoadHtml(FetchString(_url));

            return document.DocumentNode
                .SelectNodes("//div[@class='changelog_app span5']")
                .Select(CreateChangelog).ToList();
        }

        string FetchString(Uri uri) {
            using (var wcLifeTime = new WebClient()) {
                new AuthProvider(new AuthProviderSettingsStorage(DomainEvilGlobal.Settings)).HandleAuthInfo(uri, wcLifeTime);
                return wcLifeTime.DownloadString(uri);
            }
        }

        static Changelog CreateChangelog(HtmlNode node) {
            var changelog = new Changelog();
            ChangelogVersion version = null;
            foreach (var child in node.ChildNodes) {
                HandleTitle(child, changelog);
                version = HandleChangelogVersionWithEntries(child, version, changelog);
                version = HandleOlderSection(child, version, changelog);
            }
            return changelog;
        }

        static ChangelogVersion HandleOlderSection(HtmlNode child, ChangelogVersion version, Changelog changelog) {
            if (child.Name == "div" && child.Attributes["class"].Value.Contains("older")) {
                version = null;
                foreach (var n2 in child.ChildNodes)
                    version = HandleChangelogVersionWithEntries(n2, version, changelog);
                version = null;
            }
            return version;
        }

        static void HandleTitle(HtmlNode child, Changelog changelog) {
            if (child.Name == "h2")
                changelog.Title = child.InnerHtml;
        }

        static ChangelogVersion HandleChangelogVersionWithEntries(HtmlNode child, ChangelogVersion version,
            Changelog changelog) {
            version = HandleChangelogVersion(child, changelog, version);
            version = HandleChangelogEntries(child, version);
            return version;
        }

        static ChangelogVersion HandleChangelogVersion(HtmlNode child, Changelog changelog, ChangelogVersion version) {
            if (child.Name == "h3") {
                version = CreateChangelogVersion(changelog, child);
                changelog.Versions.Add(version);
            }
            return version;
        }

        static ChangelogVersion HandleChangelogEntries(HtmlNode child, ChangelogVersion version) {
            if (child.Name == "ul") {
                foreach (var n in child.ChildNodes.Where(x => x.Name == "li"))
                    version.Entries.Add(CreateChangelogEntry(version, n));

                version = null;
            }
            return version;
        }

        static ChangelogVersion CreateChangelogVersion(Changelog changelog, HtmlNode child) {
            string description = null;
            string title = null;
            string type = null;
            string version = null;
            DateTime dateTime;

            var data = Clean(child.InnerHtml);
            if (rxChangelogVersion.IsMatch(data)) {
                var match = rxChangelogVersion.Match(data);
                version = Version.Parse(match.Groups[1].Value).ToString();
                type = match.Groups[3].Value;
                dateTime = DateTime.Parse(match.Groups[5].Value);
            } else
                dateTime = DateTime.Parse(data);

            return new ChangelogVersion {
                Description = description,
                Title = title,
                Type = type,
                Changelog = changelog,
                Version = version,
                Released = dateTime
            };
        }

        static string Clean(string innerHtml) {
            return Trim(String.Join(" ", innerHtml.Split('\n')
                .Select(Trim)));
        }

        static string Trim(string x) {
            return x.TrimStart(trimChars).TrimEnd(trimChars);
        }

        static ChangelogEntry CreateChangelogEntry(ChangelogVersion version, HtmlNode n) {
            string type = null;
            string title = null;
            var data = Clean(n.InnerHtml);
            var match = rxChangelogEntry.Match(data);
            if (match.Success) {
                type = match.Groups[1].Value;
                title = match.Groups[2].Value;
                // description = title.split index1 to end?
            } else {
                match = rxChangelogEntry2.Match(data);
                if (match.Success) {
                    type = match.Groups[1].Value;
                    title = match.Groups[2].Value;
                } else
                    title = data;
            }
            return new ChangelogEntry {Title = title, Type = TryParseChangeType(type), Version = version};
        }

        static ChangeType TryParseChangeType(string type) {
            ChangeType t;
            Enum.TryParse(MapOldTypeToNewType(type), out t);
            return t;
        }

        static string MapOldTypeToNewType(string type) {
            if (type == null)
                return null;

            if (type.Equals("feature", StringComparison.InvariantCultureIgnoreCase))
                type = "Added";
            if (type.Equals("fix", StringComparison.InvariantCultureIgnoreCase))
                type = "Fixed";
            return type;
        }
    }

    public enum ChangeType
    {
        Other,
        Added,
        Removed,
        Fixed
    }

    public class Changelog
    {
        public Changelog() {
            Versions = new List<ChangelogVersion>();
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public List<ChangelogVersion> Versions { get; set; }
    }

    public class ChangelogEntry
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public ChangeType Type { get; set; }

        [JsonIgnore, XmlIgnore]
        public ChangelogVersion Version { get; set; }
    }

    public class ChangelogVersion
    {
        public ChangelogVersion() {
            Entries = new List<ChangelogEntry>();
        }

        public string Version { get; set; }
        public string Type { get; set; }
        public DateTime Released { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<ChangelogEntry> Entries { get; set; }

        [JsonIgnore, XmlIgnore]
        public Changelog Changelog { get; set; }
    }
}