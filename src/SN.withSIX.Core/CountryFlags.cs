// <copyright company="SIX Networks GmbH" file="CountryFlags.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using SmartAssembly.Attributes;

namespace SN.withSIX.Core
{
    [DoNotObfuscateType]
    public enum CountryFlags
    {
        Afghanistan,
        Albania,
        Algeria,
        American_Samoa,
        Andorra,
        Angola,
        Anguilla,
        Antigua_and_Barbuda,
        Argentina,
        Armenia,
        Aruba,
        Australia,
        Austria,
        Azerbaijan,
        Bahamas,
        Bahrain,
        Bangladesh,
        Barbados,
        Belarus,
        Belgium,
        Belize,
        Benin,
        Bermuda,
        Bhutan,
        Bolivia,
        Bosnia,
        Botswana,
        Brazil,
        British_Virgin_Islands,
        Brunei,
        Bulgaria,
        Burkina_Faso,
        Burundi,
        Cambodia,
        Cameroon,
        Canada,
        Cape_Verde,
        Cayman_Islands,
        Central_African_Republic,
        Chad,
        Chile,
        China,
        Christmas_Island,
        Colombia,
        Comoros,
        Cook_Islands,
        Costa_Rica,
        Croatia,
        Cuba,
        Cyprus,
        Czech_Republic,
        Democratic_Republic_of_the_Congo,
        Denmark,
        Djibouti,
        Dominica,
        Dominican_Republic,
        East_Timor,
        Ecuador,
        Egypt,
        El_Salvador,
        Empty,
        Equatorial_Guinea,
        Eritrea,
        Estonia,
        Ethiopia,
        EU,
        Falkland_Islands,
        Faroe_Islands,
        Fiji,
        Finland,
        France,
        French_Polynesia,
        Gabon,
        Gambia,
        Georgia,
        Germany,
        Ghana,
        Gibraltar,
        Greece,
        Greenland,
        Grenada,
        Guam,
        Guatemala,
        Guinea,
        Guinea_Bissau,
        Guyana,
        Haiti,
        Honduras,
        Hong_Kong,
        Hungary,
        Iceland,
        India,
        Indonesia,
        Iran,
        Iraq,
        Ireland,
        Israel,
        Italy,
        Ivory_Coast,
        Jamaica,
        Japan,
        Jordan,
        Kazakhstan,
        Kenya,
        Kiribati,
        Kuwait,
        Kyrgyzstan,
        Laos,
        Latvia,
        Lebanon,
        Lesotho,
        Liberia,
        Libya,
        Liechtenstein,
        Lithuania,
        Luxembourg,
        Macao,
        Macedonia,
        Madagascar,
        Malawi,
        Malaysia,
        Maldives,
        Mali,
        Malta,
        Marshall_Islands,
        Martinique,
        Mauritania,
        Mauritius,
        Mexico,
        Micronesia,
        Moldova,
        Monaco,
        Mongolia,
        Montenegro,
        Montserrat,
        Morocco,
        Mozambique,
        Myanmar,
        Namibia,
        Nauru,
        Nepal,
        Netherlands,
        Netherlands_Antilles,
        New_Zealand,
        Nicaragua,
        Niger,
        Nigeria,
        Niue,
        Norfolk_Island,
        North_Korea,
        Norway,
        Oman,
        Pakistan,
        Palau,
        Palestine,
        Panama,
        Papua_New_Guinea,
        Paraguay,
        Peru,
        Philippines,
        Pitcairn_Islands,
        Poland,
        Portugal,
        Puerto_Rico,
        Qatar,
        Republic_of_the_Congo,
        Romania,
        Russian_Federation,
        Rwanda,
        Saint_Kitts_and_Nevis,
        Saint_Lucia,
        Saint_Pierre,
        Saint_Vincent_and_the_Grenadines,
        Samoa,
        San_Marino,
        Sao_Tom�_and_Pr�ncipe,
        Saudi_Arabia,
        Senegal,
        Serbia,
        Seychelles,
        Sierra_Leone,
        Singapore,
        Slovakia,
        Slovenia,
        Soloman_Islands,
        Somalia,
        South_Africa,
        South_Georgia,
        South_Korea,
        Spain,
        Sri_Lanka,
        Sudan,
        Suriname,
        Swaziland,
        Sweden,
        Switzerland,
        Syria,
        Taiwan,
        Tajikistan,
        Tanzania,
        Thailand,
        Togo,
        Tonga,
        Trinidad_and_Tobago,
        Tunisia,
        Turkey,
        Turkmenistan,
        Turks_and_Caicos_Islands,
        Tuvalu,
        UAE,
        Uganda,
        Ukraine,
        United_Kingdom,
        United_States_of_America,
        Uruguay,
        US_Virgin_Islands,
        Uzbekistan,
        Vanuatu,
        Vatican_City,
        Venezuela,
        Vietnam,
        Wallis_and_Futuna,
        Yemen,
        Zambia,
        Zimbabwe
    }

    public class CountryFlagsMapping
    {
        public static Dictionary<string, string> CountryDict = new Dictionary<string, string> {
            {"AD", "Andorra"},
            {"AE", "United_Arab_Emirates"},
            {"AF", "Afghanistan"},
            {"AG", "Antigua_and_Barbuda"},
            {"AI", "Anguilla"},
            {"AL", "Albania"},
            {"AM", "Armenia"},
            {"AN", "Netherlands_Antilles"},
            {"AO", "Angola"},
            {"AQ", "Antarctica"},
            {"AR", "Argentina"},
            {"AS", "American_Samoa"},
            {"AT", "Austria"},
            {"AU", "Australia"},
            {"AW", "Aruba"},
            {"AX", "Aland_Islands"},
            {"AZ", "Azerbaijan"},
            {"BA", "Bosnia_and_Herzegovina"},
            {"BB", "Barbados"},
            {"BD", "Bangladesh"},
            {"BE", "Belgium"},
            {"BF", "Burkina_Faso"},
            {"BG", "Bulgaria"},
            {"BH", "Bahrain"},
            {"BI", "Burundi"},
            {"BJ", "Benin"},
            {"BM", "Bermuda"},
            {"BN", "Brunei_Darussalam"},
            {"BO", "Bolivia"},
            {"BR", "Brazil"},
            {"BS", "Bahamas"},
            {"BT", "Bhutan"},
            {"BV", "Bouvet_Island"},
            {"BW", "Botswana"},
            {"BY", "Belarus"},
            {"BZ", "Belize"},
            {"CA", "Canada"},
            //{"CC", "Cocos_"}, (Keeling)_Islands
            {"CD", "Democratic_Republic_of_the_Congo"},
            {"CF", "Central_African_Republic"},
            {"CG", "Congo"},
            {"CH", "Switzerland"},
            {"CI", "Ivory_Coast"},
            {"CK", "Cook_Islands"},
            {"CL", "Chile"},
            {"CM", "Cameroon"},
            {"CN", "China"},
            {"CO", "Colombia"},
            {"CR", "Costa_Rica"},
            {"CU", "Cuba"},
            {"CV", "Cape_Verde"},
            {"CX", "Christmas_Island"},
            {"CY", "Cyprus"},
            {"CZ", "Czech_Republic"},
            {"DE", "Germany"},
            {"DJ", "Djibouti"},
            {"DK", "Denmark"},
            {"DM", "Dominica"},
            {"DO", "Dominican_Republic"},
            {"DZ", "Algeria"},
            {"EC", "Ecuador"},
            {"EE", "Estonia"},
            {"EG", "Egypt"},
            {"EH", "Western_Sahara"},
            {"ER", "Eritrea"},
            {"ES", "Spain"},
            {"ET", "Ethiopia"},
            {"FI", "Finland"},
            {"FJ", "Fiji"},
            {"FK", "Falkland_Islands"},
            {"FM", "Federated_States_of_Micronesia"},
            {"FO", "Faroe_Islands"},
            {"FR", "France"},
            // {"FX", "France"}, Metropolitan
            {"GA", "Gabon"},
            {"GB", "United_Kingdom"},
            {"GD", "Grenada"},
            {"GE", "Georgia"},
            {"GF", "French_Guiana"},
            {"GH", "Ghana"},
            {"GI", "Gibraltar"},
            {"GL", "Greenland"},
            {"GM", "Gambia"},
            {"GN", "Guinea"},
            {"GP", "Guadeloupe"},
            {"GQ", "Equatorial_Guinea"},
            {"GR", "Greece"},
            {"GS", "S_Georgia_and_S._Sandwich_Islands"},
            {"GT", "Guatemala"},
            {"GU", "Guam"},
            {"GW", "Guinea_Bissau"},
            {"GY", "Guyana"},
            {"HK", "Hong_Kong"},
            //{"HM", "Heard_Island_and_McDonald_Islands"},
            {"HN", "Honduras"},
            {"HR", "Croatia"},
            {"HT", "Haiti"},
            {"HU", "Hungary"},
            {"ID", "Indonesia"},
            {"IE", "Ireland"},
            {"IL", "Israel"},
            {"IN", "India"},
            {"IO", "British_Indian_Ocean_Territory"},
            {"IQ", "Iraq"},
            {"IR", "Iran"},
            {"IS", "Iceland"},
            {"IT", "Italy"},
            {"JM", "Jamaica"},
            {"JO", "Jordan"},
            {"JP", "Japan"},
            {"KE", "Kenya"},
            {"KG", "Kyrgyzstan"},
            {"KH", "Cambodia"},
            {"KI", "Kiribati"},
            {"KM", "Comoros"},
            {"KN", "Saint_Kitts_and_Nevis"},
            {"KP", "North_Korea"},
            {"KR", "South_Korea"},
            {"KW", "Kuwait"},
            {"KY", "Cayman_Islands"},
            {"KZ", "Kazakhstan"},
            {"LA", "Laos"},
            {"LB", "Lebanon"},
            {"LC", "Saint_Lucia"},
            {"LI", "Liechtenstein"},
            {"LK", "Sri_Lanka"},
            {"LR", "Liberia"},
            {"LS", "Lesotho"},
            {"LT", "Lithuania"},
            {"LU", "Luxembourg"},
            {"LV", "Latvia"},
            {"LY", "Libya"},
            {"MA", "Morocco"},
            {"MC", "Monaco"},
            {"MD", "Moldova"},
            {"ME", "Montenegro"},
            {"MG", "Madagascar"},
            {"MH", "Marshall_Islands"},
            {"MK", "Macedonia"},
            {"ML", "Mali"},
            {"MM", "Myanmar"},
            {"MN", "Mongolia"},
            {"MO", "Macao"},
            {"MP", "Northern_Mariana_Islands"},
            {"MQ", "Martinique"},
            {"MR", "Mauritania"},
            {"MS", "Montserrat"},
            {"MT", "Malta"},
            {"MU", "Mauritius"},
            {"MV", "Maldives"},
            {"MW", "Malawi"},
            {"MX", "Mexico"},
            {"MY", "Malaysia"},
            {"MZ", "Mozambique"},
            {"NA", "Namibia"},
            {"NC", "New_Caledonia"},
            {"NE", "Niger"},
            {"NF", "Norfolk_Island"},
            {"NG", "Nigeria"},
            {"NI", "Nicaragua"},
            {"NL", "Netherlands"},
            {"NO", "Norway"},
            {"NP", "Nepal"},
            {"NR", "Nauru"},
            {"NU", "Niue"},
            {"NZ", "New_Zealand"},
            {"OM", "Oman"},
            {"PA", "Panama"},
            {"PE", "Peru"},
            {"PF", "French_Polynesia"},
            {"PG", "Papua_New_Guinea"},
            {"PH", "Philippines"},
            {"PK", "Pakistan"},
            {"PL", "Poland"},
            {"PM", "Saint_Pierre_and_Miquelon"},
            {"PN", "Pitcairn"},
            {"PR", "Puerto_Rico"},
            {"PS", "Palestine"},
            {"PT", "Portugal"},
            {"PW", "Palau"},
            {"PY", "Paraguay"},
            {"QA", "Qatar"},
            {"RE", "Reunion"},
            {"RO", "Romania"},
            {"RS", "Serbia"},
            {"RU", "Russian_Federation"},
            {"RW", "Rwanda"},
            {"SA", "Saudi_Arabia"},
            {"SB", "Solomon_Islands"},
            {"SC", "Seychelles"},
            {"SD", "Sudan"},
            {"SE", "Sweden"},
            {"SG", "Singapore"},
            {"SH", "Saint_Helena"},
            {"SI", "Slovenia"},
            {"SJ", "Svalbard_and_Jan_Mayen"},
            {"SK", "Slovakia"},
            {"SL", "Sierra_Leone"},
            {"SM", "San_Marino"},
            {"SN", "Senegal"},
            {"SO", "Somalia"},
            {"SR", "Suriname"},
            {"ST", "Sao_Tome_and_Principe"},
            //{"SU", "USSR_"},
            {"SV", "El_Salvador"},
            {"SY", "Syria"},
            {"SZ", "Swaziland"},
            {"TC", "Turks_and_Caicos_Islands"},
            {"TD", "Chad"},
            {"TF", "French_Southern_Territories"},
            {"TG", "Togo"},
            {"TH", "Thailand"},
            {"TJ", "Tajikistan"},
            {"TK", "Tokelau"},
            //{"TL", "Timor"},
            {"TM", "Turkmenistan"},
            {"TN", "Tunisia"},
            {"TO", "Tonga"},
            {"TP", "East_Timor"},
            {"TR", "Turkey"},
            {"TT", "Trinidad_and_Tobago"},
            {"TV", "Tuvalu"},
            {"TW", "Taiwan"},
            {"TZ", "Tanzania"},
            {"UA", "Ukraine"},
            {"UG", "Uganda"},
            {"UK", "United_Kingdom"},
            {"UM", "United_States_Minor_Outlying_Islands"},
            {"US", "United_States_of_America"},
            {"UY", "Uruguay"},
            {"UZ", "Uzbekistan"},
            {"VA", "Vatican_City"},
            {"VC", "Saint_Vincent_and_the_Grenadines"},
            {"VE", "Venezuela"},
            {"VG", "British_Virgin_Islands"},
            {"VI", "US_Virgin_Islands"},
            {"VN", "Viet_Nam"},
            {"VU", "Vanuatu"},
            {"WF", "Wallis_and_Futuna"},
            {"WS", "Samoa"},
            {"YE", "Yemen"},
            {"YT", "Mayotte"},
            // {"YU", "Yugoslavia_"},
            {"ZA", "South_Africa"},
            {"ZM", "Zambia"},
            // {"ZR", "Zaire_"}
            {"ZW", "Zimbabwe"}
        };
    }
}