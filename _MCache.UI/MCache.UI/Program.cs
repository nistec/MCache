using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nistec.Caching.Remote.UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //McLock.Lock.ValidateLock();
            Application.Run(new CacheManagmentForm());
        }
    }
}