using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;

namespace MControl.Caching.Install
{
    /// <summary>
    /// Settings
    /// </summary>
    internal class Settings
    {
        internal const string ServiceProcess = "Nistec.Cache.Agent.exe";
        internal const string ServiceName = "MControl.Cache";
        internal const string DisplayName = "MControl.Cache";
        internal const string WindowsAppProcess = "MControl.Cache.Server.exe";
        internal const string TrayAppProcess = "";
        internal const string ManagerAppProcess = "";



        /// <summary>
        /// Gets if server service is installed.
        /// </summary>
        /// <returns></returns>
        public static bool IsServiceInstalled()
        {
            foreach (ServiceController service in ServiceController.GetServices())
            {
                if (service.ServiceName == Settings.ServiceName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets server service installed.
        /// </summary>
        /// <param name="createIfNotInstalled"></param>
        /// <returns></returns>
        public static ServiceController GetServiceInstalled(bool createIfNotInstalled)
        {
            foreach (ServiceController service in ServiceController.GetServices())
            {
                if (service.ServiceName == Settings.ServiceName)
                {
                    return service;
                }
            }

            if (createIfNotInstalled)
            {
                var service = new ServiceController(Settings.ServiceName);
                service.DisplayName = Settings.DisplayName;
                service.ServiceName = Settings.ServiceName;
                return service;
            }
            return null;
        }
    }
}
