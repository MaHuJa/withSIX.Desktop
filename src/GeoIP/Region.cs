// <copyright company="SIX Networks GmbH">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

public class Region
{
    public String countryCode;
    public String countryName;
    public String region;

    public Region() {}

    public Region(String countryCode, String countryName, String region) {
        this.countryCode = countryCode;
        this.countryName = countryName;
        this.region = region;
    }

    public String getcountryCode() {
        return countryCode;
    }

    public String getcountryName() {
        return countryName;
    }

    public String getregion() {
        return region;
    }
}