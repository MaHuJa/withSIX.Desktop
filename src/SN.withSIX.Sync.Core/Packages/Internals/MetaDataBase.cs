// <copyright company="SIX Networks GmbH" file="MetaDataBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Sync.Core.Packages.Internals
{
    // TODO: Should not allow Version or Branch to be changed after construct?
    public abstract class MetaDataBase : IComparePK<MetaDataBase>, IComparePK<Dependency>
    {
        public const string TodoWriteYourName = "TODO: Write your name";
        public const string TodoFullName = "TODO: Write a full name";
        public const string TodoDesc = "TODO: Write a description";
        public const string TodoSummary = "TODO: Write a summary";
        public static readonly Dictionary<string, string> TodoAuthors = new Dictionary<string, string> {
            {TodoWriteYourName, "TODO: Write your email address"}
        };
        string _fullName;

        protected MetaDataBase(SpecificVersion info) {
            Name = info.Name;
            Branch = info.Branch;
            Version = info.Version;

            Dependencies = new Dictionary<string, string>();

            Authors = TodoAuthors.ToDictionary(x => x.Key, y => y.Key);
            Homepage = String.Empty;
            FullName = TodoFullName;

            Additional = new Dictionary<string, string>();

            Description = TodoDesc;
            Summary = TodoSummary;

            Tags = new List<string>();

            Date = Tools.Generic.GetCurrentDateTime;
        }

        protected MetaDataBase(string fqn) : this(new SpecificVersion(fqn)) {}
        public Dictionary<string, string> Additional { get; set; }
        public Dictionary<string, string> Dependencies { get; set; }
        public string Name { get; set; }
        public string Branch { get; set; }
        public Version Version { get; set; }
        public string Summary { get; set; }
        public Dictionary<string, string> Authors { get; set; }
        public List<string> Tags { get; set; }
        public DateTime Date { get; set; }
        public string Homepage { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }

        public bool ComparePK(Dependency other) {
            return other != null && other.GetFullName().Equals(GetFullName());
        }

        public virtual bool ComparePK(object other) {
            var o = other as Dependency;
            if (o == null)
                return false;
            return ComparePK((Dependency) other);
        }

        public bool ComparePK(MetaDataBase other) {
            return other != null && other.GetFullName().Equals(GetFullName());
        }

        public SpecificVersion ToSpecificVersion() {
            return new SpecificVersion(GetFullName());
        }

        public string GetFullName() {
            return _fullName ?? (_fullName = GetFullNameInternal());
        }

        string GetFullNameInternal() {
            var name = Name;
            var versionInfo = GetVersionInfo();
            if (versionInfo != null)
                name += "-" + versionInfo;
            return name;
        }

        public string GetVersionInfo() {
            var name = string.Empty;
            if (Version != null)
                name += Version;
            if (!String.IsNullOrWhiteSpace(Branch)) {
                var b = Branch.ToLower();
                if (b != "stable")
                    name += "-" + b;
            }
            return name;
        }
    }
}