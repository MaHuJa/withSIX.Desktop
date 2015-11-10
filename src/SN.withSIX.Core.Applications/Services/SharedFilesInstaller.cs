// <copyright company="SIX Networks GmbH" file="SharedFilesInstaller.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Core.Applications.Services
{
    public class SharedFilesInstallFailedException : Exception
    {
        public SharedFilesInstallFailedException(string message, Exception innerException)
            : base(message, innerException) {}
    }
}