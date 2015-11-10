// <copyright company="SIX Networks GmbH" file="ContentBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Core;
using SN.withSIX.Play.Core.Options;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Models")]
    public abstract class ContentBase : SyncBase, IHaveNotes, IFavorite
    {
        [DataMember] string _Author;
        string[] _categories;
        [DataMember] string _Description;
        [DataMember] string _HomepageUrl;
        [DataMember] string _Image;
        [DataMember] string _ImageLarge;
        [DataMember] string _Name;
        protected ContentBase(Guid id) : base(id) {}
        public string HomepageUrl
        {
            get { return _HomepageUrl; }
            set { SetProperty(ref _HomepageUrl, value); }
        }
        public bool HasImage { get; set; }
        public string Description
        {
            get { return _Description; }
            set { SetProperty(ref _Description, value); }
        }
        public string Author
        {
            get { return _Author; }
            set { SetProperty(ref _Author, value); }
        }
        public virtual string Name
        {
            get { return _Name; }
            set { SetProperty(ref _Name, value); }
        }
        public string[] Categories
        {
            get { return _categories; }
            set { SetProperty(ref _categories, value); }
        }
        public string Image
        {
            get { return _Image; }
            set { SetProperty(ref _Image, value); }
        }
        public string ImageLarge
        {
            get { return _ImageLarge; }
            set { SetProperty(ref _ImageLarge, value); }
        }
        public abstract bool IsFavorite { get; set; }
        public abstract bool HasNotes { get; }
        public abstract string Notes { get; set; }
        public virtual string NoteName
        {
            get { return Name; }
        }

        public static string GetResourcePath(string resource, int size = 0) {
            if (!String.IsNullOrWhiteSpace(resource))
                return Common.App.GetResourcePath(resource);
            switch (size) {
            case 1:
                return Common.App.GetResourcePath(Common.AppCommon.DefaultModResourceFileLarge);
            case 2:
                return Common.App.GetResourcePath(Common.AppCommon.DefaultModResourceFileHuge);
            default:
                return Common.App.GetResourcePath(Common.AppCommon.DefaultModResourceFile);
            }
        }

        public override string ToString() {
            return GetType().Name + ": " + Name;
        }
    }
}