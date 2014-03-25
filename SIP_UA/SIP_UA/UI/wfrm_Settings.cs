using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace LumiSoft.SIP.UA.UI
{
    /// <summary>
    /// This class implements settings window.
    /// </summary>
    public class wfrm_Settings : Form
    {
        private ListBox m_pList = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public wfrm_Settings()
        {
            InitUI();
        }

        #region method InitUI

        /// <summary>
        /// Creates and initializes UI.
        /// </summary>
        private void InitUI()
        {
            this.ClientSize = new Size(200,300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Settings";

            m_pList = new ListBox();
            m_pList.Size = new Size(150,300);
            m_pList.Location = new Point(0,0);
            m_pList.IntegralHeight = false;
            //m_pList.ItemHeight = 20;
            m_pList.Font = new Font(m_pList.Font.FontFamily,10);
            m_pList.Items.Add("Accounts");
            m_pList.Items.Add("Credentials");
            m_pList.Items.Add("Audio");
            
            this.Controls.Add(m_pList);
        }

        #endregion


        #region Events handling

        #endregion
    }
}
