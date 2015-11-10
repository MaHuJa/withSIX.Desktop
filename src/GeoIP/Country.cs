// <copyright company="SIX Networks GmbH">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

public class Country
{
    readonly String code;
    readonly String name;

    /**
     * Creates a new Country.
     *
     * @param code the country code.
     * @param name the country name.
     */

    public Country(String code, String name) {
        this.code = code;
        this.name = name;
    }

    /**
     * Returns the ISO two-letter country code of this country.
     *
     * @return the country code.
     */

    public String getCode() {
        return code;
    }

    /**
     * Returns the name of this country.
     *
     * @return the country name.
     */

    public String getName() {
        return name;
    }
}