// <copyright company="SIX Networks GmbH" file="ContentDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Applications.DataModels.Games
{
    public abstract class ContentDataModel : DataModelRequireId<Guid>
    {
        string _author;
        string _description;
        bool _isFavorite;
        bool _isFree;
        string _name;
        DateTime _releasedOn;
        string _slug;
        Uri _storeUrl;
        Uri _supportUrl;
        protected ContentDataModel(Guid id) : base(id) {}
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public string Author
        {
            get { return _author; }
            set { SetProperty(ref _author, value); }
        }
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }
        public DateTime ReleasedOn
        {
            get { return _releasedOn; }
            set { SetProperty(ref _releasedOn, value); }
        }
        public string Slug
        {
            get { return _slug; }
            set { SetProperty(ref _slug, value); }
        }
        public Uri StoreUrl
        {
            get { return _storeUrl; }
            set { SetProperty(ref _storeUrl, value); }
        }
        public Uri SupportUrl
        {
            get { return _supportUrl; }
            set { SetProperty(ref _supportUrl, value); }
        }
        public bool IsFree
        {
            get { return _isFree; }
            set { SetProperty(ref _isFree, value); }
        }
        public bool IsFavorite
        {
            get { return _isFavorite; }
            set { SetProperty(ref _isFavorite, value); }
        }
    }
}