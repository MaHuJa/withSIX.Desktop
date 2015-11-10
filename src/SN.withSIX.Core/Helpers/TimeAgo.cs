// <copyright company="SIX Networks GmbH" file="TimeAgo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Core.Helpers
{
    public static class TimeAgo
    {
        public static string Ago(this DateTime date) {
            const int second = 1;
            const int minute = 60*second;
            const int hour = 60*minute;
            const int day = 24*hour;
            const int week = 7*day;
            const int month = 30*day;

            date = date.ToLocalTime();

            var ts = new TimeSpan(Tools.Generic.GetCurrentDateTime.Ticks - date.Ticks);
            var delta = Math.Abs(ts.TotalSeconds);

            if (delta < 0)
                return "not yet";
            if (delta < 1*minute)
                return "just now"; //ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
            if (delta < 2*minute)
                return "a minute ago";
            if (delta < 45*minute)
                return ts.Minutes + " minutes ago";
            if (delta < 90*minute)
                return "an hour ago";
            if (delta < 24*hour)
                return ts.Hours + " hours ago";
            if (delta < 48*hour)
                return "yesterday";
            if (delta < 7*day)
                return ts.Days + " days ago";

            if (delta < 4*week) {
                var weeks = Convert.ToInt32(Math.Floor((double) ts.Days/7));
                return weeks <= 1 ? "one week ago" : weeks + " weeks ago";
            }

            if (delta < 12*month) {
                var months = Convert.ToInt32(Math.Floor((double) ts.Days/30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }

            var years = Convert.ToInt32(Math.Floor((double) ts.Days/365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }
    }
}