// <copyright company="SIX Networks GmbH" file="UserconfigProcessorBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.IO;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Play.Core.Games.Legacy.Arma
{
    public abstract class UserconfigProcessorBase : IEnableLogging
    {
        protected bool ConfirmUserconfigIsNotFile(string uconfig) {
            if (File.Exists(uconfig) && !Directory.Exists(uconfig)) {
                this.Logger().Warn("WARNING: Userconfig folder not existing (or is file), aborting");
                return false;
            }
            return true;
        }
    }
}