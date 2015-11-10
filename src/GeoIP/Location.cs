// <copyright company="SIX Networks GmbH">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

//using System.Math;

public class Location
{
    static double EARTH_DIAMETER = 2*6378.2;
    static double PI = 3.14159265;
    static readonly double RAD_CONVERT = PI/180;
    public int area_code;
    public String city;
    public String countryCode;
    public String countryName;
    public int dma_code;
    public double latitude;
    public double longitude;
    public int metro_code;
    public String postalCode;
    public String region;
    public String regionName;

    public double distance(Location loc) {
        double delta_lat, delta_lon;
        double temp;

        var lat1 = latitude;
        var lon1 = longitude;
        var lat2 = loc.latitude;
        var lon2 = loc.longitude;

        // convert degrees to radians
        lat1 *= RAD_CONVERT;
        lat2 *= RAD_CONVERT;

        // find the deltas
        delta_lat = lat2 - lat1;
        delta_lon = (lon2 - lon1)*RAD_CONVERT;

        // Find the great circle distance
        temp = Math.Pow(Math.Sin(delta_lat/2), 2) + Math.Cos(lat1)*Math.Cos(lat2)*Math.Pow(Math.Sin(delta_lon/2), 2);
        return EARTH_DIAMETER*Math.Atan2(Math.Sqrt(temp), Math.Sqrt(1 - temp));
    }
}