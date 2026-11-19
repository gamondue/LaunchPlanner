using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace gamon.LaunchPlanner
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
            frmAbout fAbout = new frmAbout();
            Application.Run(fAbout);
            frmBackground frmBackground = new frmBackground();
            Application.Run(frmBackground);
        }
    }
}