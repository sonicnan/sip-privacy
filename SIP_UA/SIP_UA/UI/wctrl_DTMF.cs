using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace LumiSoft.SIP.UA.UI
{
    /// <summary>
    /// This class represents DTMF keypad control.
    /// </summary>
    public class wctrl_DTMF : UserControl
    {
        private Button m_p1          = null;
        private Button m_p2          = null;
        private Button m_p3          = null;
        private Button m_p4          = null;
        private Button m_p5          = null;
        private Button m_p6          = null;
        private Button m_p7          = null;
        private Button m_p8          = null;
        private Button m_p9          = null;
        private Button m_pAsterisk   = null;
        private Button m_p0          = null;
        private Button m_pCrossHatch = null;

        private wctrl_Call m_pCall = null;
        private DateTime   m_Start = DateTime.Now;       

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="call">SIP call.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>call</b> is null reference.</exception>
        public wctrl_DTMF(wctrl_Call call)
        {
            if(call == null){
                throw new ArgumentNullException("call");
            }

            InitUI();

            m_pCall = call;
        }

        #region method InitUI

        /// <summary>
        /// Creates and initializes UI.
        /// </summary>
        private void InitUI()
        {
            this.ClientSize = new Size(80,105);

            m_p1 = new Button();
            m_p1.Size = new Size(20,20);
            m_p1.Location = new Point(5,5);
            m_p1.Text = "1";
            m_p1.MouseDown += delegate(object sender,MouseEventArgs e){
                m_Start = DateTime.Now;
            };
            m_p1.Click += delegate(object sender,EventArgs e){
                OnDtmfKeyPressed(1,(int)(DateTime.Now - m_Start).TotalMilliseconds);
            };

            m_p2 = new Button();            
            m_p2.Size = new Size(20,20);
            m_p2.Location = new Point(30,5);
            m_p2.Text = "2";
            m_p2.MouseDown += delegate(object sender,MouseEventArgs e){
                m_Start = DateTime.Now;
            };
            m_p2.Click += delegate(object sender,EventArgs e){
                OnDtmfKeyPressed(2,(int)(DateTime.Now - m_Start).TotalMilliseconds);
            };

            m_p3 = new Button();           
            m_p3.Size = new Size(20,20);
            m_p3.Location = new Point(55,5);
            m_p3.Text = "3";
            m_p3.MouseDown += delegate(object sender,MouseEventArgs e){
                m_Start = DateTime.Now;
            };
            m_p3.Click += delegate(object sender,EventArgs e){
                OnDtmfKeyPressed(3,(int)(DateTime.Now - m_Start).TotalMilliseconds);
            };

            m_p4 = new Button();
            m_p4.Size = new Size(20,20);
            m_p4.Location = new Point(5,30);
            m_p4.Text = "4";
            m_p4.MouseDown += delegate(object sender,MouseEventArgs e){
                m_Start = DateTime.Now;
            };
            m_p4.Click += delegate(object sender,EventArgs e){
                OnDtmfKeyPressed(4,(int)(DateTime.Now - m_Start).TotalMilliseconds);
            };

            m_p5 = new Button();
            m_p5.Size = new Size(20,20);
            m_p5.Location = new Point(30,30);
            m_p5.Text = "5";
            m_p5.MouseDown += delegate(object sender,MouseEventArgs e){
                m_Start = DateTime.Now;
            };
            m_p5.Click += delegate(object sender,EventArgs e){
                OnDtmfKeyPressed(5,(int)(DateTime.Now - m_Start).TotalMilliseconds);
            };

            m_p6 = new Button();
            m_p6.Size = new Size(20,20);
            m_p6.Location = new Point(55,30);
            m_p6.Text = "6";
            m_p6.MouseDown += delegate(object sender,MouseEventArgs e){
                m_Start = DateTime.Now;
            };
            m_p6.Click += delegate(object sender,EventArgs e){
                OnDtmfKeyPressed(6,(int)(DateTime.Now - m_Start).TotalMilliseconds);
            };

            m_p7 = new Button();
            m_p7.Size = new Size(20,20);
            m_p7.Location = new Point(5,55);
            m_p7.Text = "7";
            m_p7.MouseDown += delegate(object sender,MouseEventArgs e){
                m_Start = DateTime.Now;
            };
            m_p7.Click += delegate(object sender,EventArgs e){
                OnDtmfKeyPressed(7,(int)(DateTime.Now - m_Start).TotalMilliseconds);
            };

            m_p8 = new Button();
            m_p8.Size = new Size(20,20);
            m_p8.Location = new Point(30,55);
            m_p8.Text = "8";
            m_p8.MouseDown += delegate(object sender,MouseEventArgs e){
                m_Start = DateTime.Now;
            };
            m_p8.Click += delegate(object sender,EventArgs e){
                OnDtmfKeyPressed(8,(int)(DateTime.Now - m_Start).TotalMilliseconds);
            };

            m_p9 = new Button();
            m_p9.Size = new Size(20,20);
            m_p9.Location = new Point(55,55);
            m_p9.Text = "9";
            m_p9.MouseDown += delegate(object sender,MouseEventArgs e){
                m_Start = DateTime.Now;
            };
            m_p9.Click += delegate(object sender,EventArgs e){
                OnDtmfKeyPressed(9,(int)(DateTime.Now - m_Start).TotalMilliseconds);
            };

            m_pAsterisk = new Button();
            m_pAsterisk.Size = new Size(20,20);
            m_pAsterisk.Location = new Point(5,80);
            m_pAsterisk.Text = "*";
            m_pAsterisk.MouseDown += delegate(object sender,MouseEventArgs e){
                m_Start = DateTime.Now;
            };
            m_pAsterisk.Click += delegate(object sender,EventArgs e){
                OnDtmfKeyPressed(10,(int)(DateTime.Now - m_Start).TotalMilliseconds);
            };
            
            m_p0 = new Button();
            m_p0.Size = new Size(20,20);
            m_p0.Location = new Point(30,80);
            m_p0.Text = "0";
            m_p0.MouseDown += delegate(object sender,MouseEventArgs e){
                m_Start = DateTime.Now;
            };
            m_p0.Click += delegate(object sender,EventArgs e){
                OnDtmfKeyPressed(0,(int)(DateTime.Now - m_Start).TotalMilliseconds);
            };

            m_pCrossHatch = new Button();
            m_pCrossHatch.Size = new Size(20,20);
            m_pCrossHatch.Location = new Point(55,80);
            m_pCrossHatch.Text = "#";
            m_pCrossHatch.MouseDown += delegate(object sender,MouseEventArgs e){
                m_Start = DateTime.Now;
            };
            m_pCrossHatch.Click += delegate(object sender,EventArgs e){
                OnDtmfKeyPressed(11,(int)(DateTime.Now - m_Start).TotalMilliseconds);
            };

            this.Controls.Add(m_p1);
            this.Controls.Add(m_p2);
            this.Controls.Add(m_p3);
            this.Controls.Add(m_p4);
            this.Controls.Add(m_p5);
            this.Controls.Add(m_p6);
            this.Controls.Add(m_p7);
            this.Controls.Add(m_p8);
            this.Controls.Add(m_p9);
            this.Controls.Add(m_pAsterisk);
            this.Controls.Add(m_p0);
            this.Controls.Add(m_pCrossHatch);
        }

        void m_p1_Click(object sender,EventArgs e)
        {
            
        }

        #endregion


        #region events implementation

        #region method OnDtmfKeyPressed

        private void OnDtmfKeyPressed(int key,int duration)
        {
            m_pCall.SendDTMF(key,duration);
        }

        #endregion

        #endregion
    }
}
