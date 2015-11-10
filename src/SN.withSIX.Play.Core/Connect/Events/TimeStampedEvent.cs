// <copyright company="SIX Networks GmbH" file="TimeStampedEvent.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SN.withSIX.Core;

namespace SN.withSIX.Play.Core.Connect.Events
{
    public abstract class TimeStampedEvent : EventArgs
    {
        public readonly DateTime TimeStamp;

        protected TimeStampedEvent() {
            TimeStamp = Tools.Generic.GetCurrentUtcDateTime;
        }
    }
}