using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using LumiSoft.SIP.UA.Resources;

namespace LumiSoft.SIP.UA.UI
{
    /// <summary>
    /// This class implements add/edit contact window.
    /// </summary>
    public class wfrm_Contact : Form
    {
        private Label    mt_DisplayName = null;
        private TextBox  m_pDisplayName = null;
        private ComboBox m_pPhone1Type = null;
        private Label    mt_SipUri      = null;
        private TextBox  m_pSipUri      = null;
        private Label    mt_Email       = null;
        private TextBox  m_pEmail       = null;
        private Label    mt_Web         = null;
        private TextBox  m_pWeb         = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public wfrm_Contact()
        {
            InitUI();
        }

        #region method InitUI

        /// <summary>
        /// Creates and initializes UI.
        /// </summary>
        private void InitUI()
        {
            this.ClientSize = new Size(300,200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = ResManager.GetIcon("im.ico",new Size(16,16));
            this.Text = "Add/edit contact.";

            mt_DisplayName = new Label();
            mt_DisplayName.Size = new Size(100,20);
            mt_DisplayName.Location = new Point(5,20);
            mt_DisplayName.TextAlign = ContentAlignment.MiddleLeft;
            mt_DisplayName.Text = "Display Name:";

            m_pDisplayName = new TextBox();
            m_pDisplayName.Size = new Size(150,20);
            m_pDisplayName.Location = new Point(105,20);

            m_pPhone1Type = new ComboBox();
            m_pPhone1Type.Size = new Size(100,20);
            m_pPhone1Type.Location = new Point(5,45);
            m_pPhone1Type.DropDownStyle = ComboBoxStyle.DropDownList;
            m_pPhone1Type.Items.Add("SIP");

            mt_SipUri = new Label();

            m_pSipUri = new TextBox();

            mt_Email = new Label();

            m_pEmail = new TextBox();

            mt_Web = new Label();

            m_pWeb = new TextBox();

            this.Controls.Add(mt_DisplayName);
            this.Controls.Add(m_pDisplayName);
        }

        #endregion


        #region Events handling

        #endregion
    }
}
