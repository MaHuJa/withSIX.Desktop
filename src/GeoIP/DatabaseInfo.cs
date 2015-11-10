// <copyright company="SIX Networks GmbH">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

public class DatabaseInfo
{
    public static int COUNTRY_EDITION = 1;
    public static int REGION_EDITION_REV0 = 7;
    public static int REGION_EDITION_REV1 = 3;
    public static int CITY_EDITION_REV0 = 6;
    public static int CITY_EDITION_REV1 = 2;
    public static int ORG_EDITION = 5;
    public static int ISP_EDITION = 4;
    public static int PROXY_EDITION = 8;
    public static int ASNUM_EDITION = 9;
    public static int NETSPEED_EDITION = 10;

    //private static SimpleDateFormat formatter = new SimpleDateFormat("yyyyMMdd");

    readonly String info;
    /**
     * Creates a new DatabaseInfo object given the database info String.
     * @param info
     */

    public DatabaseInfo(String info) {
        this.info = info;
    }

    public int getType() {
        if ((info == null) | (info == ""))
            return COUNTRY_EDITION;
        // Get the type code from the database info string and then
        // subtract 105 from the value to preserve compatability with
        // databases from April 2003 and earlier.
        return Convert.ToInt32(info.Substring(4, 7)) - 105;
    }

    /**
     * Returns true if the database is the premium version.
     *
     * @return true if the premium version of the database.
     */

    public bool isPremium() {
        return info.IndexOf("FREE") < 0;
    }

    /**
     * Returns the date of the database.
     *
     * @return the date of the database.
     */

    public DateTime getDate() {
        for (var i = 0; i < info.Length - 9; i++) {
            if (Char.IsWhiteSpace(info[i])) {
                var dateString = info.Substring(i + 1, i + 9);
                try {
                    //synchronized (formatter) {
                    return DateTime.ParseExact(dateString, "yyyyMMdd", null);
                    //}
                } catch (Exception e) {
                    Console.Write(e.Message);
                }
                break;
            }
        }
        return DateTime.Now;
    }

    public String toString() {
        return info;
    }
}