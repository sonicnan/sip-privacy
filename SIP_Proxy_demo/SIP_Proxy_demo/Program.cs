using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SIP_Proxy_demo
{
    /// <summary>
    /// Application main class.
    /// </summary>
    public static class Program
    {
        #region static method Main

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            //Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new wfrm_Main(true));
        }
                       
        #endregion


        #region method CurrentDomain_UnhandledException

        /// <summary>
        /// This method is called when unhandled exception happens.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_UnhandledException(object sender,UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(null,"Error:\r\n" + ((Exception)e.ExceptionObject).ToString(),"Unhandled Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }

        #endregion

        #region method Application_ThreadException

        /// <summary>
        /// This method is called when unhandled exception happens.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender,System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(null,"Error:\r\n" + e.Exception.ToString(),"Unhandled Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }

        #endregion
    }
}