using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace LumiSoft.SIP.UA.UI
{
    /// <summary>
    /// Settings -> Accounts window.
    /// </summary>
    public class wfrm_Settings_Accounts : Form
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public wfrm_Settings_Accounts()
        {
            InitUI();
        }

        #region method InitUI

        /// <summary>
        /// Creates and initializes UI.
        /// </summary>
        private void InitUI()
        {
            this.Size = new Size(400,200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "SIP Accounts";
        }

        #endregion
    }
}
