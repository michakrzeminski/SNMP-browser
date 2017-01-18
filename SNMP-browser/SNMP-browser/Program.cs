using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SNMP_browser
{
    static class Program
    {

        [STAThread]
        static void Main()
        {
            // Application.EnableVisualStyles();
            // Application.SetCompatibleTextRenderingDefault(false);
            // MainWindow mainwindow = new MainWindow();
            // Application.Run(mainwindow);
            SnmpClient.Instance.ipPort = "162";
        }
    }
}
