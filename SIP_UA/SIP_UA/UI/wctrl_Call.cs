using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Drawing;
using System.Windows.Forms;

using LumiSoft.SIP.UA.Resources;

using LumiSoft.Net;
using LumiSoft.Net.SDP;
using LumiSoft.Net.SIP;
using LumiSoft.Net.SIP.UA;
using LumiSoft.Net.SIP.Debug;
using LumiSoft.Net.SIP.Stack;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.Media;
using LumiSoft.Net.Media.Codec.Audio;
using LumiSoft.Net.RTP;
using LumiSoft.Net.RTP.Debug;
using LumiSoft.Net.UPnP.NAT;

namespace LumiSoft.SIP.UA.UI
{
    /// <summary>
    /// This class represent SIP call.
    /// </summary>
    public class wctrl_Call : UserControl
    {
        private Label  m_pStatusText  = null;
        private Label  m_pDuration    = null;
        private Label  m_pDisplayName = null;
        private Button m_pDTMF        = null;
        private Button m_pToggleHold  = null;
        private Button m_pAccept      = null;
        private Button m_pHangup      = null;

        private object                    m_pLock                 = new object();
        private SIP_CallState             m_CallState             = SIP_CallState.Calling;
        private wfrm_Main                 m_pMainUI               = null;      
        private SIP_RequestSender         m_pInitialInviteSender  = null;
        private SIP_ServerTransaction     m_pIncomingInvite       = null;
        private RTP_MultimediaSession     m_pRtpMultimediaSession = null;
        private DateTime                  m_StartTime;
        private SIP_Dialog_Invite         m_pDialog               = null;
        private TimerEx                   m_pKeepAliveTimer       = null;
        private SDP_Message               m_pLocalSDP             = null;
        private SDP_Message               m_pRemoteSDP            = null; 
        private bool                      m_IsOnHold              = false;
        private WavePlayer                m_pPlayer               = null;
        private AudioOutDevice            m_pAudioOutDevice       = null;
        private AudioInDevice             m_pAudioInDevice        = null;
        private Timer                     m_pTimerDuration        = null;
        private Dictionary<string,object> m_pTags                 = null;
        private SIP_UA m_pUA = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="mainUI">Reference to main UI.</param>
        /// <param name="audioOut">Audio out device.</param>
        /// <param name="audioIn">Audio in device.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>mainUI</b>,<b>audioOut</b> or <b>audioIn</b> is null reference.</exception>
        internal wctrl_Call(wfrm_Main mainUI,AudioOutDevice audioOut,AudioInDevice audioIn)
        {
            if(mainUI == null){
                throw new ArgumentNullException("mainUI");
            }
            if(audioOut == null){
                throw new ArgumentNullException("audioOut");
            }
            if(audioIn == null){
                throw new ArgumentNullException("audioIn");
            }

            m_pUA = mainUI.SIP_UA;
            m_pMainUI         = mainUI;
            m_pAudioOutDevice = audioOut;
            m_pAudioInDevice  = audioIn;
            m_pPlayer         = new WavePlayer(audioOut);

            InitUI();

            m_pTags = new Dictionary<string,object>();

            m_pStatusText.DoubleClick += new EventHandler(wctrl_Call_DoubleClick);
        }

        void wctrl_Call_DoubleClick(object sender, EventArgs e)
        {
            SendDTMF(2,500);
        }

        #region method InitUI

        /// <summary>
        /// Creates and initializes UI.
        /// </summary>
        private void InitUI()
        {
            this.Size = new Size(200,80);
            this.BorderStyle = BorderStyle.FixedSingle;            
            this.BackColor = Color.FromArgb(213,237,249);

            m_pStatusText = new Label();
            m_pStatusText.Size = new Size(100,20);
            m_pStatusText.Location = new Point(5,5);
            m_pStatusText.Font = new Font(m_pStatusText.Font.FontFamily,7);
            m_pStatusText.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            m_pDuration = new Label();
            m_pDuration.Size = new Size(70,20);
            m_pDuration.Location = new Point(115,5);
            m_pDuration.Anchor = AnchorStyles.Right | AnchorStyles.Top;

            m_pDisplayName = new Label();
            m_pDisplayName.Size = new Size(110,30);
            m_pDisplayName.Location = new Point(5,20);
            m_pDisplayName.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            m_pDisplayName.ForeColor = Color.Gray;
            m_pDisplayName.Font = new Font(m_pDisplayName.Font.FontFamily,7,FontStyle.Bold);
            m_pDisplayName.TextAlign = ContentAlignment.MiddleLeft;

            m_pDTMF = new Button();
            m_pDTMF.Enabled = false;
            m_pDTMF.Size = new Size(50,24);
            m_pDTMF.Location = new Point(60,50);
            m_pDTMF.Anchor = AnchorStyles.Right;
            m_pDTMF.Text = "DTMF";
            m_pDTMF.Click += new EventHandler(m_pDTMF_Click);

            m_pToggleHold = new Button();
            m_pToggleHold.Enabled = false;
            m_pToggleHold.Visible = false;
            m_pToggleHold.Size = new Size(50,24);
            m_pToggleHold.Location = new Point(115,50);
            m_pToggleHold.Anchor = AnchorStyles.Right;
            m_pToggleHold.Font = new Font(m_pToggleHold.Font.FontFamily,7);
            m_pToggleHold.Text = "Hold";
            m_pToggleHold.Click += new EventHandler(m_pToggleHold_Click);

            m_pAccept = new Button();
            m_pAccept.Visible = false;
            m_pAccept.Size = new Size(50,24);
            m_pAccept.Location = new Point(115,50);
            m_pAccept.Anchor = AnchorStyles.Right;
            m_pAccept.Font = new Font(m_pAccept.Font.FontFamily,7);
            m_pAccept.Text = "Accept";
            m_pAccept.Click += new EventHandler(m_pAccept_Click);

            m_pHangup = new Button();
            m_pHangup.Size = new Size(24,24);
            m_pHangup.Location = new Point(170,50);
            m_pHangup.Anchor = AnchorStyles.Right;
            m_pHangup.Image = ResManager.GetIcon("call_hangup.ico",new Size(16,16)).ToBitmap();
            m_pHangup.Click += new EventHandler(m_pHangup_Click);
            
            this.Controls.Add(m_pStatusText);
            this.Controls.Add(m_pDuration);
            this.Controls.Add(m_pDisplayName);
            this.Controls.Add(m_pDTMF);
            this.Controls.Add(m_pToggleHold);
            this.Controls.Add(m_pAccept);
            this.Controls.Add(m_pHangup);
        }
                                                                
        #endregion

        #region method Dispose

        /// <summary>
        /// Cleans up any resource being used.
        /// </summary>
        public new void Dispose()
        {  
            lock(m_pLock){
                if(this.State == SIP_CallState.Disposed){
                    return;
                }
                SetState(SIP_CallState.Disposed);

                //m_pUA.Stack = null;
                m_pLocalSDP = null;
                if(m_pDialog != null){
                    m_pDialog.Dispose();
                    m_pDialog = null;
                }
                if(m_pTimerDuration != null){
                    m_pTimerDuration.Dispose();
                    m_pTimerDuration = null;
                }
                if(m_pKeepAliveTimer != null){
                    m_pKeepAliveTimer.Dispose();
                    m_pKeepAliveTimer = null;
                }

                this.StateChanged = null;
            }
            
            // We need BeginInvoke here to access UI, we are running on thread pool thread.
            this.BeginInvoke(new MethodInvoker(delegate(){
                base.Dispose();
            }));            
        }

        #endregion


        #region method InitCalling

        /// <summary>
        /// Initializes new outgoing call.
        /// </summary>
        /// <param name="account">Local account.</param>
        /// <param name="to">Call target address.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>account</b> or <b>to</b> is null reference.</exception>
        internal void InitCalling(Account account,SIP_t_NameAddress to)
        {
            if(account == null){
                throw new ArgumentNullException("account");
            }
            if(to == null){
                throw new ArgumentNullException("to");
            }

            // We need BeginInvoke here to access UI, we are running on thread pool thread.
            this.BeginInvoke(new MethodInvoker(delegate(){
                m_CallState           = SIP_CallState.Calling;
                this.StateChanged    += this.m_pCall_StateChanged;
                m_pStatusText.Text    = "Calling";
                m_pDuration.Text      = "00:00:00";
                m_pDisplayName.Text   = to.Uri.ToString();
                m_pToggleHold.Visible = true;
                m_pHangup.Enabled     = false;
            }));
            
            bool sips = false;
            if(string.Equals(to.Uri.Scheme,"sips",StringComparison.InvariantCultureIgnoreCase)){
                sips = true;
            }

            #region Setup RTP session
            
            m_pRtpMultimediaSession = new RTP_MultimediaSession(RTP_Utils.GenerateCNAME());
            RTP_Session rtpSession = m_pMainUI.CreateRtpSession(m_pRtpMultimediaSession);
            // Port search failed.
            if(rtpSession == null){
                throw new Exception("Calling not possible, RTP session failed to allocate IP end points.");
            }
            
            if(m_pMainUI.IsDebug){
                // We need BeginInvoke here to access UI, we are running on thread pool thread.
                this.BeginInvoke(new MethodInvoker(delegate(){
                    wfrm_RTP_Debug rtpDebug = new wfrm_RTP_Debug(m_pRtpMultimediaSession);
                    rtpDebug.Show();
                }));
            }

            #endregion

            #region Create SDP offer

            SDP_Message sdpOffer = new SDP_Message();
            sdpOffer.Version = "0";
            sdpOffer.Origin = new SDP_Origin("-",sdpOffer.GetHashCode(),1,"IN","IP4",System.Net.Dns.GetHostAddresses("")[0].ToString());
            sdpOffer.SessionName = "SIP Call";            
            sdpOffer.Times.Add(new SDP_Time(0,0));

            #region Add 1 audio stream

            SDP_MediaDescription       mediaStream = new SDP_MediaDescription(SDP_MediaTypes.audio,0,1,"RTP/AVP",null);
            Dictionary<int,AudioCodec> audioCodecs = new Dictionary<int,AudioCodec>(m_pMainUI.AudioCodecs);

            rtpSession.NewReceiveStream += delegate(object s,RTP_ReceiveStreamEventArgs e){
                AudioOut_RTP audioOut = new AudioOut_RTP(m_pAudioOutDevice,e.Stream,audioCodecs);
                audioOut.Start();
                mediaStream.Tags["rtp_audio_out"] = audioOut;
            };

            if(!m_pMainUI.HandleNAT(mediaStream,rtpSession)){
                throw new Exception("Calling not possible, because of NAT or firewall restrictions.");
            }
                        
            foreach(KeyValuePair<int,AudioCodec> entry in audioCodecs){
                mediaStream.Attributes.Add(new SDP_Attribute("rtpmap",entry.Key + " " + entry.Value.Name + "/" + entry.Value.CompressedAudioFormat.SamplesPerSecond));
                mediaStream.MediaFormats.Add(entry.Key.ToString());
            }
            mediaStream.Attributes.Add(new SDP_Attribute("ptime","20"));
            mediaStream.Attributes.Add(new SDP_Attribute("sendrecv",""));
            mediaStream.Tags["rtp_session"]  = rtpSession;
            mediaStream.Tags["audio_codecs"] = audioCodecs;
            sdpOffer.MediaDescriptions.Add(mediaStream);

            #endregion

            m_pLocalSDP = sdpOffer;

            #endregion

            // Create INVITE request.
            SIP_Request invite = m_pUA.Stack.CreateRequest(SIP_Methods.INVITE, to, new SIP_t_NameAddress(account.DisplayName, AbsoluteUri.Parse((sips ? "sips:" : "sip:") + account.AOR)));
            if(account.UseProxy){
                invite.Route.Add("<" + account.ProxyServer + ";lr>");
            }
            invite.ContentType = "application/sdp";
            invite.Data        = sdpOffer.ToByte();

            SIP_UA_Call m_pUACall = m_pUA.CreateCall(invite);
            m_pUACall.Start();
            /*
            m_pInitialInviteSender = m_pStack.CreateRequestSender(invite);
            bool finalResponseSeen = false;
            List<SIP_Dialog_Invite> earlyDialogs = new List<SIP_Dialog_Invite>();            
            m_pInitialInviteSender.ResponseReceived += delegate(object s,SIP_ResponseReceivedEventArgs e){
                // Skip 2xx retransmited response.
                if(finalResponseSeen){
                    return;
                }
                if(e.Response.StatusCode >= 200){
                    finalResponseSeen = true;
                }

                try{
                    #region Provisional

                    if(e.Response.StatusCodeType == SIP_StatusCodeType.Provisional){
                        /* RFC 3261 13.2.2.1.
                            Zero, one or multiple provisional responses may arrive before one or
                            more final responses are received.  Provisional responses for an
                            INVITE request can create "early dialogs".  If a provisional response
                            has a tag in the To field, and if the dialog ID of the response does
                            not match an existing dialog, one is constructed using the procedures
                            defined in Section 12.1.2.
                        
                        if(e.Response.StatusCode > 100 && e.Response.To.Tag != null){
                            earlyDialogs.Add((SIP_Dialog_Invite)e.GetOrCreateDialog);
                        }

                        // 180_Ringing.
                        if(e.Response.StatusCode == 180){
                            m_pPlayer.Play(ResManager.GetStream("ringing.wav"),10);

                            // We need BeginInvoke here to access UI, we are running on thread pool thread.
                            m_pStatusText.BeginInvoke(new MethodInvoker(delegate(){
                                m_pStatusText.Text = "Remote-party is ringing";
                            }));
                        }
                    } 

                    #endregion

                    #region Success

                    else if(e.Response.StatusCodeType == SIP_StatusCodeType.Success){
                        SIP_Dialog dialog = e.GetOrCreateDialog;

                        /* Exit all all other dialogs created by this call (due to forking).
                           That is not defined in RFC but, since UAC can send BYE to early and confirmed dialogs, 
                           all this is 100% valid.
                                                
                        foreach(SIP_Dialog_Invite d in earlyDialogs.ToArray()){
                            if(!d.Equals(dialog)){
                                d.Terminate("Another forking leg accepted.",true);
                            }
                        }

                        m_pDialog = (SIP_Dialog_Invite)dialog;       
                        m_pDialog.StateChanged += new EventHandler(m_pDialog_StateChanged);
                        m_pDialog.RequestReceived += new EventHandler<SIP_RequestReceivedEventArgs>(m_pDialog_RequestReceived);
                
                        SetState(SIP_CallState.Active);
                        
                        // Remote-party provided SDP.
                        if(e.Response.ContentType != null && e.Response.ContentType.ToLower().IndexOf("application/sdp") > -1){
                            try{
                                // SDP offer. We sent offerless INVITE, we need to send SDP answer in ACK request.'
                                if(e.ClientTransaction.Request.ContentType == null || e.ClientTransaction.Request.ContentType.ToLower().IndexOf("application/sdp") == -1){
                                    // Currently we never do it, so it never happens. This is place holder, if we ever support it.
                                }
                                // SDP answer to our offer.
                                else{
                                    ProcessMediaAnswer(e.ClientTransaction,this.LocalSDP,SDP_Message.Parse(Encoding.UTF8.GetString(e.Response.Data)));                                    
                                }
                            }
                            catch{
                                Terminate("SDP answer parsing/processing failed.");
                            }
                        }
                        else{
                            // If we provided SDP offer, there must be SDP answer.
                            if(e.ClientTransaction.Request.ContentType != null && e.ClientTransaction.Request.ContentType.ToLower().IndexOf("application/sdp") > -1){
                                Terminate("Invalid 2xx response, required SDP answer is missing.");
                            }
                        } 
                    }

                    #endregion

                    #region Failure

                    else{
                        /* RFC 3261 13.2.2.3.
                            All early dialogs are considered terminated upon reception of the non-2xx final response.
                        
                        foreach(SIP_Dialog_Invite dialog in earlyDialogs.ToArray()){
                            dialog.Terminate("All early dialogs are considered terminated upon reception of the non-2xx final response. (RFC 3261 13.2.2.3)",false);
                        }
                                                
                        // We need BeginInvoke here to access UI, we are running on thread pool thread.
                        if(this.State != SIP_CallState.Terminating){
                            this.BeginInvoke(new MethodInvoker(delegate(){
                                m_pStatusText.Text = "Calling failed: " + e.Response.StatusCode_ReasonPhrase;
                            }));
                        }

                        // Stop calling or ringing.
                        m_pPlayer.Stop();

                        // Terminate call.
                        Terminate("Remote party rejected a call.",false);
                    }

                    #endregion
                }
                catch(Exception x){
                    Terminate("Error: " + x.Message);
                }
            };
            m_pInitialInviteSender.Completed += new EventHandler(delegate(object s,EventArgs e){
                m_pInitialInviteSender = null;
            
                if(this.State == SIP_CallState.Terminating){
                    SetState(SIP_CallState.Terminated);
                }
            });
            m_pInitialInviteSender.Start();

            // We need BeginInvoke here to access UI, we are running on thread pool thread.
            this.BeginInvoke(new MethodInvoker(delegate(){
                m_pHangup.Enabled = true;
            }));
             * 
             * */
        }
                
        #endregion

        #region method InitIncoming

        /// <summary>
        /// Initializes incoming call.
        /// </summary>
        /// <param name="account">Local account that accepted call.</param>
        /// <param name="transaction">Incomin INVITE server transaction.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>account</b> or <b>transaction</b> is null reference.</exception>
        internal void InitIncoming(Account account,SIP_ServerTransaction transaction)
        {
            if(account == null){
                throw new ArgumentNullException("account");
            }
            if(transaction == null){
                throw new ArgumentNullException("transaction ");
            }

            m_pIncomingInvite = transaction;
            m_CallState       = SIP_CallState.Incoming;

            // Send ringing to remote-party.
            SIP_Response responseRinging = m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x180_Ringing, transaction.Request, transaction.Flow);
            responseRinging.To.Tag = SIP_Utils.CreateTag();
            transaction.SendResponse(responseRinging);

            m_pDialog = (SIP_Dialog_Invite)m_pUA.Stack.TransactionLayer.GetOrCreateDialog(transaction, responseRinging);
            m_pDialog.StateChanged += new EventHandler(m_pDialog_StateChanged);
            
            this.StateChanged += this.m_pCall_StateChanged;
            m_pStatusText.Text = "Incoming call";
            m_pDuration.Text = "00:00:00";
            m_pDisplayName.Text = transaction.Request.To.Address.Uri.ToString();
            m_pToggleHold.Visible = false;
            m_pAccept.Visible = true;
            m_pPlayer.Play(ResManager.GetStream("ringing.wav"),20);
        }

        #endregion


        #region Events handling

        #region method m_pDTMF_Click

        private void m_pDTMF_Click(object sender,EventArgs e)
        {
            wctrl_DTMF dtmf = new wctrl_DTMF(this);

            ToolStripDropDown dropDown = new ToolStripDropDown();            
            ToolStripControlHost container = new ToolStripControlHost(dtmf);
            dropDown.ClientSize = container.Size;
            dropDown.Items.Add(container);

            dropDown.Show(m_pDTMF,new Point(0,m_pDTMF.Height));
        }

        #endregion

        #region method m_pToggleHold_Click

        /// <summary>
        /// Is called when toggle onhold button clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pToggleHold_Click(object sender,EventArgs e)
        {
            try{
                if(!m_IsOnHold){
                    PutCallOnHold();
                    m_pToggleHold.Enabled = false;
                }
                else{
                    PutCallUnHold();
                    m_pToggleHold.Enabled = false;
                }
            }
            catch(Exception x){
                MessageBox.Show("Error: " + x.Message,"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        #endregion

        #region method m_pAccept_Click

        /// <summary>
        /// Is called when "Accept" button clicked.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event data.</param>
        private void m_pAccept_Click(object sender,EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try{
                m_pRtpMultimediaSession = new RTP_MultimediaSession(RTP_Utils.GenerateCNAME());

                // Build local SDP template
                SDP_Message sdpLocal = new SDP_Message();
                sdpLocal.Version = "0";
                sdpLocal.Origin = new SDP_Origin("-",sdpLocal.GetHashCode(),1,"IN","IP4",System.Net.Dns.GetHostAddresses("")[0].ToString());
                sdpLocal.SessionName = "SIP Call";            
                sdpLocal.Times.Add(new SDP_Time(0,0));
                           
                ProcessMediaOffer(m_pIncomingInvite,SDP_Message.Parse(Encoding.UTF8.GetString(m_pIncomingInvite.Request.Data)),sdpLocal);
                                                
                SetState(SIP_CallState.Active);

                if(m_pMainUI.IsDebug){
                    wfrm_RTP_Debug rtpDebug = new wfrm_RTP_Debug(m_pRtpMultimediaSession);
                    rtpDebug.Show();
                }

                m_pIncomingInvite = null;
                m_pToggleHold.Visible = true;
                m_pAccept.Visible = false;
                m_pPlayer.Stop();
            }
            catch(Exception x){
                Terminate("Error: " + x.Message,false);
            }
            this.Cursor = Cursors.Default;
        }

        #endregion

        #region method m_pHangup_Click

        /// <summary>
        /// Is called when hang up button clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pHangup_Click(object sender,EventArgs e)
        {
            Terminate("Hang Up");
        }

        #endregion

                
        #region method m_pCall_StateChanged

        /// <summary>
        /// Is called when call state has changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pCall_StateChanged(object sender,EventArgs e)
        {
            #region Active

            if(this.State == SIP_CallState.Active){
                // Stop ringing.
                m_pPlayer.Stop();

                // We need BeginInvoke here to access UI, we are running on thread pool thread.
                this.BeginInvoke(new MethodInvoker(delegate(){
                    m_StartTime           = DateTime.Now;
                    m_pStatusText.Text    = "Call established";
                    m_pDTMF.Enabled       = true;
                    m_pToggleHold.Enabled = true;
                    m_pToggleHold.Text    = "Hold";

                    // Start ping timer.
                    m_pKeepAliveTimer = new TimerEx(40000);
                    m_pKeepAliveTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_pKeepAliveTimer_Elapsed);
                    m_pKeepAliveTimer.Enabled = true;

                    // Start "duration" updater timer.
                    m_pTimerDuration = new Timer();
                    m_pTimerDuration.Interval = 1000;
                    m_pTimerDuration.Tick += new EventHandler(m_pTimerDuration_Tick);
                    m_pTimerDuration.Enabled = true;
                }));
            }

            #endregion

            #region Terminated

            else if(this.State == SIP_CallState.Terminated){
                if(this.LocalSDP != null){
                    foreach(SDP_MediaDescription media in this.LocalSDP.MediaDescriptions){
                        if(media.Tags.ContainsKey("rtp_audio_in")){
                            ((AudioIn_RTP)media.Tags["rtp_audio_in"]).Dispose();
                        }
                        if(media.Tags.ContainsKey("rtp_audio_out")){
                            ((AudioOut_RTP)media.Tags["rtp_audio_out"]).Dispose();
                        }

                        if(media.Tags.ContainsKey("upnp_rtp_map")){
                            try{
                                m_pMainUI.UPnP.DeletePortMapping((UPnP_NAT_Map)media.Tags["upnp_rtp_map"]);
                            }
                            catch{
                            }
                        }
                        if(media.Tags.ContainsKey("upnp_rtcp_map")){
                            try{
                                m_pMainUI.UPnP.DeletePortMapping((UPnP_NAT_Map)media.Tags["upnp_rtcp_map"]);
                            }
                            catch{
                            }
                        }
                    }
                }

                if(this.RtpMultimediaSession != null){
                    this.RtpMultimediaSession.Dispose();
                }

                if(this.Dialog != null && this.Dialog.IsTerminatedByRemoteParty){
                    m_pPlayer.Play(ResManager.GetStream("hangup.wav"),1);
                }

                // We need BeginInvoke here to access UI, we are running on thread pool thread.
                this.BeginInvoke(new MethodInvoker(delegate(){
                    this.BackColor = Color.WhiteSmoke;
                    m_pDTMF.Enabled       = false;
                    m_pToggleHold.Enabled = false;
                    m_pToggleHold.Text    = "Hold";
                    m_pHangup.Enabled     = false;

                    if(m_pTimerDuration != null){
                        m_pTimerDuration.Dispose();
                    }

                    // Show ended-call UI for 10 seconds, before removed from UI.
                    Timer timerDispose = new Timer();
                    timerDispose.Interval = 10000;
                    timerDispose.Tick += delegate(object s1,EventArgs e1){
                        timerDispose.Dispose();

                        Dispose();
                    };
                    timerDispose.Enabled = true;
                }));
            }

            #endregion
        }
                
        #endregion

        #region method m_pDialog_StateChanged

        /// <summary>
        /// Is called when SIP dialog state has changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pDialog_StateChanged(object sender,EventArgs e)
        {
            if(this.State == SIP_CallState.Disposed || this.State == SIP_CallState.Terminated){
                return;
            }
        
            if(m_pDialog.State == SIP_DialogState.Terminated){
                SetState(SIP_CallState.Terminated); 
            }
        }

        #endregion

        #region method m_pDialog_RequestReceived

        /// <summary>
        /// Is called when dialog receives new SIP request.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pDialog_RequestReceived(object sender,SIP_RequestReceivedEventArgs e)
        {
            // re-INVITE
            if(e.Request.RequestLine.Method == SIP_Methods.INVITE){
                e.IsHandled = true;

                try{                                                        
                    // Remote-party provided SDP offer.
                    if(e.Request.ContentType != null && e.Request.ContentType.ToLower().IndexOf("application/sdp") > -1){
                        ProcessMediaOffer(e.ServerTransaction,SDP_Message.Parse(Encoding.UTF8.GetString(e.Request.Data)),m_pLocalSDP);

                        // We are holding a call.
                        if(this.IsLocalOnHold){
                            // We don't need to do anything here.
                        }
                        // Remote-party is holding a call.
                        else if(IsRemotePartyHolding(SDP_Message.Parse(Encoding.UTF8.GetString(e.Request.Data)))){
                            // We need BeginInvoke here to access UI, we are running on thread pool thread.
                            this.BeginInvoke(new MethodInvoker(delegate(){
                                m_pStatusText.Text = "Remote party holding a call";                                    
                            }));

                            // Play on-hold music.
                            m_pPlayer.Play(ResManager.GetStream("onhold.wav"),20);
                         }
                         // Call is active.
                         else{
                            // We need BeginInvoke here to access UI, we are running on thread pool thread.
                            this.BeginInvoke(new MethodInvoker(delegate(){
                                m_pStatusText.Text = "Call established";                                    
                            }));

                            // Stop on-hold music.
                            m_pPlayer.Stop();
                        }
                    }
                    // Error: Re-INVITE can't be SDP offerless.
                    else{
                        e.ServerTransaction.SendResponse(m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": Re-INVITE must contain SDP offer.", e.Request));
                    }
                }
                catch(Exception x1){
                    e.ServerTransaction.SendResponse(m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": " + x1.Message, e.Request));
                }
            }
        }

        #endregion

        #region method m_pTimerDuration_Tick

        /// <summary>
        /// Is called when "duration" timer triggers.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pTimerDuration_Tick(object sender,EventArgs e)
        {
            try{
                if(this.State == SIP_CallState.Active){
                    TimeSpan duration = (DateTime.Now - this.StartTime);
                    m_pDuration.Text = duration.Hours.ToString("00") + ":" + duration.Minutes.ToString("00") + ":" + duration.Seconds.ToString("00");
                }
            }
            catch{
                // We don't care about errors here.
            }
        }

        #endregion

        #region method m_pKeepAliveTimer_Elapsed

        /// <summary>
        /// Is called when ping timer triggers.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pKeepAliveTimer_Elapsed(object sender,System.Timers.ElapsedEventArgs e)
        {
            try{
                // Send ping request if any flow using object has not sent ping within 30 seconds.
                if(m_pDialog.Flow.LastPing.AddSeconds(30) < DateTime.Now){
                    m_pDialog.Flow.SendPing();
                }
            }
            catch{
            }
        }

        #endregion

        #endregion


        #region method Terminate
                
        /// <summary>
        /// Terminates call.
        /// </summary>
        /// <param name="reason">Call termination reason. This text is sent to remote-party.</param>
        public void Terminate(string reason)
        {
            Terminate(reason,true);
        }

        /// <summary>
        /// Terminates a call.
        /// </summary>
        /// <param name="reason">Call termination reason. This text is sent to remote-party.</param>
        /// <param name="sendBye">If true BYE request with <b>reason</b> text is sent remote-party.</param>
        public void Terminate(string reason,bool sendBye)
        {        
            lock(m_pLock){
                if(this.State == SIP_CallState.Disposed){
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                if(this.State == SIP_CallState.Terminating || this.State == SIP_CallState.Terminated){
                    return;
                }
                else if(this.State == SIP_CallState.Active){
                    SetState(SIP_CallState.Terminating);
                    
                    m_pDialog.Terminate(reason,sendBye);

                    // We need BeginInvoke here to access UI, we are running on thread pool thread.
                    this.BeginInvoke(new MethodInvoker(delegate(){
                        m_pStatusText.Text = reason;
                    }));
                }                
                else if(this.State == SIP_CallState.Calling && m_pInitialInviteSender != null){
                    /* RFC 3261 15.
                        If we are caller and call is not active yet, we must do following actions:
                            *) Send CANCEL, set call Terminating flag.
                            *) If we get non 2xx final response, we are done. (Normally cancel causes '408 Request terminated')
                            *) If we get 2xx response (2xx sent by remote party before our CANCEL reached), we must send BYE to active dialog.
                    */

                    SetState(SIP_CallState.Terminating);

                    m_pInitialInviteSender.Cancel();                    
                }
                else if(this.State == SIP_CallState.Incoming){
                    m_pIncomingInvite.SendResponse(m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x600_Busy_Everywhere, m_pIncomingInvite.Request));

                    SetState(SIP_CallState.Terminated);
                }
            }
        }

        #endregion

        #region method PutCallOnHold

        /// <summary>
        /// Puts call on hold.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is called when this method is called and call is not in valid state.</exception>
        public void PutCallOnHold()
        {   
            if(this.State != SIP_CallState.Active){
                throw new InvalidOperationException("Method 'PutCallOnHold' is valid only in call 'Active' state.");
            }
            if(m_IsOnHold){
                return;
            }
        
            // Get copy of SDP.            
            SDP_Message onHoldOffer = this.LocalSDP.Clone();
            // Each time we modify SDP message we need to increase session version.
            onHoldOffer.Origin.SessionVersion++;

            // Mark all active enabled media streams inactive.
            foreach(SDP_MediaDescription m in onHoldOffer.MediaDescriptions){
                if(m.Port != 0){
                    m.SetStreamMode("inactive");
                }
            }

            // Create INVITE request.
            SIP_Request invite = this.Dialog.CreateRequest(SIP_Methods.INVITE);
            invite.ContentType = "application/sdp";
            invite.Data        = onHoldOffer.ToByte();

            bool finalResponseSeen = false;
            SIP_RequestSender sender = this.Dialog.CreateRequestSender(invite);
            sender.ResponseReceived += delegate(object s,SIP_ResponseReceivedEventArgs e){
                // Skip 2xx retransmited response.
                if(finalResponseSeen){
                    return;
                }
                if(e.Response.StatusCode >= 200){
                    finalResponseSeen = true;
                }

                try{
                    #region Provisional

                    if(e.Response.StatusCodeType == SIP_StatusCodeType.Provisional){
                        // We don't care provisional responses here.
                    }

                    #endregion
                 
                    #region Success

                    else if(e.Response.StatusCodeType == SIP_StatusCodeType.Success){
                        // Remote-party provided SDP answer.
                        if(e.Response.ContentType != null && e.Response.ContentType.ToLower().IndexOf("application/sdp") > -1){
                            try{
                                ProcessMediaAnswer(e.ClientTransaction,onHoldOffer,SDP_Message.Parse(Encoding.UTF8.GetString(e.Response.Data)));
                            }
                            catch{
                                Terminate("SDP answer parsing failed.");
                            }

                            // Call terminated.
                            if(this.State != SIP_CallState.Active){
                                return;
                            }

                            m_IsOnHold = true;

                            // We need BeginInvoke here to access UI, we are running on thread pool thread.
                            this.BeginInvoke(new MethodInvoker(delegate(){                                
                                this.BackColor = Color.WhiteSmoke;
                                m_pToggleHold.Enabled = true;
                                m_pToggleHold.Text = "Unhold";                                

                                m_pStatusText.Text = "We are holding a call";
                            }));
                        }
                    }

                    #endregion

                    #region Failure

                    else{
                        // We need BeginInvoke here to access UI, we are running on thread pool thread.
                        this.BeginInvoke(new MethodInvoker(delegate(){
                            MessageBox.Show("Re-INVITE Error: " + e.Response.StatusCode_ReasonPhrase,"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                            m_pToggleHold.Enabled = true;
                        }));

                        // 481 Call Transaction Does Not Exist.
                        if(e.Response.StatusCode == 481){
                            Terminate("Remote-party call does not exist any more.",false);
                        }
                    }

                    #endregion
                }
                catch(Exception x){
                    // We need BeginInvoke here to access UI, we are running on thread pool thread.
                    this.BeginInvoke(new MethodInvoker(delegate(){
                        MessageBox.Show("Error: " + x.Message,"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    }));
                }
            };
            sender.Start();
        }

        #endregion

        #region method PutCallUnHold

        /// <summary>
        /// Takes call off on hold.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is called when this method is called and call is not in valid state.</exception>
        private void PutCallUnHold()
        {
            if(this.State != SIP_CallState.Active){
                throw new InvalidOperationException("Method 'PutCallUnHold' is valid only in call 'Active' state.");
            }
            if(!m_IsOnHold){
                return;
            }

            // Put all calls on-hold.
            m_pMainUI.PutAllCallsOnHold();

            // Get copy of SDP.            
            SDP_Message onHoldOffer = this.LocalSDP.Clone();
            // Each time we modify SDP message we need to increase session version.
            onHoldOffer.Origin.SessionVersion++;

            // Mark all active enabled media streams sendrecv.
            foreach(SDP_MediaDescription m in onHoldOffer.MediaDescriptions){
                if(m.Port != 0){
                    m.SetStreamMode("sendrecv");
                }
            }

            // Create INVITE request.
            SIP_Request invite = this.Dialog.CreateRequest(SIP_Methods.INVITE);
            invite.ContentType = "application/sdp";
            invite.Data        = onHoldOffer.ToByte();

            bool finalResponseSeen = false;
            SIP_RequestSender sender = this.Dialog.CreateRequestSender(invite);
            sender.ResponseReceived += delegate(object s,SIP_ResponseReceivedEventArgs e){
                // Skip 2xx retransmited response.
                if(finalResponseSeen){
                    return;
                }
                if(e.Response.StatusCode >= 200){
                    finalResponseSeen = true;
                }

                try{
                    #region Provisional

                    if(e.Response.StatusCodeType == SIP_StatusCodeType.Provisional){
                        // We don't care provisional responses here.
                    }

                    #endregion
                 
                    #region Success

                    else if(e.Response.StatusCodeType == SIP_StatusCodeType.Success){
                        // Remote-party provided SDP answer.
                        if(e.Response.ContentType != null && e.Response.ContentType.ToLower().IndexOf("application/sdp") > -1){
                            try{
                                SDP_Message sdpAnswer = SDP_Message.Parse(Encoding.UTF8.GetString(e.Response.Data));

                                ProcessMediaAnswer(e.ClientTransaction,onHoldOffer,sdpAnswer);

                                // Call terminated.
                                if(this.State != SIP_CallState.Active){
                                    return;
                                }

                                m_IsOnHold = false;

                                // We need BeginInvoke here to access UI, we are running on thread pool thread.
                                this.BeginInvoke(new MethodInvoker(delegate(){                                    
                                    this.BackColor = Color.FromArgb(213,237,249);
                                    m_pToggleHold.Enabled = true;
                                    m_pToggleHold.Text = "Hold";

                                    if(IsRemotePartyHolding(sdpAnswer)){
                                        m_pStatusText.Text = "Remote party holding a call";

                                        // Play onhold music.
                                        m_pPlayer.Play(ResManager.GetStream("onhold.wav"),20);
                                    }
                                    else{
                                        m_pStatusText.Text = "Call established";
                                    }
                                }));
                            }
                            catch{
                                Terminate("SDP answer parsing failed.");
                            }
                        }
                    }

                    #endregion

                    #region Failure

                    else{
                        // We need BeginInvoke here to access UI, we are running on thread pool thread.
                        this.BeginInvoke(new MethodInvoker(delegate(){
                            MessageBox.Show("Re-INVITE Error: " + e.Response.StatusCode_ReasonPhrase,"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                            m_pToggleHold.Enabled = true;
                        }));

                        // 481 Call Transaction Does Not Exist.
                        if(e.Response.StatusCode == 481){
                            Terminate("Remote-party call does not exist any more.",false);
                        }
                    }

                    #endregion
                }
                catch(Exception x){
                    // We need BeginInvoke here to access UI, we are running on thread pool thread.
                    this.BeginInvoke(new MethodInvoker(delegate(){
                        MessageBox.Show("Error: " + x.Message,"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    }));
                }
            };
            sender.Start();
        }

        #endregion

        #region method SendDTMF

        /// <summary>
        /// Sends DTMF to remote party.
        /// </summary>
        /// <param name="eventNo">DTMF event number.</param>
        /// <param name="duration">Duration in millisecond.</param>
        /// <exception cref="InvalidOperationException">Is called when this method is called and call is not in valid state.</exception>
        public void SendDTMF(int eventNo,int duration)
        {
            if(this.State != SIP_CallState.Active){
                throw new InvalidOperationException("Method 'SendDTMF' is valid only in call 'Active' state.");
            }

            /* SIP INFO DTMF example.
                [Other-Message-Fields]
                Content-Type: application/dtmf-relay 

                Signal=5 
                Duration=160 
            */

            SIP_Request info = m_pDialog.CreateRequest(SIP_Methods.INFO);
            info.ContentType = "application/dtmf-relay";
            info.Data = Encoding.ASCII.GetBytes("Signal=" + eventNo + "\r\n" + "Duration=" + duration + "\r\n");

            SIP_RequestSender infoSender = m_pDialog.CreateRequestSender(info);
            infoSender.Start();
        }

        #endregion


        #region method SetState

        /// <summary>
        /// Set call state.
        /// </summary>
        /// <param name="state">New call state.</param>
        private void SetState(SIP_CallState state)
        {
            // Disposed call may not change state.
            if(this.State == SIP_CallState.Disposed){
                return;
            }

            m_CallState = state;
                        
            OnStateChanged(state);
        }

        #endregion

        #region method ProcessMediaOffer

        /// <summary>
        /// Processes media offer.
        /// </summary>
        /// <param name="transaction">Server transaction</param>
        /// <param name="offer">Remote-party SDP offer.</param>
        /// <param name="localSDP">Current local SDP.</param>
        /// <exception cref="ArgumentNullException">Is raised <b>transaction</b>,<b>offer</b> or <b>localSDP</b> is null reference.</exception>
        private void ProcessMediaOffer(SIP_ServerTransaction transaction,SDP_Message offer,SDP_Message localSDP)
        {    
            if(transaction == null){
                throw new ArgumentNullException("transaction");
            }
            if(offer == null){
                throw new ArgumentNullException("offer");
            }
            if(localSDP == null){
                throw new ArgumentNullException("localSDP");
            }
                        
            try{ 
                #region SDP basic validation

                // Version field must exist.
                if(offer.Version == null){
                    transaction.SendResponse(m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": Invalid SDP answer: Required 'v'(Protocol Version) field is missing.",transaction.Request));

                    return;
                }

                // Origin field must exist.
                if(offer.Origin == null){
                    transaction.SendResponse(m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": Invalid SDP answer: Required 'o'(Origin) field is missing.",transaction.Request));

                    return;
                }

                // Session Name field.

                // Check That global 'c' connection attribute exists or otherwise each enabled media stream must contain one.
                if(offer.Connection == null){
                    for(int i=0;i<offer.MediaDescriptions.Count;i++){
                        if(offer.MediaDescriptions[i].Connection == null){
                            transaction.SendResponse(m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": Invalid SDP answer: Global or per media stream no: " + i + " 'c'(Connection) attribute is missing.",transaction.Request));

                            return;
                        }
                    }
                }

                #endregion
                                                            
                // Re-INVITE media streams count must be >= current SDP streams.
                if(localSDP.MediaDescriptions.Count > offer.MediaDescriptions.Count){
                    transaction.SendResponse(m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": re-INVITE SDP offer media stream count must be >= current session stream count.",transaction.Request));

                    return;
                }

                bool audioAccepted = false;
                // Process media streams info.
                for(int i=0;i<offer.MediaDescriptions.Count;i++){
                    SDP_MediaDescription offerMedia  = offer.MediaDescriptions[i];
                    SDP_MediaDescription answerMedia = (localSDP.MediaDescriptions.Count > i ? localSDP.MediaDescriptions[i] : null);

                    // Disabled stream.
                    if(offerMedia.Port == 0){
                        // Remote-party offered new disabled stream.
                        if(answerMedia == null){
                            // Just copy offer media stream data to answer and set port to zero.
                            localSDP.MediaDescriptions.Add(offerMedia);
                            localSDP.MediaDescriptions[i].Port = 0;
                        }
                        // Existing disabled stream or remote party disabled it.
                        else{
                            answerMedia.Port = 0;

                            #region Cleanup active RTP stream and it's resources, if it exists

                            // Dispose existing RTP session.
                            if(answerMedia.Tags.ContainsKey("rtp_session")){                            
                                ((RTP_Session)offerMedia.Tags["rtp_session"]).Dispose();
                                answerMedia.Tags.Remove("rtp_session");
                            }

                            // Release UPnPports if any.
                            if(answerMedia.Tags.ContainsKey("upnp_rtp_map")){
                                try{
                                    m_pMainUI.UPnP.DeletePortMapping((UPnP_NAT_Map)answerMedia.Tags["upnp_rtp_map"]);
                                }
                                catch{
                                }
                                answerMedia.Tags.Remove("upnp_rtp_map");
                            }
                            if(answerMedia.Tags.ContainsKey("upnp_rtcp_map")){
                                try{
                                    m_pMainUI.UPnP.DeletePortMapping((UPnP_NAT_Map)answerMedia.Tags["upnp_rtcp_map"]);
                                }
                                catch{
                                }
                                answerMedia.Tags.Remove("upnp_rtcp_map");
                            }

                            #endregion
                        }
                    }
                    // Remote-party wants to communicate with this stream.
                    else{
                        // See if we can support this stream.
                        if(!audioAccepted && m_pMainUI.CanSupportMedia(offerMedia)){
                            // New stream.
                            if(answerMedia == null){
                                answerMedia = new SDP_MediaDescription(SDP_MediaTypes.audio,0,2,"RTP/AVP",null);
                                localSDP.MediaDescriptions.Add(answerMedia);
                            }
                            
                            #region Build audio codec map with codecs which we support
   
                            Dictionary<int,AudioCodec> audioCodecs = m_pMainUI.GetOurSupportedAudioCodecs(offerMedia);
                            answerMedia.MediaFormats.Clear();
                            answerMedia.Attributes.Clear();
                            foreach(KeyValuePair<int,AudioCodec> entry in audioCodecs){
                                answerMedia.Attributes.Add(new SDP_Attribute("rtpmap",entry.Key + " " + entry.Value.Name + "/" + entry.Value.CompressedAudioFormat.SamplesPerSecond));
                                answerMedia.MediaFormats.Add(entry.Key.ToString());
                            }
                            answerMedia.Attributes.Add(new SDP_Attribute("ptime","20"));
                            answerMedia.Tags["audio_codecs"] = audioCodecs;

                            #endregion

                            #region Create/modify RTP session

                            // RTP session doesn't exist, create it.
                            if(!answerMedia.Tags.ContainsKey("rtp_session")){
                                RTP_Session rtpSess = m_pMainUI.CreateRtpSession(m_pRtpMultimediaSession);
                                // RTP session creation failed,disable this stream.
                                if(rtpSess == null){                                    
                                    answerMedia.Port = 0;

                                    break;
                                }
                                answerMedia.Tags.Add("rtp_session",rtpSess);

                                rtpSess.NewReceiveStream += delegate(object s,RTP_ReceiveStreamEventArgs e){
                                    if(answerMedia.Tags.ContainsKey("rtp_audio_out")){
                                        ((AudioOut_RTP)answerMedia.Tags["rtp_audio_out"]).Dispose();
                                    }
            
                                    AudioOut_RTP audioOut = new AudioOut_RTP(m_pAudioOutDevice,e.Stream,audioCodecs);
                                    audioOut.Start();
                                    answerMedia.Tags["rtp_audio_out"] = audioOut;
                                };

                                // NAT
                                if(!m_pMainUI.HandleNAT(answerMedia,rtpSess)){
                                    // NAT handling failed,disable this stream.
                                    answerMedia.Port = 0;

                                    break;
                                }                                
                            }
                                                        
                            RTP_StreamMode offerStreamMode = GetRtpStreamMode(offer,offerMedia);
                            if(offerStreamMode == RTP_StreamMode.Inactive){
                                answerMedia.SetStreamMode("inactive");
                            }
                            else if(offerStreamMode == RTP_StreamMode.Receive){
                                answerMedia.SetStreamMode("sendonly");
                            }
                            else if(offerStreamMode == RTP_StreamMode.Send){
                                if(m_IsOnHold){
                                    answerMedia.SetStreamMode("inactive");
                                }
                                else{
                                    answerMedia.SetStreamMode("recvonly");
                                }
                            }
                            else if(offerStreamMode == RTP_StreamMode.SendReceive){
                                if(m_IsOnHold){
                                    answerMedia.SetStreamMode("inactive");
                                }
                                else{
                                    answerMedia.SetStreamMode("sendrecv");
                                }
                            }
                            
                            RTP_Session rtpSession = (RTP_Session)answerMedia.Tags["rtp_session"];                                              
                            rtpSession.Payload = Convert.ToInt32(answerMedia.MediaFormats[0]);
                            rtpSession.StreamMode = GetRtpStreamMode(localSDP,answerMedia);
                            rtpSession.RemoveTargets();
                            if(GetSdpHost(offer,offerMedia) != "0.0.0.0"){
                                rtpSession.AddTarget(GetRtpTarget(offer,offerMedia));
                            }
                            rtpSession.Start();

                            #endregion

                            #region Create/modify audio-in source
                                                    
                            if(!answerMedia.Tags.ContainsKey("rtp_audio_in")){
                                AudioIn_RTP rtpAudioIn = new AudioIn_RTP(m_pAudioInDevice,20,audioCodecs,rtpSession.CreateSendStream());                        
                                rtpAudioIn.Start();
                                answerMedia.Tags.Add("rtp_audio_in",rtpAudioIn);
                            }
                            else{
                                ((AudioIn_RTP)answerMedia.Tags["rtp_audio_in"]).AudioCodecs = audioCodecs;
                            }
                            
                            #endregion

                            audioAccepted = true;
                        }
                        // We don't accept this stream, so disable it.
                        else{                            
                            // Just copy offer media stream data to answer and set port to zero.

                            // Delete exisiting media stream.
                            if(answerMedia != null){
                                localSDP.MediaDescriptions.RemoveAt(i);
                            }
                            localSDP.MediaDescriptions.Add(offerMedia);
                            localSDP.MediaDescriptions[i].Port = 0;
                        }
                    }
                }

                m_pLocalSDP  = localSDP;
                m_pRemoteSDP = offer;

                #region Create and send 2xx response
            
                SIP_Response response = m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x200_Ok,transaction.Request,transaction.Flow);
                //response.Contact = SIP stack will allocate it as needed;
                response.ContentType = "application/sdp";
                response.Data = localSDP.ToByte();

                transaction.SendResponse(response);
                  
                // Start retransmitting 2xx response, while ACK receives.
                Handle2xx(transaction);
                                
                #endregion
            }
            catch(Exception x){
                transaction.SendResponse(m_pUA.Stack.CreateResponse(SIP_ResponseCodes.x500_Server_Internal_Error + ": " + x.Message,transaction.Request));
            }
        }

        #endregion

        #region method ProcessMediaAnswer

        /// <summary>
        /// Processes media answer.
        /// </summary>
        /// <param name="transaction">INVITE client transaction.</param>
        /// <param name="offer">SDP media offer.</param>
        /// <param name="answer">SDP remote-party meida answer.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>transaction</b>,<b>offer</b> or <b>answer</b> is null reference.</exception>
        private void ProcessMediaAnswer(SIP_ClientTransaction transaction,SDP_Message offer,SDP_Message answer)
        {
            if(transaction == null){
                throw new ArgumentNullException("transaction");
            }
            if(offer == null){
                throw new ArgumentNullException("offer");
            }
            if(answer == null){
                throw new ArgumentNullException("answer");
            }

            try{
                // This method takes care of ACK sending and 2xx response retransmission ACK sending.
                HandleAck(m_pDialog,transaction);

                #region SDP basic validation

                // Version field must exist.
                if(offer.Version == null){
                    Terminate("Invalid SDP answer: Required 'v'(Protocol Version) field is missing.");

                    return;
                }

                // Origin field must exist.
                if(offer.Origin == null){
                    Terminate("Invalid SDP answer: Required 'o'(Origin) field is missing.");

                    return;
                }

                // Session Name field.

                // Check That global 'c' connection attribute exists or otherwise each enabled media stream must contain one.
                if(offer.Connection == null){
                    for(int i=0;i<offer.MediaDescriptions.Count;i++){
                        if(offer.MediaDescriptions[i].Connection == null){
                            Terminate("Invalid SDP answer: Global or per media stream no: " + i + " 'c'(Connection) attribute is missing.");

                            return;
                        }
                    }
                }


                // Check media streams count.
                if(offer.MediaDescriptions.Count != answer.MediaDescriptions.Count){
                    Terminate("Invalid SDP answer, media descriptions count in answer must be equal to count in media offer (RFC 3264 6.).");

                    return;
                }

                #endregion
                                                                                
                // Process media streams info.
                for(int i=0;i<offer.MediaDescriptions.Count;i++){
                    SDP_MediaDescription offerMedia  = offer.MediaDescriptions[i];
                    SDP_MediaDescription answerMedia = answer.MediaDescriptions[i];
                    
                    // Remote-party disabled this stream.
                    if(answerMedia.Port == 0){

                        #region Cleanup active RTP stream and it's resources, if it exists

                        // Dispose existing RTP session.
                        if(offerMedia.Tags.ContainsKey("rtp_session")){                            
                            ((RTP_Session)offerMedia.Tags["rtp_session"]).Dispose();
                            offerMedia.Tags.Remove("rtp_session");
                        }

                        // Release UPnPports if any.
                        if(offerMedia.Tags.ContainsKey("upnp_rtp_map")){
                            try{
                                m_pMainUI.UPnP.DeletePortMapping((UPnP_NAT_Map)offerMedia.Tags["upnp_rtp_map"]);
                            }
                            catch{
                            }
                            offerMedia.Tags.Remove("upnp_rtp_map");
                        }
                        if(offerMedia.Tags.ContainsKey("upnp_rtcp_map")){
                            try{
                                m_pMainUI.UPnP.DeletePortMapping((UPnP_NAT_Map)offerMedia.Tags["upnp_rtcp_map"]);
                            }
                            catch{
                            }
                            offerMedia.Tags.Remove("upnp_rtcp_map");
                        }

                        #endregion
                    }
                    // Remote-party accepted stream.
                    else{
                        Dictionary<int,AudioCodec> audioCodecs = (Dictionary<int,AudioCodec>)offerMedia.Tags["audio_codecs"];

                        #region Validate stream-mode disabled,inactive,sendonly,recvonly

                        /* RFC 3264 6.1.
                            If a stream is offered as sendonly, the corresponding stream MUST be
                            marked as recvonly or inactive in the answer.  If a media stream is
                            listed as recvonly in the offer, the answer MUST be marked as
                            sendonly or inactive in the answer.  If an offered media stream is
                            listed as sendrecv (or if there is no direction attribute at the
                            media or session level, in which case the stream is sendrecv by
                            default), the corresponding stream in the answer MAY be marked as
                            sendonly, recvonly, sendrecv, or inactive.  If an offered media
                            stream is listed as inactive, it MUST be marked as inactive in the
                            answer.
                        */

                        // If we disabled this stream in offer and answer enables it (no allowed), terminate call.
                        if(offerMedia.Port == 0){
                            Terminate("Invalid SDP answer, you may not enable sdp-offer disabled stream no: " + i + " (RFC 3264 6.).");

                            return;
                        }

                        RTP_StreamMode offerStreamMode  = GetRtpStreamMode(offer,offerMedia);
                        RTP_StreamMode answerStreamMode = GetRtpStreamMode(answer,answerMedia);                                                
                        if(offerStreamMode == RTP_StreamMode.Send && answerStreamMode != RTP_StreamMode.Receive){
                            Terminate("Invalid SDP answer, sdp stream no: " + i + " stream-mode must be 'recvonly' (RFC 3264 6.).");

                            return;
                        }
                        if(offerStreamMode == RTP_StreamMode.Receive && answerStreamMode != RTP_StreamMode.Send){
                            Terminate("Invalid SDP answer, sdp stream no: " + i + " stream-mode must be 'sendonly' (RFC 3264 6.).");

                            return;
                        }
                        if(offerStreamMode == RTP_StreamMode.Inactive && answerStreamMode != RTP_StreamMode.Inactive){
                            Terminate("Invalid SDP answer, sdp stream no: " + i + " stream-mode must be 'inactive' (RFC 3264 6.).");

                            return;
                        }

                        #endregion

                        #region Create/modify RTP session
                                                
                        RTP_Session rtpSession = (RTP_Session)offerMedia.Tags["rtp_session"];
                        rtpSession.Payload = Convert.ToInt32(answerMedia.MediaFormats[0]);
                        rtpSession.StreamMode = (answerStreamMode == RTP_StreamMode.Inactive ? RTP_StreamMode.Inactive : offerStreamMode);                        
                        rtpSession.RemoveTargets();
                        if(GetSdpHost(answer,answerMedia) != "0.0.0.0"){
                            rtpSession.AddTarget(GetRtpTarget(answer,answerMedia));
                        }
                        rtpSession.Start();

                        #endregion

                        #region Create/modify audio-in source

                        if(!offerMedia.Tags.ContainsKey("rtp_audio_in")){
                            AudioIn_RTP rtpAudioIn = new AudioIn_RTP(m_pAudioInDevice,20,audioCodecs,rtpSession.CreateSendStream());                        
                            rtpAudioIn.Start();
                            offerMedia.Tags.Add("rtp_audio_in",rtpAudioIn);
                        }
                        
                        #endregion
                    }
                }

                m_pLocalSDP  = offer;
                m_pRemoteSDP = answer;                
            }
            catch(Exception x){
                Terminate("Error processing SDP answer: " + x.Message);
            }
        }

        #endregion

        #region method GetRtpStreamMode

        /// <summary>
        /// Gets RTP stream mode.
        /// </summary>
        /// <param name="sdp">SDP message.</param>
        /// <param name="media">SDP media.</param>
        /// <returns>Returns RTP stream mode.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>sdp</b> or <b>media</b> is null reference.</exception>
        private RTP_StreamMode GetRtpStreamMode(SDP_Message sdp,SDP_MediaDescription media)
        {
            if(sdp == null){
                throw new ArgumentNullException("sdp");
            }
            if(media == null){
                throw new ArgumentNullException("media");
            }

            // Try to get per media stream mode.
            foreach(SDP_Attribute a in media.Attributes){
                if(string.Equals(a.Name,"sendrecv",StringComparison.InvariantCultureIgnoreCase)){
                    return RTP_StreamMode.SendReceive;
                }
                else if(string.Equals(a.Name,"sendonly",StringComparison.InvariantCultureIgnoreCase)){
                    return RTP_StreamMode.Send;
                }
                else if(string.Equals(a.Name,"recvonly",StringComparison.InvariantCultureIgnoreCase)){
                    return RTP_StreamMode.Receive;
                }
                else if(string.Equals(a.Name,"inactive",StringComparison.InvariantCultureIgnoreCase)){
                    return RTP_StreamMode.Inactive;
                }
            }

            // No per media stream mode, try to get per session stream mode.
            foreach(SDP_Attribute a in sdp.Attributes){
                if(string.Equals(a.Name,"sendrecv",StringComparison.InvariantCultureIgnoreCase)){
                    return RTP_StreamMode.SendReceive;
                }
                else if(string.Equals(a.Name,"sendonly",StringComparison.InvariantCultureIgnoreCase)){
                    return RTP_StreamMode.Send;
                }
                else if(string.Equals(a.Name,"recvonly",StringComparison.InvariantCultureIgnoreCase)){
                    return RTP_StreamMode.Receive;
                }
                else if(string.Equals(a.Name,"inactive",StringComparison.InvariantCultureIgnoreCase)){
                    return RTP_StreamMode.Inactive;
                }
            }

            return RTP_StreamMode.SendReceive;
        }

        #endregion

        #region method GetSdpHost

        /// <summary>
        /// Gets SDP per media or global connection host.
        /// </summary>
        /// <param name="sdp">SDP message.</param>
        /// <param name="mediaStream">SDP media stream.</param>
        /// <returns>Returns SDP per media or global connection host.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>sdp</b> or <b>mediaStream</b> is null reference.</exception>
        private string GetSdpHost(SDP_Message sdp,SDP_MediaDescription mediaStream)
        {
            if(sdp == null){
                throw new ArgumentNullException("sdp");
            }
            if(mediaStream == null){
                throw new ArgumentNullException("mediaStream");
            }

            // We must have SDP global or per media connection info.
            string host = mediaStream.Connection != null ? mediaStream.Connection.Address : null;
            if(host == null){
                host = sdp.Connection.Address != null ? sdp.Connection.Address : null;

                if(host == null){
                    throw new ArgumentException("Invalid SDP message, global or per media 'c'(Connection) attribute is missing.");
                }
            }

            return host;
        }

        #endregion

        #region method GetRtpTarget

        /// <summary>
        /// Gets RTP target for SDP media stream.
        /// </summary>
        /// <param name="sdp">SDP message.</param>
        /// <param name="mediaStream">SDP media stream.</param>
        /// <returns>Return RTP target.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>sdp</b> or <b>mediaStream</b> is null reference.</exception>
        private RTP_Address GetRtpTarget(SDP_Message sdp,SDP_MediaDescription mediaStream)
        {
            if(sdp == null){
                throw new ArgumentNullException("sdp");
            }
            if(mediaStream == null){
                throw new ArgumentNullException("mediaStream");
            }

            // We must have SDP global or per media connection info.
            string host = mediaStream.Connection != null ? mediaStream.Connection.Address : null;
            if(host == null){
                host = sdp.Connection.Address != null ? sdp.Connection.Address : null;

                if(host == null){
                    throw new ArgumentException("Invalid SDP message, global or per media 'c'(Connection) attribute is missing.");
                }
            }

            int remoteRtcpPort = mediaStream.Port + 1;
            // Use specified RTCP port, if specified.
            foreach(SDP_Attribute attribute in mediaStream.Attributes){
                if(string.Equals(attribute.Name,"rtcp",StringComparison.InvariantCultureIgnoreCase)){
                    remoteRtcpPort = Convert.ToInt32(attribute.Value);

                    break;
                }
            }

            return new RTP_Address(System.Net.Dns.GetHostAddresses(host)[0],mediaStream.Port,remoteRtcpPort);
        }

        #endregion

        #region method IsRemotePartyHolding

        /// <summary>
        /// Checks if remote-party is holding audio.
        /// </summary>
        /// <param name="sdp">Remote-party SDP offer/answer.</param>
        /// <returns>Returns true is remote-party is holding audio, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>sdp</b> is null reference.</exception>
        private bool IsRemotePartyHolding(SDP_Message sdp)
        {
            if(sdp == null){
                throw new ArgumentNullException("sdp");
            }

            // Check if first audio stream is SendRecv, otherwise remote-party holding audio.
            foreach(SDP_MediaDescription media in sdp.MediaDescriptions){
                if(media.Port != 0 && media.MediaType == "audio"){
                    if(GetRtpStreamMode(sdp,media) != RTP_StreamMode.SendReceive){
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        #endregion

        #region method HandleAck

        /// <summary>
        /// This method takes care of ACK sending and 2xx response retransmission ACK sending.
        /// </summary>
        /// <param name="dialog">SIP dialog.</param>
        /// <param name="transaction">SIP client transaction.</param>
        private void HandleAck(SIP_Dialog dialog,SIP_ClientTransaction transaction)
        {
            if(dialog == null){
                throw new ArgumentNullException("dialog");
            }
            if(transaction == null){
                throw new ArgumentNullException("transaction");
            }
            
            /* RFC 3261 6.
                The ACK for a 2xx response to an INVITE request is a separate transaction.
              
               RFC 3261 13.2.2.4.
                The UAC core MUST generate an ACK request for each 2xx received from
                the transaction layer.  The header fields of the ACK are constructed
                in the same way as for any request sent within a dialog (see Section
                12) with the exception of the CSeq and the header fields related to
                authentication.  The sequence number of the CSeq header field MUST be
                the same as the INVITE being acknowledged, but the CSeq method MUST
                be ACK.  The ACK MUST contain the same credentials as the INVITE.  If
                the 2xx contains an offer (based on the rules above), the ACK MUST
                carry an answer in its body.
            */

            SIP_t_ViaParm via = new SIP_t_ViaParm();
            via.ProtocolName = "SIP";
            via.ProtocolVersion = "2.0";
            via.ProtocolTransport = transaction.Flow.Transport;
            via.SentBy = new HostEndPoint(transaction.Flow.LocalEP);
            via.Branch = SIP_t_ViaParm.CreateBranch();
            via.RPort = 0;

            SIP_Request ackRequest = dialog.CreateRequest(SIP_Methods.ACK);
            ackRequest.Via.AddToTop(via.ToStringValue());
            ackRequest.CSeq = new SIP_t_CSeq(transaction.Request.CSeq.SequenceNumber,SIP_Methods.ACK);
            // Authorization
            foreach(SIP_HeaderField h in transaction.Request.Authorization.HeaderFields){
                ackRequest.Authorization.Add(h.Value);
            }
            // Proxy-Authorization 
            foreach(SIP_HeaderField h in transaction.Request.ProxyAuthorization.HeaderFields){
                ackRequest.Authorization.Add(h.Value);
            }

            // Send ACK.
            SendAck(dialog,ackRequest);

            // Start receive 2xx retransmissions.
            transaction.ResponseReceived += delegate(object sender,SIP_ResponseReceivedEventArgs e){
                if(this.State == SIP_CallState.Disposed || this.State == SIP_CallState.Terminated){                    
                    return;
                }

                // Don't send ACK for forked 2xx, our sent BYE(to all early dialogs) or their early timer will kill these dialogs.
                // Send ACK only to our accepted dialog 2xx response retransmission.
                if(e.Response.From.Tag == ackRequest.From.Tag && e.Response.To.Tag == ackRequest.To.Tag){
                    SendAck(dialog,ackRequest);
                }
            };
        }

        #endregion

        #region method SendAck

        /// <summary>
        /// Sends ACK to remote-party.
        /// </summary>
        /// <param name="dialog">SIP dialog.</param>
        /// <param name="ack">SIP ACK request.</param>
        private void SendAck(SIP_Dialog dialog,SIP_Request ack)
        {
            if(dialog == null){
                throw new ArgumentNullException("dialog");
            }
            if(ack == null){
                throw new ArgumentNullException("ack");
            }

            try{
                // Try existing flow.
                dialog.Flow.Send(ack);

                // Log
                if(dialog.Stack.Logger != null){
                    byte[] ackBytes = ack.ToByteData();

                    dialog.Stack.Logger.AddWrite(
                        dialog.ID,
                        null,
                        ackBytes.Length,
                        "Request [DialogID='" +  dialog.ID + "';" + "method='" + ack.RequestLine.Method + "'; cseq='" + ack.CSeq.SequenceNumber + "'; " + 
                        "transport='" + dialog.Flow.Transport + "'; size='" + ackBytes.Length + "'] sent '" + dialog.Flow.LocalEP + "' -> '" + dialog.Flow.RemoteEP + "'.",                                
                        dialog.Flow.LocalEP,
                        dialog.Flow.RemoteEP,
                        ackBytes
                    );
                }
            }
            catch{
                /* RFC 3261 13.2.2.4.
                    Once the ACK has been constructed, the procedures of [4] are used to
                    determine the destination address, port and transport.  However, the
                    request is passed to the transport layer directly for transmission,
                    rather than a client transaction.
                */
                try{
                    dialog.Stack.TransportLayer.SendRequest(ack);
                }
                catch(Exception x){
                    // Log
                    if(dialog.Stack.Logger != null){
                        dialog.Stack.Logger.AddText("Dialog [id='" + dialog.ID + "'] ACK send for 2xx response failed: " + x.Message + ".");
                    }
                }
            }
        }

        #endregion

        #region method Handle2xx

        /// <summary>
        /// This method takes care of INVITE 2xx response retransmissions while ACK received.
        /// </summary>
        /// <param name="transaction">INVITE server transaction.</param>
        /// <exception cref="ArgumentException">Is raised when <b>transaction</b> is null reference.</exception>
        private void Handle2xx(SIP_ServerTransaction transaction)
        {
            if(transaction == null){
                throw new ArgumentException("transaction");
            }

            /* RFC 6026 8.1.
                Once the response has been constructed, it is passed to the INVITE
                server transaction.  In order to ensure reliable end-to-end
                transport of the response, it is necessary to periodically pass
                the response directly to the transport until the ACK arrives.  The
                2xx response is passed to the transport with an interval that
                starts at T1 seconds and doubles for each retransmission until it
                reaches T2 seconds (T1 and T2 are defined in Section 17).
                Response retransmissions cease when an ACK request for the
                response is received.  This is independent of whatever transport
                protocols are used to send the response.
             
                If the server retransmits the 2xx response for 64*T1 seconds without
                receiving an ACK, the dialog is confirmed, but the session SHOULD be
                terminated.  This is accomplished with a BYE, as described in Section
                15.
              
                 T1 - 500
                 T2 - 4000
            */

            TimerEx timer = null;
            
            EventHandler<SIP_RequestReceivedEventArgs> callback = delegate(object s1,SIP_RequestReceivedEventArgs e){
                try{
                    if(e.Request.RequestLine.Method == SIP_Methods.ACK){
                        // ACK for INVITE 2xx response received, stop retransmitting INVITE 2xx response.
                        if(transaction.Request.CSeq.SequenceNumber == e.Request.CSeq.SequenceNumber){
                            if(timer != null){
                                timer.Dispose();
                            }
                        }
                    }
                }
                catch{
                    // We don't care about errors here.
                }
            };
            m_pDialog.RequestReceived += callback;
                
            // Create timer and sart retransmitting INVITE 2xx response.
            timer = new TimerEx(500);
            timer.AutoReset = false;
            timer.Elapsed += delegate(object s,System.Timers.ElapsedEventArgs e){
                try{
                    lock(transaction.SyncRoot){
                        if(transaction.State == SIP_TransactionState.Accpeted){
                            transaction.SendResponse(transaction.FinalResponse);
                        }
                        else{
                            timer.Dispose();

                            return;
                        }
                    }

                    timer.Interval = Math.Min(timer.Interval * 2,4000);
                    timer.Enabled = true;
                }
                catch{
                    // We don't care about errors here.
                }
            };
            timer.Disposed += delegate(object s1,EventArgs e1){
                try{
                    m_pDialog.RequestReceived -= callback;
                }
                catch{
                    // We don't care about errors here.
                }
            };
            timer.Enabled = true;                       
        }

        #endregion

 // TODO: State check for properties.               
        #region Properties implementation

        /// <summary>
        /// Gets current call state.
        /// </summary>
        public SIP_CallState State
        {
            get{ return m_CallState; }
        }

        /// <summary>
        /// Gets or sests audio-out device.
        /// </summary>
        public AudioOutDevice AudioOutDevice
        {
            get{ return m_pAudioOutDevice; }

            set{
                if(m_pAudioOutDevice != value){
                    m_pAudioOutDevice = value;

                    m_pPlayer = new WavePlayer(m_pAudioOutDevice);

                    foreach(SDP_MediaDescription media in this.LocalSDP.MediaDescriptions){
                        if(media.Tags.ContainsKey("rtp_audio_out")){
                            ((AudioOut_RTP)media.Tags["rtp_audio_out"]).AudioOutDevice = m_pAudioOutDevice;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sests audio-in device.
        /// </summary>
        public AudioInDevice AudioInDevice
        {
            get{ return m_pAudioInDevice; }

            set{
                if(m_pAudioInDevice != value){
                    m_pAudioInDevice = value;

                    foreach(SDP_MediaDescription media in this.LocalSDP.MediaDescriptions){
                        if(media.Tags.ContainsKey("rtp_audio_in")){
                            ((AudioIn_RTP)media.Tags["rtp_audio_in"]).AudioInDevice = m_pAudioInDevice;
                        }
                    }
                }
            }
        }
                
        /// <summary>
        /// Gets call start time.
        /// </summary>
        public DateTime StartTime
        {
            get{ return m_StartTime; }
        }

        /// <summary>
        /// Gets call RTP multimedia session.
        /// </summary>
        public RTP_MultimediaSession RtpMultimediaSession
        {
            get{ return m_pRtpMultimediaSession; }
        }

        /// <summary>
        /// Gets call dialog. Returns null if dialog not created yet.
        /// </summary>
        public SIP_Dialog_Invite Dialog
        {
            get{ return m_pDialog; }
        }

        /// <summary>
        /// Gets or sets current local SDP.
        /// </summary>
        public SDP_Message LocalSDP
        {
            get{ return m_pLocalSDP; }
        }

        /// <summary>
        /// Gets or sets current remote SDP.
        /// </summary>
        public SDP_Message RemoteSDP
        {
            get{ return m_pRemoteSDP; }
        }

        /// <summary>
        /// Gets if we are holding a call.
        /// </summary>
        public bool IsLocalOnHold
        {
            get{ return m_IsOnHold; }
        }

        /// <summary>
        /// Gets user data items collection.
        /// </summary>
        public Dictionary<string,object> Tags
        {
            get{ return m_pTags; }
        }

        #endregion

        #region Events implementation
        
        /// <summary>
        /// Is raised when call state has changed.
        /// </summary>
        public event EventHandler StateChanged = null;

        #region method OnStateChanged

        /// <summary>
        /// Raises <b>StateChanged</b> event.
        /// </summary>
        /// <param name="state">New call state.</param>
        private void OnStateChanged(SIP_CallState state)
        {
            if(this.StateChanged != null){
                this.StateChanged(this,new EventArgs());
            }
        }

        #endregion

        #endregion
    }
}
