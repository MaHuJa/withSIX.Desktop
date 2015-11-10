// <copyright company="SIX Networks GmbH" file="GeneralExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Extensions
{
    public static class GeneralExtensions
    {
        public enum TimeConversion
        {
            Milliseconds,
            Seconds
        }

        // TODO: TimeSpan has FromSeconds, and TotalSeconds, TotalMilliseconds etc... useful?
        public static int Seconds(this int seconds, TimeConversion convertTo = TimeConversion.Milliseconds) {
            if (convertTo == TimeConversion.Milliseconds)
                return seconds*1000;
            return seconds;
        }

        public static int Minutes(this int minutes, TimeConversion convertTo = TimeConversion.Milliseconds) {
            return (minutes*60).Seconds(convertTo);
        }

        public static int Hours(this int hours, TimeConversion convertTo = TimeConversion.Milliseconds) {
            return (hours*60).Minutes(convertTo);
        }

        public static int Days(this int days, TimeConversion convertTo = TimeConversion.Milliseconds) {
            return (days*24).Hours(convertTo);
        }
    }
}