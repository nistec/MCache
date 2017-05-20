//licHeader
//===============================================================================================================
// System  : Nistec.Cache - Nistec.Cache Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of cache core.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using Nistec.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace Nistec
{

    public enum ServiceCmd
    {
        Install,
        Uninstall,
        Start,
        Stop,
        Restart,
        Pause,
        //ServiceStatus,
        //ServiceDeatils,
        RunAsWindow

    }
    class ServiceManager
    {
        //bool tbPauseEnabled = false;
        //bool tbStartEnabled = false;
        //bool tbStopEnabled = false;
        //bool tbRestartEnabled = false;
        //bool tbInstallEnabled = false;

        ServiceController m_controller;


        public static void DisplayUsage()
        {

            System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName(Settings.ServiceName);
            int usage = 0;
            if (process == null)
                return ;
            for (int i = 0; i < process.Length; i++)
            {
                usage += (int)((int)process[i].WorkingSet64) / 1024;
            }
            Console.WriteLine("Service Usage : {0}.", usage);
        }


        bool FindIsServiceInstalled()
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

        public bool IsServiceInstalled()
        {
            if (m_controller == null)
                return false;
            return m_controller.ServiceName == Settings.ServiceName;
        }
        public void DispalyServiceStatus()
        {
            if (m_controller == null)
                Console.WriteLine("service not installed");
            Console.WriteLine("service status: {0}", m_controller.Status);
        }
        public ServiceControllerStatus GetServiceStatus()
        {
            if (m_controller == null)
                return (ServiceControllerStatus) 0;
            return m_controller.Status;
        }
        public bool IsServiceStarted()
        {
            if (m_controller == null)
                return false;
            return m_controller.Status == ServiceControllerStatus.Running;
        }

        public int DoServiceCommand(ServiceCmd cmd)
        {
            switch (cmd)
            {
                case ServiceCmd.Install:
                    if (IsServiceInstalled())
                    {
                        Console.WriteLine("Service allready installed!");
                        return -1;
                    }
                    System.Diagnostics.Process.Start(Application.StartupPath + "/" + Settings.ServiceProcess, "/i");
                    return 1;
                case ServiceCmd.Uninstall:
                    if (!IsServiceInstalled())
                    {
                        Console.WriteLine("Service not installed!");
                        return -1;
                    }
                    System.Diagnostics.Process.Start(Application.StartupPath + "/" + Settings.ServiceProcess, "/u");
                    return 1;
                case ServiceCmd.Start:
                    if (!IsServiceInstalled())
                    {
                        Console.WriteLine("Service not installed!");
                        return -1;
                    }
                    if (IsServiceStarted())
                    {
                        Console.WriteLine("Service allready started!");
                        return -1;
                    }
                    DoStart();
                    return 1;
                case ServiceCmd.Stop:
                    if (!IsServiceInstalled())
                    {
                        Console.WriteLine("Service not installed!");
                        return -1;
                    }
                    if (!IsServiceStarted())
                    {
                        Console.WriteLine("Service allready stoped!");
                        return -1;
                    }
                    DoStop();
                    return 1;
                case ServiceCmd.Restart:
                    if (!IsServiceInstalled())
                    {
                        Console.WriteLine("Service not installed!");
                        return -1;
                    }
                    if (!IsServiceStarted())
                    {
                        Console.WriteLine("Service allready stoped!");
                        return -1;
                    }
                    DoRestart();
                    return 1;
                case ServiceCmd.Pause:
                    if (!IsServiceInstalled())
                    {
                        Console.WriteLine("Service not installed!");
                        return -1;
                    }
                    if (!IsServiceStarted())
                    {
                        Console.WriteLine("Service allready stoped!");
                        return -1;
                    }
                    DoPause();
                    return 1;
                case ServiceCmd.RunAsWindow:
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        System.Diagnostics.Process.Start("mono", Application.StartupPath + "/" + Settings.WindowsAppProcess + " -winform");
                    }
                    else
                    {
                        System.Diagnostics.Process.Start(Application.StartupPath + "/" + Settings.WindowsAppProcess, "-winform");
                    }
                    return 1;
            }
            return 0;
        }

        #region service

        protected string GetServiceTypeName(ServiceType type)
        {
            string serviceType = "";
            if ((type & ServiceType.InteractiveProcess) != 0)
            {
                serviceType = "Interactive ";
                type -= ServiceType.InteractiveProcess;
            }
            switch (type)
            {
                case ServiceType.Adapter:
                    serviceType += "Adapter";
                    break;
                case ServiceType.FileSystemDriver:
                case ServiceType.KernelDriver:
                case ServiceType.RecognizerDriver:
                    serviceType += "Driver";
                    break;
                case ServiceType.Win32OwnProcess:
                    serviceType += "Win32 Service Process";
                    break;
                case ServiceType.Win32ShareProcess:
                    serviceType += "Win32 Shared Process";
                    break;
                default:
                    serviceType += "unknown type " + type.ToString();
                    break;
            }
            return serviceType;
        }
        /*
        protected void SetServiceStatus(ServiceController controller)
        {

            bool isEnabled = controller != null;
            tbStartEnabled = isEnabled;
            tbStopEnabled = isEnabled;
            tbPauseEnabled = isEnabled;
            tbRestartEnabled = isEnabled;
            //tbInstall.Enabled = !isEnabled;// !Settings.IsServiceInstalled();
            if (controller != null)
            {
                if (!controller.CanPauseAndContinue)
                {
                    tbPauseEnabled = false;
                    //tbRestart.Enabled = false;
                }
                if (!controller.CanStop)
                {
                    tbStopEnabled = false;
                }
                SetServiceStatus(controller.Status);
            }

        }
        protected void SetServiceStatus(ServiceControllerStatus status)
        {

            switch (status)
            {
                case ServiceControllerStatus.ContinuePending:
                    //textServiceStatus.Text = "Continue Pending";
                    tbPauseEnabled = false;
                    //tbInstall.Enabled = false;
                    break;
                case ServiceControllerStatus.Paused:
                    //textServiceStatus.Text = "Paused";
                    tbPauseEnabled = false;
                    tbStartEnabled = false;
                    //tbInstall.Enabled = false;
                    break;
                case ServiceControllerStatus.PausePending:
                    //textServiceStatus.Text = "Pause Pending";
                    tbPauseEnabled = false;
                    tbStartEnabled = false;
                    //tbInstall.Enabled = false;
                    break;
                case ServiceControllerStatus.StartPending:
                    //textServiceStatus.Text = "Start Pending";
                    tbStartEnabled = false;
                    //tbInstall.Enabled = false;
                    break;
                case ServiceControllerStatus.Running:
                    //textServiceStatus.Text = "Running";
                    tbStartEnabled = false;
                    //tbInstall.Enabled = false;
                    break;
                case ServiceControllerStatus.Stopped:
                    //textServiceStatus.Text = "Stopped";
                    tbStopEnabled = false;
                    tbRestartEnabled = false;
                    //tbInstall.Enabled = true;
                    break;
                case ServiceControllerStatus.StopPending:
                    //textServiceStatus.Text = "Stop Pending";
                    tbStopEnabled = false;
                    tbRestartEnabled = false;
                    tbInstallEnabled = false;
                    break;
                default:
                    //textServiceStatus.Text = "Unknown status";
                    //tbInstall.Enabled = true;

                    break;
            }

        }
        */
        #endregion

        #region Service controller

        
        private void LoadServiceController()
        {
            if (m_controller == null)
            {
                var agent = Settings.GetServiceInstalled();
                if (agent != null)
                {
                    m_controller = agent.ServiceController;
                }
            }
        }

        private ServiceController GetServiceController()
        {
            LoadServiceController();
            return m_controller;
        }

        private bool IsServiceControllerInstalled()
        {
            LoadServiceController();
            return (m_controller != null);
        }

        private bool IsServiceControllerRunning()
        {
            LoadServiceController();

            if (m_controller != null)
            {
                return m_controller.Status == ServiceControllerStatus.Running;
            }

            return false;
        }

        public void ShowServiceDetails()
        {
            Console.WriteLine("Service Details...");
            LoadServiceController();
            if (m_controller == null)
            {
                Console.WriteLine("Service not installed");
                return;
            }
            Console.WriteLine("Service Name : {0}.", m_controller.ServiceName);
            Console.WriteLine("Service Type : {0}.", m_controller.ServiceType);
            Console.WriteLine("Service Status : {0}.", m_controller.Status);
            DisplayUsage();
            Console.WriteLine();
            //SetServiceStatus(m_controller);
        }
        /*
        private void DoRefreshSubAction(bool reset)
        {
            switch (curSubAction)
            {
                case SubActions.Performance:
                case SubActions.Statistic:
                case SubActions.Usage:
                    if (reset)
                    {
                        switch (MsgBox.ShowQuestionYNC("This action will reset performance counter, Continue? ", ""))
                        {
                            case System.Windows.Forms.DialogResult.Cancel:
                                break;
                            case System.Windows.Forms.DialogResult.OK:
                            case System.Windows.Forms.DialogResult.Yes:
                                DoResetPerformanceCounter();
                                DoRefreshPerformance(curSubAction);
                                break;
                            case System.Windows.Forms.DialogResult.No:
                                DoRefreshPerformance(curSubAction);
                                break;
                        }
                    }
                    else
                    {
                        DoRefreshPerformance(curSubAction);
                    }
                    break;
                default:
                    break;
            }
        }

        private void DoRefreshPerformance(SubActions action)
        {
            switch (action)
            {
                case SubActions.Performance:
                    DoPerformanceReport(); break;
                case SubActions.Statistic:
                    DoStatistic(); break;
                case SubActions.Usage:
                    DoUsage(); break;
            }
        }

        private bool IsPerformanceSubActions
        {
            get { return (curSubAction == SubActions.Performance || curSubAction == SubActions.Statistic || curSubAction == SubActions.Usage); }
        }

        private void DoRefresh()
        {
            bool hasError = false;

            if (IsPerformanceSubActions)
            {
                DoRefreshSubAction(true);
                return;
            }
            curSubAction = SubActions.Default;

            if (curAction == Actions.RemoteCache)
            {
                CreateNodeItems(true);
                ShowGridItems();
                //return;
            }
            else if (curAction == Actions.Session)
            {
                CreateNodeSession(true);
            }
            else if (curAction == Actions.SessionActive)
            {
                CreateNodeSession(SessionState.Active);
            }
            else if (curAction == Actions.SessionIdle)
            {
                CreateNodeSession(SessionState.Idle);
            }
            //else if (curAction == Actions.SessionItems)
            //{
            //    CreateNodeSessionItems();
            //}
            else if (curAction == Actions.DataCache)
            {
                CreateNodeDataItems(true);
                ShowGridDataItems();
                //return;
            }
            else if (curAction == Actions.SyncDb)
            {
                CreateNodeSyncItems(true);
                ShowGridSyncItems();
                //return;
            }
            else if (curAction == Actions.Services)
            {
                RefreshServiceList();
            }
        }
        */
        private void DoInstall()
        {
            bool hasError = false;
            try
            {
                Console.WriteLine("Install service...");
                if (DoServiceCommand(ServiceCmd.Install) > 0)
                {
                    //RefreshServiceList();
                    ShowServiceDetails();
                }
            }
            catch (Exception ex)
            {
                hasError = true;
                DisplayStatus("DoInstall", ex);
            }
            finally
            {
                //WaitDlg.EndProgress();
                DisplayFinallStatus("Service install completed...", hasError);
            }
        }

        private void DoPause()
        {
            bool hasError = false;
            try
            {
                //curSubAction = SubActions.Default;

                ServiceController controller = GetServiceController();
                if (controller == null)
                    return;
                //WaitDlg.RunProgress("Pause...");
                Console.WriteLine("Pause service...");
                if (controller.Status == ServiceControllerStatus.Paused || controller.Status == ServiceControllerStatus.PausePending)
                {
                    controller.Continue();
                    controller.WaitForStatus(ServiceControllerStatus.Running);
                }
                else
                {
                    controller.Pause();
                    controller.WaitForStatus(ServiceControllerStatus.Paused);
                }
                System.Threading.Thread.Sleep(1000);
                //SetServiceStatus(controller);
                //ShowServiceDetails();
            }
            catch (Exception ex)
            {
                hasError = true;
                DisplayStatus("DoPause", ex);
            }
            finally
            {
                //WaitDlg.EndProgress();
                DisplayFinallStatus("Service is Pauseed...", hasError);
            }
        }
        private void DoRestart()
        {
            bool hasError = false;
            try
            {
                //curSubAction = SubActions.Default;

                ServiceController controller = GetServiceController();
                if (controller == null)
                    return;
                //WaitDlg.RunProgress("Stop...");
                Console.WriteLine("Stop service...");
                controller.Stop();
                controller.WaitForStatus(ServiceControllerStatus.Stopped);
                System.Threading.Thread.Sleep(1000);
                //WaitDlg.RunProgress("Start...");
                Console.WriteLine("Start service...");
                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running);
                System.Threading.Thread.Sleep(1000);
                //SetServiceStatus(controller);
                //ShowServiceDetails();
            }
            catch (Exception ex)
            {
                hasError = true;
                DisplayStatus("DoRestart", ex);
            }
            finally
            {
                //WaitDlg.EndProgress();
                DisplayFinallStatus("Restart completed...", hasError);
            }

        }

        private void DoStart()
        {
            bool hasError = false;
            try
            {
                //curSubAction = SubActions.Default;

                if (m_controller == null)
                    return;
                //WaitDlg.RunProgress("Start...");
                Console.WriteLine("Start service...");
                m_controller.Start();
                m_controller.WaitForStatus(ServiceControllerStatus.Running);
                System.Threading.Thread.Sleep(1000);
                //SetServiceStatus(m_controller);
                //ShowServiceDetails();
            }
            catch (Exception ex)
            {
                hasError = true;
                DisplayStatus("DoStop", ex);
            }
            finally
            {
                //WaitDlg.EndProgress();
                DisplayFinallStatus("Start completed...", hasError);
            }
        }

        private void DoStop()
        {
            bool hasError = false;
            //ServiceController controller = null;
            try
            {
                //curSubAction = SubActions.Default;

                if (m_controller == null)
                    return;
                //WaitDlg.RunProgress("Stop...");
                Console.WriteLine("Stop service...");
                m_controller.Stop();
                m_controller.WaitForStatus(ServiceControllerStatus.Stopped);
                System.Threading.Thread.Sleep(1000);
                //SetServiceStatus(m_controller);
                ShowServiceDetails();
            }
            catch (Exception ex)
            {
                hasError = true;
                //SetServiceStatus(ServiceControllerStatus.StopPending);
                DisplayStatus("DoStop", ex);
            }
            finally
            {
                //WaitDlg.EndProgress();
                DisplayFinallStatus("Stop completed...", hasError);
            }
        }
        #endregion

        static void DisplayStatus(string method, Exception ex)
        {
            Console.WriteLine("Error {0}: {1}", method, ex.Message);
        }
        static void DisplayFinallStatus(string message, bool hasError)
        {
            if (!hasError)
                Console.WriteLine(message);
        }
    }
}
