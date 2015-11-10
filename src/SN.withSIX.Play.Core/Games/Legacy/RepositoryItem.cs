// <copyright company="SIX Networks GmbH" file="RepositoryItem.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using NDepend.Path;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Sync.Core.Repositories;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    public class RepositoryItem : PropertyChangedBase, IComparePK<RepositoryItem>
    {
        bool _isPinned;
        bool _isSelected;
        IAbsoluteDirectoryPath _repositoryDirectory;
        IAbsoluteDirectoryPath _workingDirectory;

        public RepositoryItem(string name, string description = null, IAbsoluteDirectoryPath workingDirectory = null,
            IAbsoluteDirectoryPath repoDirectory = null,
            RepositoryHandler handler = null) {
            Name = name;
            Description = description;
            WorkingDirectory = workingDirectory;
            RepositoryDirectory = repoDirectory;
            Handler = handler ?? new RepositoryHandler();
            Bundles = new BundleList(Handler);
            Packages = new PackageList(Handler);

            var command = ReactiveCommand.CreateAsyncTask(x => HandleRepo());
            command.Subscribe();

            this.WhenAnyValue(x => x.WorkingDirectory, x => x.RepositoryDirectory, (w, r) => String.Empty)
                .InvokeCommand(command);
        }

        public BundleList Bundles { get; protected set; }
        public PackageList Packages { get; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public bool IsPinned
        {
            get { return _isPinned; }
            set { SetProperty(ref _isPinned, value); }
        }
        public RepositoryHandler Handler { get; }
        public IAbsoluteDirectoryPath WorkingDirectory
        {
            get { return _workingDirectory; }
            set { SetProperty(ref _workingDirectory, value); }
        }
        public IAbsoluteDirectoryPath RepositoryDirectory
        {
            get { return _repositoryDirectory; }
            set { SetProperty(ref _repositoryDirectory, value); }
        }
        [IgnoreDataMember]
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public bool ComparePK(object other) {
            var o = other as RepositoryItem;
            return o != null && ComparePK(o);
        }

        public bool ComparePK(RepositoryItem other) {
            return other != null && other.WorkingDirectory == WorkingDirectory &&
                   other.RepositoryDirectory == RepositoryDirectory;
        }

        public PackageItem CreatePackage() {
            var package = new PackageItem("New Package", Handler, Enumerable.Empty<SpecificVersion>());
            Packages.Items.Add(package);
            return package;
        }

        async Task HandleRepo() {
            await Handler.UpdateRepository(WorkingDirectory, RepositoryDirectory).ConfigureAwait(false);

            Bundles.ProcessBundles();
            Packages.ProcessPackages();
        }

        public BundleItem CreateCollection() {
            var collection = new BundleItem("New Collection", Handler);
            Bundles.Items.Add(collection);
            return collection;
        }

        [DoNotObfuscate]
        public void OpenInExplorer() {
            Tools.FileUtil.OpenFolderInExplorer(Handler.BundleManager.WorkDir.ToString());
        }

        [DoNotObfuscate]
        public void OpenSynqInExplorer() {
            Tools.FileUtil.OpenFolderInExplorer(Handler.Repository.RootPath.ToString());
        }

        [DoNotObfuscate]
        public void OpenInCommandPrompt() {
            Tools.FileUtil.OpenCommandPrompt(Handler.BundleManager.WorkDir.ToString());
        }
    }
}