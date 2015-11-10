// <copyright company="SIX Networks GmbH" file="NotificationBaseDataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.MVVM.ViewModels;

namespace SN.withSIX.Play.Applications.DataModels
{
    public abstract class NotificationBaseDataModel : DataModel
    {
        protected NotificationBaseDataModel(string categoryIcon) {
            TimeStamp = DateTime.Now;
            CategoryIcon = categoryIcon;
            OneTimeAction = true;
        }

        public DateTime TimeStamp { get; private set; }
        public string CategoryIcon { get; private set; }
        public IDispatchCommand OnClickDispatch { get; protected set; }
        public IDispatchCommand CloseCommand { get; private set; }
        public bool OneTimeAction { get; set; }
    }
}