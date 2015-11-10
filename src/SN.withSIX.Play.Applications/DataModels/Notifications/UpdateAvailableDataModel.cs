// <copyright company="SIX Networks GmbH" file="UpdateAvailableDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Applications;

namespace SN.withSIX.Play.Applications.DataModels.Notifications
{
    public class UpdateAvailableDataModel : NotificationBaseDataModel
    {
        public UpdateAvailableDataModel(string version, string icon)
            : base(SixIconFont.withSIX_icon_Download) {
            Version = version;
            Icon = icon;
        }

        public string Version { get; private set; }
        public string Icon { get; private set; }
    }
}