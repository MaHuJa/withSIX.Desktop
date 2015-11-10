// <copyright company="SIX Networks GmbH" file="RvProfileInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Mini.Plugin.Arma.Attributes
{
    public class RvProfileInfoAttribute : Attribute
    {
        public RvProfileInfoAttribute(string mainName, string otherProfilesName, string profileExtension) {
            //            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(mainName));
            //            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(otherProfilesName));
            //            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(profileExtension));

            DocumentsMainName = mainName;
            DocumentsOtherProfilesName = otherProfilesName;
            ProfileExtension = profileExtension;
        }

        public string DocumentsMainName { get; private set; }
        public string DocumentsOtherProfilesName { get; private set; }
        public string ProfileExtension { get; private set; }
    }
}