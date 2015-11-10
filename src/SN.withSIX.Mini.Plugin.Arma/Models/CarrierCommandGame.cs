// <copyright company="SIX Networks GmbH" file="CarrierCommandGame.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Core;
using SN.withSIX.Mini.Core.Games;
using SN.withSIX.Mini.Core.Games.Attributes;

namespace SN.withSIX.Mini.Plugin.Arma.Models
{
    [Game(GameUUids.CarrierCommand, Name = "Carrier Command: Gaea Mission", Slug = "Carrier-Command",
        Executables = new[] {"carrier.exe"})]
    [DataContract]
    public class CarrierCommandGame : BasicGame
    {
        protected CarrierCommandGame(Guid id) : this(id, new CarrierCommandGameSettings()) {}
        public CarrierCommandGame(Guid id, CarrierCommandGameSettings settings) : base(id, settings) {}
    }
}