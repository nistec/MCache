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
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new CacheManagmentForm());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Remote.UI error: " + ex.Message);
                Application.Restart();
            }
        }
    }
}