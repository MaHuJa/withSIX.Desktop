// <copyright company="SIX Networks GmbH" file="YomaConfig.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using NDepend.Path;
using SN.withSIX.Core;

namespace SN.withSIX.Sync.Core.Legacy.Yoma
{
    public class YomaConfig
    {
        const string Namespace = "http://tempuri.org/DSServer.xsd";

        public YomaConfig() {
            Addons = new YomaAddon[] {};
            Mods = new YomaMod[] {};
        }

        public YomaConfig(XDocument inputAddons, XDocument inputMods, XDocument inputServer = null) {
            XNamespace ns = Namespace;
            Addons =
                inputAddons.Root.Elements(ns + "Addons")
                    .Select(Tools.Serialization.Xml.Deserialize<YomaAddon>).ToArray();

            Mods =
                inputMods.Root.Elements(ns + "Mods")
                    .Select(Tools.Serialization.Xml.Deserialize<YomaMod>).ToArray();

            if (inputServer != null) {
                var el = inputServer.Root.Element(ns + "Server");
                if (el != null)
                    Server = Tools.Serialization.Xml.Deserialize<YomaServer>(el);
            }
        }

        public YomaConfig(IAbsoluteFilePath inputAddonsFile, IAbsoluteFilePath inputModsFile,
            IAbsoluteFilePath inputServerFile = null)
            : this(
                XDocument.Load(inputAddonsFile.ToString()), XDocument.Load(inputModsFile.ToString()),
                inputServerFile == null ? null : XDocument.Load(inputServerFile.ToString())) {
            Contract.Requires<ArgumentOutOfRangeException>(inputAddonsFile != null);
            Contract.Requires<ArgumentOutOfRangeException>(inputModsFile != null);
        }

        public YomaAddon[] Addons { get; set; }
        public YomaMod[] Mods { get; private set; }
        public YomaServer Server { get; private set; }

        [XmlRoot("Addons", Namespace = Namespace)]
        public class YomaAddon
        {
            string _md5;
            public YomaAddon() {}

            public YomaAddon(string md5, string path, string pbo, long size, string url) {
                Md5 = md5;
                Path = path;
                Pbo = pbo;
                Size = size;
                Url = url;
            }

            public string Md5
            {
                get { return _md5; }
                set { _md5 = value == null ? value : value.ToLower(); }
            }
            public string Path { get; set; }
            public string Pbo { get; set; }
            public long Size { get; set; }
            public string Url { get; set; }
        }

        [XmlRoot("Mods", Namespace = Namespace)]
        public class YomaMod
        {
            public YomaMod() {}

            public YomaMod(string name, int type, string webPage, long sequenceId) {
                Name = name;
                Type = type;
                WebPage = webPage;
                SequenceID = sequenceId;
            }

            public string Name { get; set; }
            public int Type { get; set; }
            public string WebPage { get; set; }
            public long SequenceID { get; set; }
        }

        [XmlRoot("Server", Namespace = Namespace)]
        public class YomaServer
        {
            public string AddonPwd { get; set; }
            public string AddonURL { get; set; }
            public string AddonUserName { get; set; }
            public string AutoRunTS { get; set; }
            public string GameIP { get; set; }
            public int GamePort { get; set; }
            public string GamePwd { get; set; }
            public string HomePage { get; set; }
            public string Name { get; set; }
            public string TSAnonimous { get; set; }
            public string TSChannel { get; set; }
            public string TSChannelPwd { get; set; }
            public string TSIp { get; set; }
            public string TSNickname { get; set; }
            public int TSPort { get; set; }
            public string TSPwd { get; set; }
            public string TSUser { get; set; }
            public string TSVersion { get; set; }
            public string Version { get; set; }
            public string Game { get; set; }
            public string RequiredAddons { get; set; }
            public string OptionalAddons { get; set; }
            public string MessageOfTheDay { get; set; }
        }
    }
}