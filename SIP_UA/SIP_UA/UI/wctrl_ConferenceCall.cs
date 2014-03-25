using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LumiSoft.SIP.UA.UI
{
    /// <summary>
    /// This class represents SIP conference call.
    /// </summary>
    public class wctrl_ConferenceCall
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        internal wctrl_ConferenceCall()
        {
        }


        #region method Join

        /// <summary>
        /// Joins call to conference call.
        /// </summary>
        /// <param name="call">SIP call to join.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>call</b> is null reference.</exception>
        public void Join(wctrl_Call call)
        {
            if(call == null){
                throw new ArgumentNullException("call");
            }

            // TODO:
        }

        #endregion

        // public void Disjoin(call)


        #region Properties implementation

        //public wctrl_Call[] Calls

        #endregion
    }
}
