using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace SIP_Proxy_demo
{
    /// <summary>
    /// Add/edit user window.
    /// </summary>
    public class wfrm_User : Form
    {
        private Label    mt_UserName   = null;
        private TextBox  m_pUserName   = null;
        private Label    mt_Password   = null;
        private TextBox  m_pPassword   = null;
        private Label    mt_AOR        = null;
        private TextBox  m_pAOR        = null;
        private GroupBox m_pSeparator1 = null;
        private Button   m_pCancel     = null;
        private Button   m_pOk         = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public wfrm_User()
        {
            InitUI();
        }

        #region method InitUI

        /// <summary>
        /// Created and initializes UI.
        /// </summary>
        private void InitUI()
        {
            this.ClientSize = new Size(350,135);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Text = "Add User";

            mt_UserName = new Label();
            mt_UserName.Size = new Size(100,20);
            mt_UserName.Location = new Point(0,10);
            mt_UserName.TextAlign = ContentAlignment.MiddleRight;
            mt_UserName.Text = "User Name:";

            m_pUserName = new TextBox();
            m_pUserName.Size = new Size(235,20);
            m_pUserName.Location = new Point(105,10);

            mt_Password = new Label();
            mt_Password.Size = new Size(100,20);
            mt_Password.Location = new Point(0,35);
            mt_Password.TextAlign = ContentAlignment.MiddleRight;
            mt_Password.Text = "Password:";

            m_pPassword = new TextBox();
            m_pPassword.Size = new Size(235,35);
            m_pPassword.Location = new Point(105,35);

            mt_AOR = new Label();
            mt_AOR.Size = new Size(100,20);
            mt_AOR.Location = new Point(0,60);
            mt_AOR.TextAlign = ContentAlignment.MiddleRight;
            mt_AOR.Text = "Address of Record:";

            m_pAOR = new TextBox();
            m_pAOR.Size = new Size(235,20);
            m_pAOR.Location = new Point(105,60);
            
            m_pSeparator1 = new GroupBox();
            m_pSeparator1.Size = new Size(340,3);
            m_pSeparator1.Location = new Point(5,95);

            m_pCancel = new Button();
            m_pCancel.Size = new Size(70,20);
            m_pCancel.Location = new Point(195,105);
            m_pCancel.Text = "Cancel";
            m_pCancel.Click += new EventHandler(m_pCancel_Click);

            m_pOk = new Button();
            m_pOk.Size = new Size(70,20);
            m_pOk.Location = new Point(270,105);
            m_pOk.Text = "Ok";
            m_pOk.Click += new EventHandler(m_pOk_Click);

            this.Controls.Add(mt_UserName);
            this.Controls.Add(m_pUserName);
            this.Controls.Add(mt_Password);
            this.Controls.Add(m_pPassword);
            this.Controls.Add(mt_AOR);
            this.Controls.Add(m_pAOR);
            this.Controls.Add(m_pSeparator1);
            this.Controls.Add(m_pCancel);
            this.Controls.Add(m_pOk);
        }
                                
        #endregion


        #region Events Handling

        #region method m_pCancel_Click

        private void m_pCancel_Click(object sender,EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region method m_pOk_Click

        private void m_pOk_Click(object sender,EventArgs e)
        {
            if(m_pUserName.Text.Length == 0){
                MessageBox.Show(this,"Please fill user name !","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            if(m_pPassword.Text.Length == 0){
                MessageBox.Show(this,"Please fill password !","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            if(m_pAOR.Text.Length == 0){
                MessageBox.Show(this,"Please fill address of record !","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        #endregion

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets active user name.
        /// </summary>
        public string UserName
        {
            get{ return m_pUserName.Text; }
        }

        /// <summary>
        /// Gets active password.
        /// </summary>
        public string Password
        {
            get{ return m_pPassword.Text; }
        }

        /// <summary>
        /// Gets active address of record.
        /// </summary>
        public string AddressOfRecord
        {
            get{ return m_pAOR.Text; }
        }

        #endregion

    }
}