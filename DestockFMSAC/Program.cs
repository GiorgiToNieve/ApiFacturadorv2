using System;
using System.Linq;
using System.Windows.Forms;
using log4net.Config;

namespace DestockFMSAC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            #region Configuración Log4Net
            log4net.GlobalContext.Properties["LogDate"] = DateTime.Now.ToString("yyyyMMdd");
            XmlConfigurator.Configure();
            #endregion

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmTransaccion());
        }
    }
}