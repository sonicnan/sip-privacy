﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;

using LumiSoft.SIP.UA.UI;

namespace LumiSoft.SIP.UA
{
    /// <summary>
    /// Application main class.
    /// </summary>
    public static class Program
    {
        #region method Main

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            try{
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(delegate(object sender,UnhandledExceptionEventArgs e){
                    Console.WriteLine(e.ExceptionObject.ToString());
                    MessageBox.Show(null,"Error: " + e.ExceptionObject.ToString(),"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);                    
                });
                Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(delegate(object sender,System.Threading.ThreadExceptionEventArgs e){
                    Console.WriteLine(e.Exception.ToString());
                    MessageBox.Show(null,"Error: " + e.Exception.ToString(),"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);                    
                });
           
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new wfrm_Main(IsDebug));
            }
            catch(Exception x){
                MessageBox.Show(null,"Error: " + x.ToString(),"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets if program is running in debug mode.
        /// </summary>
        public static bool IsDebug
        {
            get{ return true; }
        }

        #endregion
    }
}
