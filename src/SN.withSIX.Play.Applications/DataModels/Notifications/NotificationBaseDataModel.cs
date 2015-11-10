// <copyright company="SIX Networks GmbH" file="NotificationBaseDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Applications;
using SN.withSIX.Core.Applications.Extensions;

namespace SN.withSIX.Play.Applications.DataModels.Notifications
{
    // Cheating
    public class DefaultNotificationDataModel : NotificationBaseDataModel
    {
        public DefaultNotificationDataModel(string subject, string message)
            : base(SixIconFont.withSIX_icon_Users_Friends) {
            Subject = subject;
            Message = message;
            OnClickDispatch = CloseCommand;
        }

        public DefaultNotificationDataModel(string subject, string message, IDispatchCommand command)
            : this(subject, message) {
            OnClickDispatch = command;
        }

        public string Subject { get; private set; }
        public string Message { get; private set; }
    }

    public class NewSoftwareUpdateAvailableNotificationDataModel : DefaultNotificationDataModel
    {
        public NewSoftwareUpdateAvailableNotificationDataModel(string subject, string message) : base(subject, message) {}

        public NewSoftwareUpdateAvailableNotificationDataModel(string subject, string message, IDispatchCommand command)
            : base(subject, message, command) {}
    }

    public class NewSoftwareUpdateDownloadedNotificationDataModel : DefaultNotificationDataModel
    {
        public NewSoftwareUpdateDownloadedNotificationDataModel(string subject, string message) : base(subject, message) {}

        public NewSoftwareUpdateDownloadedNotificationDataModel(string subject, string message, IDispatchCommand command)
            : base(subject, message, command) {}
    }
}