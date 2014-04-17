using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using LumiSoft.Net.Media.Codec.Audio;
using LumiSoft.Net.SIP;
using LumiSoft.Net.SIP.Stack;
using LumiSoft.Net.SIP.Message;
using Security.Key;
using Security.Cryptography;

namespace LumiSoft.Net.SIP.UA
{
    /// <summary>
    /// This class implements SIP UA. Defined in RFC 3261 8.1.
    /// </summary>
    //[Obsolete("Use SIP stack instead.")]
    public class SIP_UA : IDisposable
    {
        private bool              m_IsDisposed = false;
        private SIP_Stack         m_pStack     = null;
        private List<SIP_UA_Call> m_pCalls     = null;
        private List<SIP_UA_Registration> m_pRegistrations = null;
        private object            m_pLock      = new object();
        private OfflineKeyServiceProvider m_offlinekey = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_UA()
        {
            m_pStack = new SIP_Stack();
            m_pStack.RequestReceived += new EventHandler<SIP_RequestReceivedEventArgs>(m_pStack_RequestReceived);

            m_pRegistrations = new List<SIP_UA_Registration>();
            m_pCalls = new List<SIP_UA_Call>();
        }


        /// <summary>
        /// Default constructor with SIP_Stack.
        /// </summary>
        public SIP_UA(SIP_Stack stack)
        {

            if (stack == null)
            {
                throw new ArgumentNullException("stack");
            }

            m_pStack = stack;
            m_pStack.RequestReceived += new EventHandler<SIP_RequestReceivedEventArgs>(m_pStack_RequestReceived);

            m_pRegistrations = new List<SIP_UA_Registration>();
            m_pCalls = new List<SIP_UA_Call>();
        }
             
        #region method Dispose

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            lock(m_pLock){
                if(m_IsDisposed){
                    return;
                }
                                            
                // Hang up all calls.
                foreach(SIP_UA_Call call in m_pCalls.ToArray()){
                    call.Terminate("Hang up", true);
                }

                // Unregister registrations.
                foreach (SIP_UA_Registration reg in m_pRegistrations.ToArray())
                {
                    reg.BeginUnregister(true);
                }

                // Wait till all registrations and calls disposed or wait timeout reached.
                DateTime start = DateTime.Now;
                while(m_pCalls.Count > 0){
                    System.Threading.Thread.Sleep(500);

                    // Timeout, just kill all UA.
                    if(((TimeSpan)(DateTime.Now - start)).Seconds > 15){
                        break;
                    }
                }

                m_IsDisposed = true;

                this.IncomingCall = null;

                if (m_pStack != null)
                {
                    m_pStack.Dispose();
                    m_pStack = null;
                }               
            }
        }

        #endregion


        #region Events handling


        #region method m_pStack_RequestReceived

        /// <summary>
        /// This method is called when SIP stack received new message.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pStack_RequestReceived(object sender,SIP_RequestReceivedEventArgs e)
        {
            OnRequestReceived(e);
        }


        /// <summary>
        /// This method is called when new request is received.
        /// </summary>
        /// <param name="e">Request event arguments.</param>
        private void OnRequestReceived(SIP_RequestReceivedEventArgs e)
        {
            // TODO: Performance: rise events on thread pool or see if this method called on pool aready, then we may not keep lock for events ?

            if(e.Request.RequestLine.Method == SIP_Methods.CANCEL){
                /* RFC 3261 9.2.
                    If the UAS did not find a matching transaction for the CANCEL
                    according to the procedure above, it SHOULD respond to the CANCEL
                    with a 481 (Call Leg/Transaction Does Not Exist).
                  
                    Regardless of the method of the original request, as long as the
                    CANCEL matched an existing transaction, the UAS answers the CANCEL
                    request itself with a 200 (OK) response.
                */

                SIP_ServerTransaction trToCancel = m_pStack.TransactionLayer.MatchCancelToTransaction(e.Request);
                if(trToCancel != null){
                    trToCancel.Cancel();
                    e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x200_Ok,e.Request));
                }
                else{
                    e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x481_Call_Transaction_Does_Not_Exist,e.Request));
                }
            }
            else if(e.Request.RequestLine.Method == SIP_Methods.BYE){
                /* RFC 3261 15.1.2.
                    If the BYE does not match an existing dialog, the UAS core SHOULD generate a 481
                    (Call/Transaction Does Not Exist) response and pass that to the server transaction.
                */
                // TODO:

                SIP_Dialog dialog = m_pStack.TransactionLayer.MatchDialog(e.Request);
                if(dialog != null){
                    e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x200_Ok,e.Request));
                    dialog.Terminate();
                }
                else{
                    e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x481_Call_Transaction_Does_Not_Exist,e.Request));
                }
            }
            else if(e.Request.RequestLine.Method == SIP_Methods.INVITE){

                SIP_Uri m_from = new SIP_Uri();
                m_from.ParseInternal(e.Request.From.Address.Uri.ToString());
                SIP_Uri m_to = new SIP_Uri();
                m_to.ParseInternal(e.Request.To.Address.Uri.ToString());

                m_offlinekey = new OfflineKeyServiceProvider(m_to.User, m_to.Address, m_to.User);

                Offlinekey offkey = m_offlinekey.getOfflinekey(e.Request.Hash.Parameters["tag"].Value);

                if (!Hmac.versign(e.Request.Hash.Value, e.Request.DiffieHellman.Value + m_from.User, offkey.key))
                {
                    e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x400_Bad_Request, e.Request));
                    return;
                }

                // Supress INVITE retransmissions.
                e.ServerTransaction.SendResponse(m_pStack.CreateResponse(SIP_ResponseCodes.x100_Trying,e.Request));
         
                // Create call.
                SIP_UA_Call call = new SIP_UA_Call(this,e.ServerTransaction);
                call.StateChanged += new EventHandler(Call_StateChanged);
                m_pCalls.Add(call);

                OnIncomingCall(call);
            }
            else{
                OnRequestReceived(e);
            }
        }

        #endregion

        #region method Call_StateChanged

        /// <summary>
        /// Thsi method is called when call state has chnaged.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void Call_StateChanged(object sender,EventArgs e)
        {
            SIP_UA_Call call = (SIP_UA_Call)sender;            
            if(call.State == SIP_UA_CallState.Terminated){
                m_pCalls.Remove(call);
            }
        }

        #endregion

        #endregion


        #region method CreateRegistration

        /// <summary>
        /// Creates new registration.
        /// </summary>
        /// <param name="server">Registrar server URI. For example: sip:domain.com.</param>
        /// <param name="aor">Registration address of record. For example: user@domain.com.</param>
        /// <param name="contact">Contact URI.</param>
        /// <param name="expires">Gets after how many seconds reigisration expires.</param>
        /// <returns>Returns created registration.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this method is accessed.</exception>
        /// <exception cref="ArgumentNullException">Is raised when <b>server</b>,<b>aor</b> or <b>contact</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public SIP_UA_Registration CreateRegistration(SIP_Uri server, string aor, AbsoluteUri contact, int expires)
        {
            if (m_pStack.State == SIP_StackState.Disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            if (aor == null)
            {
                throw new ArgumentNullException("aor");
            }
            if (aor == string.Empty)
            {
                throw new ArgumentException("Argument 'aor' value must be specified.");
            }
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            lock (m_pRegistrations)
            {
                SIP_UA_Registration registration = new SIP_UA_Registration(this.Stack, server, aor, contact, expires);
                registration.Disposed += new EventHandler(delegate(object s, EventArgs e)
                {
                    if (m_pStack.State != SIP_StackState.Disposed)
                    {
                        m_pRegistrations.Remove(registration);
                    }
                });
                m_pRegistrations.Add(registration);

                return registration;
            }
        }

        #endregion



        #region method CreateCall

        /// <summary>
        /// Creates call to <b>invite</b> specified recipient.
        /// </summary>
        /// <param name="invite">INVITE request.</param>
        /// <returns>Returns created call.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>invite</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the argumnets has invalid value.</exception>
        public SIP_UA_Call CreateCall(SIP_Request invite)
        {
            if(invite == null){
                throw new ArgumentNullException("invite");
            }
            if(invite.RequestLine.Method != SIP_Methods.INVITE){
                throw new ArgumentException("Argument 'invite' is not INVITE request.");
            }

            SIP_Uri m_from = new SIP_Uri();
            m_from.ParseInternal(invite.From.Address.Uri.ToString());
            m_offlinekey = new OfflineKeyServiceProvider(m_from.User, m_from.Address, m_from.User);

            SIP_Uri m_to = new SIP_Uri();
            m_to.ParseInternal(invite.To.Address.Uri.ToString());
            RSAcrypto m_publickey = new RSAcrypto();
            m_publickey.LoadPublicFromXml(m_to.Host);
            m_to.User = RSAcrypto.PublicEncryption(m_to.User, m_publickey.publickey);
            invite.To.Parse(m_to.ToString());
            invite.RequestLine.Uri = AbsoluteUri.Parse(m_to.ToString());


            lock(m_pLock){
                SIP_UA_Call call = new SIP_UA_Call(this,invite);
                call.StateChanged += new EventHandler(Call_StateChanged);
                m_pCalls.Add(call);

                return call;
            }
        }
                                
        #endregion

        
        #region Properties implementation

        /// <summary>
        /// Gets if this object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get{ return m_IsDisposed; }
        }

        /// <summary>
        /// Gets SIP stack.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
        public SIP_Stack Stack
        {
            get{
                if(m_IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_pStack; 
            }
        }

        /// <summary>
        /// Gets OfflineKeyServiceProvider.
        /// </summary>
        public OfflineKeyServiceProvider Offlinekey
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_offlinekey;
            }
        }

        /// <summary>
        /// Gets active calls.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
        public SIP_UA_Call[] Calls
        {
            get{
                if(m_IsDisposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_pCalls.ToArray();
            }
        }


        /// <summary>
        /// Gets current registrations.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and and this property is accessed.</exception>
        public SIP_UA_Registration[] Registrations
        {
            get
            {
                if (m_pStack.State == SIP_StackState.Disposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return m_pRegistrations.ToArray();
            }
        }



        #endregion

        #region Events implementation


        /// <summary>
        /// Is raised when new incoming call.
        /// </summary>
        public event EventHandler<SIP_UA_Call_EventArgs> IncomingCall = null;

        #region method OnIncomingCall

        /// <summary>
        /// Raises event <b>IncomingCall</b>.
        /// </summary>
        /// <param name="call">Incoming call.</param>
        private void OnIncomingCall(SIP_UA_Call call)
        {
            if(this.IncomingCall != null){
                this.IncomingCall(this,new SIP_UA_Call_EventArgs(call));
            }
        }

        #endregion

        #endregion

    }
}
