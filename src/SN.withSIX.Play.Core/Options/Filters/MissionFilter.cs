// <copyright company="SIX Networks GmbH" file="MissionFilter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SmartAssembly.ReportUsage;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Missions;

namespace SN.withSIX.Play.Core.Options.Filters
{
    [DataContract(Name = "MissionFilter",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models.Filters")]
    public class MissionFilter : FilterBase<IContent>
    {
        [DataMember] string _Author;
        [DataMember] string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (SetProperty(ref _Name, value))
                    PublishFilter();
            }
        }
        public string Author
        {
            get { return _Author; }
            set
            {
                if (SetProperty(ref _Author, value))
                    PublishFilter();
            }
        }

        public override void DefaultFilters() {
            _supressPublish = true;

            Name = null;
            Author = null;

            _supressPublish = false;

            base.DefaultFilters();
        }

        [ReportUsage("MissionFilter: PublishFilter")]
        public override void PublishFilter() {
            base.PublishFilter();
        }

        public override bool Handler(IContent content) {
            var mission = content as Mission;
            if (mission == null)
                return false;

            if (!String.IsNullOrWhiteSpace(Name)) {
                if (!mission.FileName.NullSafeContainsIgnoreCase(Name)
                    && !mission.Name.NullSafeContainsIgnoreCase(Name)
                    && !mission.FullName.NullSafeContainsIgnoreCase(Name))
                    return false;
            }

            if (!String.IsNullOrWhiteSpace(Author)) {
                if (!mission.Author.NullSafeContainsIgnoreCase(Name))
                    return false;
            }

            return true;
        }

        public override bool AnyFilterEnabled() {
            if (!String.IsNullOrWhiteSpace(Name))
                return true;
            return false;
        }
    }
}