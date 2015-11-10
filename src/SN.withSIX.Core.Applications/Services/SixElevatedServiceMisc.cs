// <copyright company="SIX Networks GmbH" file="SixElevatedServiceMisc.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Linq;
using System.ServiceProcess;

namespace SN.withSIX.Core.Applications.Services
{
    public static class SixElevatedServiceMisc
    {
        public static string SixElevatedServiceName = "SixElevatedService";

        public static bool IsElevatedServiceInstalled() {
            var Controller = ServiceController.GetServices()
                .FirstOrDefault(x => x.ServiceName == SixElevatedServiceName);

            return (Controller != null);
        }

        public static ServiceControllerStatus GetElevatedServiceStatus() {
            var Controller = ServiceController.GetServices()
                .FirstOrDefault(x => x.ServiceName == SixElevatedServiceName);

            return Controller.Status;
        }
    }
}