// <copyright company="SIX Networks GmbH" file="GameFilter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Games.Entities;

namespace SN.withSIX.Play.Core.Options.Filters
{
    [DataContract(Name = "GameFilter",
        Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models.Filters")]
    public class GameFilter : FilterBase<Game>
    {
        [DataMember] bool _isInstalled;
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
        public bool IsInstalled
        {
            get { return _isInstalled; }
            set { SetProperty(ref _isInstalled, value); }
        }

        public override void DefaultFilters() {
            _supressPublish = true;

            Name = null;
            IsInstalled = false;

            _supressPublish = false;

            base.DefaultFilters();
        }

        [ReportUsage("GameFilter: PublishFilter")]
        public override void PublishFilter() {
            base.PublishFilter();
        }

        protected override void ExecutePublish() {
            if (_save)
                DomainEvilGlobal.Settings.RaiseChanged();
            _save = true;

            base.ExecutePublish();
        }

        public override bool Handler(Game game) {
            if (!String.IsNullOrWhiteSpace(Name)) {
                if (!game.MetaData.Name.NullSafeContainsIgnoreCase(Name))
                    return false;
            }

            if (IsInstalled && !game.InstalledState.IsInstalled)
                return false;

            return true;
        }

        public override bool AnyFilterEnabled() {
            if (!String.IsNullOrWhiteSpace(Name))
                return true;
            if (IsInstalled)
                return true;
            return false;
        }
    }
}