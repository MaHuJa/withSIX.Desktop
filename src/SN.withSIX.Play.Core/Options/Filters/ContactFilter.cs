// <copyright company="SIX Networks GmbH" file="ContactFilter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using ReactiveUI;
using SmartAssembly.ReportUsage;
using SN.withSIX.Api.Models.Context;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Play.Core.Connect;

namespace SN.withSIX.Play.Core.Options.Filters
{
    [DataContract(Name = "ContactFilter", Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Filters")]
    public class ContactFilter : FilterBase<IContact>
    {
        [DataMember] string _name;
        bool _showOnlyIngame;
        bool _showOnlyOnline;

        public ContactFilter() {
            SetupReactive();
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (SetProperty(ref _name, value))
                    PublishFilter();
            }
        }
        public bool ShowOnlyOnline
        {
            get { return _showOnlyOnline; }
            set { SetProperty(ref _showOnlyOnline, value); }
        }
        public bool ShowOnlyIngame
        {
            get { return _showOnlyIngame; }
            set { SetProperty(ref _showOnlyIngame, value); }
        }

        void SetupReactive() {
            this.WhenAnyValue(x => x.ShowOnlyIngame)
                .Where(x => x)
                .Subscribe(x => ShowOnlyOnline = false);

            this.WhenAnyValue(x => x.ShowOnlyOnline)
                .Where(x => x)
                .Subscribe(x => ShowOnlyIngame = false);
        }

        [ReportUsage("ContactFilter: PublishFilter")]
        public override void PublishFilter() {
            base.PublishFilter();
        }

        public override void DefaultFilters() {
            _supressPublish = true;
            Name = null;
            _supressPublish = false;
            base.DefaultFilters();
        }

        public override bool AnyFilterEnabled() {
            if (!String.IsNullOrWhiteSpace(Name)
                || ShowOnlyIngame || ShowOnlyOnline)
                return true;
            return false;
        }

        protected override void ExecutePublish() {
            if (_save)
                DomainEvilGlobal.Settings.RaiseChanged();
            _save = true;

            base.ExecutePublish();
        }

        public override bool Handler(IContact entity) {
            var friend = entity as Friend;
            if (ShowOnlyIngame) {
                if (friend == null)
                    return false;
                if (friend.PlayingOn == null)
                    return false;
            }

            if (ShowOnlyOnline) {
                if (friend == null)
                    return false;
                if (friend.Status == OnlineStatus.Offline)
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(Name)) {
                if (!entity.DisplayName.NullSafeContainsIgnoreCase(Name))
                    return false;
            }

            return true;
        }

        public void SwitchShowOnlyIngame() {
            ShowOnlyIngame = !ShowOnlyIngame;
            PublishFilter();
        }

        public void SwitchShowOnlyOnline() {
            ShowOnlyOnline = !ShowOnlyOnline;
            PublishFilter();
        }

        public void ShowAll() {
            ShowOnlyIngame = false;
            ShowOnlyOnline = false;
            PublishFilter();
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            SetupReactive();
        }
    }
}