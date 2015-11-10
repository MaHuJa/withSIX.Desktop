// <copyright company="SIX Networks GmbH" file="ContentController.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Reactive.Linq;
using ReactiveUI;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Games.Legacy.Mods
{
    public abstract class ContentController : PropertyChangedBase, IHaveModel<IContent>
    {
        string _desiredRevision;
        bool _isInstalled;
        string _latestRevision;
        bool _newerVersionAvailable;
        PackageItem _package;
        string _revision;

        protected ContentController(IContent content) {
            Model = content;
            this.WhenAnyValue(x => x.Model.State)
                .Select(x => x != ContentState.NotInstalled)
                .BindTo(this, x => x.IsInstalled);
        }

        public PackageItem Package
        {
            get { return _package; }
            set { SetProperty(ref _package, value); }
        }
        public string Revision
        {
            get { return _revision; }
            protected set { SetProperty(ref _revision, value); }
        }
        public string DesiredRevision
        {
            get { return _desiredRevision; }
            protected set { SetProperty(ref _desiredRevision, value); }
        }
        public bool NewerVersionAvailable
        {
            get { return _newerVersionAvailable; }
            protected set { SetProperty(ref _newerVersionAvailable, value); }
        }
        public string LatestRevision
        {
            get { return _latestRevision; }
            protected set { SetProperty(ref _latestRevision, value); }
        }
        public virtual bool IsInstalled
        {
            get { return _isInstalled; }
            protected set { SetProperty(ref _isInstalled, value); }
        }
        public IContent Model { get; }
        public abstract UpdateState CreateUpdateState();
    }
}