// <copyright company="SIX Networks GmbH" file="SixArma2Net.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using Arma2Net.AddInProxy;

namespace SN.withSIX.Arma2Net.Presentation.Plugin
{
    [AddIn("SixArma2Net", Version = "1.0.0.0", Publisher = "SIX Networks",
        Description = "Used to plug into Play withSIX")]
    public class SixArma2Net : MethodAddIn
    {
        public void PublishMission(string mission, string world) {
            if (mission.StartsWith("/") || mission.StartsWith("\\"))
                mission = mission.Substring(1);
            using (Process.Start("pws://?publishMission=" + Uri.EscapeDataString(mission + "." + world))) {}
        }
    }
}