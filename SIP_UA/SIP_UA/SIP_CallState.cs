using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.SIP.UA
{
    /// <summary>
    /// This enum specifies SIP UA call states.
    /// </summary>
    public enum SIP_CallState
    {
        /// <summary>
        /// Outgoing calling is in progress.
        /// </summary>
        Calling,

        /// <summary>
        /// Incoming call waits for accept or reject.
        /// </summary>
        Incoming,

        /// <summary>
        /// Call is active.
        /// </summary>
        Active,
                
        /// <summary>
        /// Call is terminating.
        /// </summary>
        Terminating,

        /// <summary>
        /// Call is terminated.
        /// </summary>
        Terminated,

        /// <summary>
        /// Call has disposed.
        /// </summary>
        Disposed
    }
}
