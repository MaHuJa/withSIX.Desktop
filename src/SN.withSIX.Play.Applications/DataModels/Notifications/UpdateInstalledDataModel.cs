// <copyright company="SIX Networks GmbH" file="UpdateInstalledDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Applications;

namespace SN.withSIX.Play.Applications.DataModels.Notifications
{
    public class UpdateInstalledDataModel : NotificationBaseDataModel
    {
        string _notificationIcon;
        string _version;
        public UpdateInstalledDataModel() : base(SixIconFont.withSIX_icon_Rocket) {}
        public string Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }
        public string NotificationIcon
        {
            get { return _notificationIcon; }
            set { SetProperty(ref _notificationIcon, value); }
        }
    }
}