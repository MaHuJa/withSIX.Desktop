// <copyright company="SIX Networks GmbH" file="UnityStartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using System.Runtime.Serialization;

namespace SN.withSIX.Mini.Core.Games
{
    [DataContract]
    public abstract class UnityStartupParameters : BasicGameStartupParameters
    {
        protected UnityStartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
        [Category(GameSettingCategories.Game),
         Description(
             "Allow Game running even when its window does not have focus (i.e. running in the background)")
        ]
        public bool PopupWindow1198363464
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
    }
}