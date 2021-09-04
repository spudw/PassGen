using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace PassGen
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll")] private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)] static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        const int SW_RESTORE = 9;
        public static string sExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        public static string sProcName = Process.GetCurrentProcess().ProcessName + ".exe";
        public static string sInstallFolderName = "\\PassGen";
        public static string sInstallPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + sInstallFolderName + "\\";
        public static bool SILENTSTART = false;

        private void IsApplication_AlreadyRunning()
        {
            //string sProcName = Process.GetCurrentProcess().ProcessName;
            Process[] processes = Process.GetProcessesByName(sProcName);
            if (processes.Length > 1)
            {
                var hWnd = FindWindow(null, MainGUI.sTitle);
                ShowWindowAsync(hWnd, SW_RESTORE);
                System.Environment.Exit(1);
            }
        }

        private bool IsApplication_AutoStartEnabled()
        {
            RegistryKey oKey = PassGenFuncs.RegistryOpen();
            var Value = oKey.GetValue("AutoStart");
            return (Value == null || Convert.ToInt32(Value) == 0) ? false : true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            IsApplication_AlreadyRunning();
            if (IsApplication_AutoStartEnabled())
            {
                //MessageBox.Show("Application Starting\nArgument Count: " + e.Args.Length);
                for (int i = 0; i != e.Args.Length; ++i)
                {
                    if (e.Args[i] == "/silent")
                    {
                        SILENTSTART = true;
                    }
                }
            }
            //if (IsApplication_AutoStartEnabled()){
            //    if (sExePath != sInstallPath)
            //    {
            //        
            //    }
            //}
        }
    }
}