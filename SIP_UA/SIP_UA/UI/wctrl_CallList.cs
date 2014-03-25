using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace LumiSoft.SIP.UA.UI
{
    /// <summary>
    /// This class implements SIP call list UI control.
    /// </summary>
    public class wctrl_CallList : Panel
    {
        private List<wctrl_Call> m_pCalls = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public wctrl_CallList()
        {
            InitUI();

            m_pCalls = new List<wctrl_Call>();
        }

        #region method InitUI

        /// <summary>
        /// Creates and initializes UI.
        /// </summary>
        private void InitUI()
        {
            this.BorderStyle = BorderStyle.FixedSingle;
            this.AutoScroll = true;
        }

        #endregion


        #region method AddCall

        /// <summary>
        /// Adds call to calls list.
        /// </summary>
        /// <param name="call">Call to add.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>call</b> is null reference.</exception>
        public void AddCall(wctrl_Call call)
        {
            if(call == null){
                throw new ArgumentNullException("call");
            }

            if(m_pCalls.Count == 0){
                call.Location = new Point(0,0);
                call.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }
            else{
                call.Location = new Point(0,m_pCalls[m_pCalls.Count - 1].Bottom + 1);
                call.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }

            call.Disposed += delegate(object s1, EventArgs e1){
                RemoveCall((wctrl_Call)s1);
            };
            this.Controls.Add(call);
            m_pCalls.Add(call);

            // Update calls width.
            foreach(wctrl_Call c in m_pCalls){
                c.Width = this.ClientSize.Width;
            }
        }

        #endregion

        #region method RemoveCall

        /// <summary>
        /// Removes call from calls list.
        /// </summary>
        /// <param name="call">Call to remove.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>call</b> is null reference.</exception>
        public void RemoveCall(wctrl_Call call)
        {
            if(call == null){
                throw new ArgumentNullException("call");
            }

            int index = m_pCalls.IndexOf(call);
            if(index > -1){
                int y = call.Top;

                m_pCalls.Remove(call);
                this.Controls.Remove(call);
                                
                // Reorder calls below "call".
                for(int i=index;i<m_pCalls.Count;i++){
                    m_pCalls[i].Top = y;
                    y += m_pCalls[i].Height;
                }

                // Update calls width.
                foreach(wctrl_Call c in m_pCalls){
                    c.Width = this.ClientSize.Width;
                }
            }
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets call list.
        /// </summary>
        public wctrl_Call[] Calls
        {
            get{ return m_pCalls.ToArray(); }
        }

        #endregion
    }
}
