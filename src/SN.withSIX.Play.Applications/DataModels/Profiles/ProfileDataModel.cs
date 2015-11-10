// <copyright company="SIX Networks GmbH" file="ProfileDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Applications.DataModels.Profiles
{
    public class ProfileDataModel : DataModelRequireId<Guid>
    {
        bool _canDelete;
        string _color;
        bool _isActive;
        string _name;
        public ProfileDataModel(Guid id) : base(id) {}
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public string Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }
        public bool CanDelete
        {
            get { return _canDelete; }
            set { SetProperty(ref _canDelete, value); }
        }
        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }
    }
}