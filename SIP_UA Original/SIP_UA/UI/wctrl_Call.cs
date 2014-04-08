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
        private Button m_pAccept      = null;
        private Button m_pHangup      = null;

        private object                    m_pLock                 = new object();
        private wfrm_Main                 m_pMainUI               = null;
        private RTP_MultimediaSession     m_pRtpMultimediaSession = null;
        private DateTime                  m_StartTime;
        private SDP_Message               m_pLocalSDP             = null;
        private SDP_Message               m_pRemoteSDP            = null; 
        private bool                      m_IsOnHold              = false;
        private WavePlayer                m_pPlayer               = null;
        private AudioOutDevice            m_pAudioOutDevice       = null;
        private AudioInDevice             m_pAudioInDevice        = null;
        private Timer                     m_pTimerDuration        = null;
        private Dictionary<string,object> m_pTags                 = null;
        private SIP_UA m_pUA = null;
        private  SIP_UA_Call m_pUACall = null;
        private wfrm_RTP_Debug rtpDebug = null;
        private RTP_Session rtpSession = null;

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
                if (this.State == SIP_UA_CallState.Disposed)
                {
                    return;
                }
                m_pUACall.Dispose();

                //m_pUA.Stack = null;
                m_pLocalSDP = null;
                if(m_pTimerDuration != null){
                    m_pTimerDuration.Dispose();
                    m_pTimerDuration = null;
                }

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

            
            bool sips = false;
            if(string.Equals(to.Uri.Scheme,"sips",StringComparison.InvariantCultureIgnoreCase)){
                sips = true;
            }
            
            #region Setup RTP session
            
            m_pRtpMultimediaSession = new RTP_MultimediaSession(RTP_Utils.GenerateCNAME());
            rtpSession = m_pMainUI.CreateRtpSession(m_pRtpMultimediaSession);
            // Port search failed.
            if(rtpSession == null){
                throw new Exception("Calling not possible, RTP session failed to allocate IP end points.");
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

            m_pUACall = m_pUA.CreateCall(invite);
            m_pUACall.Start();

            // We need BeginInvoke here to access UI, we are running on thread pool thread.
            this.BeginInvoke(new MethodInvoker(delegate()
            {
                m_pUACall.StateChanged += new EventHandler(m_pCall_StateChanged);
                m_pStatusText.Text = "Calling";
                m_pDuration.Text = "00:00:00";
                m_pDisplayName.Text = to.Uri.ToString();
                m_pHangup.Enabled = false;

            }));

        }
                
        #endregion

        #region method InitIncoming

        /// <summary>
        /// Initializes incoming call.
        /// </summary>
        /// <param name="account">Local account that accepted call.</param>
        /// <param name="transaction">Incomin INVITE server transaction.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>account</b> or <b>transaction</b> is null reference.</exception>
        internal void InitIncoming(Account account,SIP_UA_Call call)
        {
            if(account == null){
                throw new ArgumentNullException("account");
            }
            if (call == null)
            {
                throw new ArgumentNullException("SIP_UA_Call");
            }

            m_pUACall = call;
            m_pUACall.SendRinging();

            this.BeginInvoke(new MethodInvoker(delegate()
            {
                m_pUACall.StateChanged += new EventHandler(m_pCall_StateChanged);
                m_pStatusText.Text = "Incoming call";
                m_pDuration.Text = "00:00:00";
                m_pDisplayName.Text = m_pUACall.RemoteUri.ToString();
                m_pAccept.Visible = true;

            }));

        }

        #endregion


        #region Events handling

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
                sdpLocal.Origin = new SDP_Origin("-", sdpLocal.GetHashCode(), 1, "IN", "IP4", System.Net.Dns.GetHostAddresses("")[0].ToString());
                sdpLocal.SessionName = "SIP Call";
                sdpLocal.Times.Add(new SDP_Time(0, 0));

                ProcessMediaOffer(m_pUACall, m_pUACall.RemoteSDP, sdpLocal);

                m_pUACall.Accept(m_pLocalSDP);

                m_pAccept.Visible = false;
            }
            catch(Exception x){
                m_pUACall.Terminate("Error: " + x.Message, false);
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
            m_pHangup.Enabled = false;
            Terminate("Hang Up");
            
            if (rtpDebug != null)
            {
                rtpDebug.Dispose();
            }
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
            #region Ringing
            if(this.State == SIP_UA_CallState.Ringing){

                m_pPlayer.Play(ResManager.GetStream("ringing.wav"), 20);

                m_pStatusText.BeginInvoke(new MethodInvoker(delegate()
                {
                    m_pStatusText.Text = "Ringing";
                }));

            }
            #endregion

            #region Active
            else if (this.State == SIP_UA_CallState.Active)
            {
                // Stop ringing.
                m_pPlayer.Stop();

                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    if (m_pMainUI.IsDebug)
                    {
                        rtpDebug = new wfrm_RTP_Debug(m_pRtpMultimediaSession);
                        rtpDebug.Show();
                    }

                // We need BeginInvoke here to access UI, we are running on thread pool thread.
                
                    m_StartTime           = DateTime.Now;
                    m_pStatusText.Text    = "Call established";
                    m_pHangup.Enabled = true;
                    
                    // Start "duration" updater timer.
                    m_pTimerDuration = new Timer();
                    m_pTimerDuration.Interval = 1000;
                    m_pTimerDuration.Tick += new EventHandler(m_pTimerDuration_Tick);
                    m_pTimerDuration.Enabled = true;
                }));

                ProcessMediaOffer(m_pUACall, m_pUACall.RemoteSDP, m_pLocalSDP);

            }

            #endregion

            #region Terminated

            else if (this.State == SIP_UA_CallState.Terminated)
            {
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

                if (m_pRtpMultimediaSession != null)
                {
                    m_pRtpMultimediaSession.Dispose();
                    m_pRtpMultimediaSession = null;
                    
                }

                // We need BeginInvoke here to access UI, we are running on thread pool thread.
                this.BeginInvoke(new MethodInvoker(delegate(){
                    this.BackColor = Color.WhiteSmoke;
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

                        m_pMainUI.Remove_wfrm_Call(this);
                    };
                    timerDispose.Enabled = true;
                }));
                m_pStatusText.BeginInvoke(new MethodInvoker(delegate()
                {
                    m_pStatusText.Text = "Terminated";
                }));
                m_pPlayer.Stop();
            }

            #endregion
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
                if (this.State == SIP_UA_CallState.Active)
                {
                    TimeSpan duration = (DateTime.Now - this.StartTime);
                    m_pDuration.Text = duration.Hours.ToString("00") + ":" + duration.Minutes.ToString("00") + ":" + duration.Seconds.ToString("00");
                }
            }
            catch{
                // We don't care about errors here.
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
            m_pUACall.Terminate(reason, true);
        }
        
        /// <summary>
        /// Terminates a call.
        /// </summary>
        /// <param name="reason">Call termination reason. This text is sent to remote-party.</param>
        /// <param name="sendBye">If true BYE request with <b>reason</b> text is sent remote-party.</param>
        public void Terminate(string reason,bool sendBye)
        {        
            lock(m_pLock){
                
                if (this.State == SIP_UA_CallState.Disposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                if (this.State == SIP_UA_CallState.Terminating || this.State == SIP_UA_CallState.Terminated)
                {
                    return;
                }
                else if (this.State == SIP_UA_CallState.Active)
                {
                    m_pUACall.Dispose();
                    // We need BeginInvoke here to access UI, we are running on thread pool thread.
                    this.BeginInvoke(new MethodInvoker(delegate()
                    {
                        m_pStatusText.Text = reason;
                    }));
                }
                else if (this.State == SIP_UA_CallState.Calling)
                {
                    /* RFC 3261 15.
                        If we are caller and call is not active yet, we must do following actions:
                            *) Send CANCEL, set call Terminating flag.
                            *) If we get non 2xx final response, we are done. (Normally cancel causes '408 Request terminated')
                            *) If we get 2xx response (2xx sent by remote party before our CANCEL reached), we must send BYE to active dialog.
                    */
                    m_pUACall.Dispose();
                }
            }
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
        private void ProcessMediaOffer(SIP_UA_Call call,SDP_Message offer,SDP_Message localSDP)
        {
            if (call == null)
            {
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
                    call.Reject(SIP_ResponseCodes.x500_Server_Internal_Error + ": Invalid SDP answer: Required 'v'(Protocol Version) field is missing.");
                    return;
                }

                // Origin field must exist.
                if(offer.Origin == null){
                    call.Reject(SIP_ResponseCodes.x500_Server_Internal_Error + ": Invalid SDP answer: Required 'o'(Origin) field is missing.");
                    return;
                }

                // Session Name field.

                // Check That global 'c' connection attribute exists or otherwise each enabled media stream must contain one.
                if(offer.Connection == null){
                    for(int i=0;i<offer.MediaDescriptions.Count;i++){
                        if(offer.MediaDescriptions[i].Connection == null){
                            call.Reject(SIP_ResponseCodes.x500_Server_Internal_Error + ": Invalid SDP answer: Global or per media stream no: " + i + " 'c'(Connection) attribute is missing.");
                            return;
                        }
                    }
                }

                #endregion
                                                            
                // Re-INVITE media streams count must be >= current SDP streams.
                if(localSDP.MediaDescriptions.Count > offer.MediaDescriptions.Count){
                    call.Reject(SIP_ResponseCodes.x500_Server_Internal_Error + ": re-INVITE SDP offer media stream count must be >= current session stream count.");
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

            }
            catch(Exception x){
                call.Reject(SIP_ResponseCodes.x500_Server_Internal_Error + ": " + x.Message);
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
        private void ProcessMediaAnswer(SIP_UA_Call call,SDP_Message offer,SDP_Message answer)
        {
            if (call == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if(offer == null){
                throw new ArgumentNullException("offer");
            }
            if(answer == null){
                throw new ArgumentNullException("answer");
            }

            try{
                
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

 // TODO: State check for properties.               
        #region Properties implementation


        /// <summary>
        /// Gets current call state.
        /// </summary>
        public SIP_UA_CallState State
        {
            get { return (SIP_UA_CallState)m_pUACall.State; }
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
        
        #endregion
    }
}
