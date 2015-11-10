// <copyright company="SIX Networks GmbH" file="PackageList.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Reactive.Linq;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public class PackageList : SelectionList<PackageItem>
    {
        readonly RepositoryHandler _handler;

        public PackageList(RepositoryHandler handler) {
            _handler = handler;
            Items.ChangeTrackingEnabled = true;
            Items.ItemChanged
                .Where(x => x.PropertyName == "ActualDependency")
                .Subscribe(
                    x => Common.App.PublishEvent(new CurrentPackageChanged(x.Sender.ActualDependency)));
        }

        public void ProcessPackages() {
            var repository = _handler.Repository;
            var pm = _handler.PackageManager;
            Items.Clear();
            if (repository == null)
                return;

            var dic = pm.GetPackagesAsVersions(_handler.Remote);
            Items.AddRange(dic.Select(
                x =>
                    new PackageItem(x.Key, _handler, x.Value)).ToArray());
        }

        public class CurrentPackageChanged
        {
            public CurrentPackageChanged(SpecificVersion dependency) {
                Current = dependency;
            }

            public SpecificVersion Current { get; private set; }
        }
    }
}