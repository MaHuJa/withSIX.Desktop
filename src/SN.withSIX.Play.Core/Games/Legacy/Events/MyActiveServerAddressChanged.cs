// <copyright company="SIX Networks GmbH" file="MyActiveServerAddressChanged.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Core.Games.Legacy.Events
{
    public class MyActiveServerAddressChanged : EventArgs
    {
        public readonly ServerAddress Address;

        public MyActiveServerAddressChanged(ServerAddress address) {
            Address = address;
        }
    }
}