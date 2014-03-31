using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Threading;

using LumiSoft.Net;
using LumiSoft.Net.SDP;
using LumiSoft.Net.SIP;
using LumiSoft.Net.SIP.Stack;
using LumiSoft.Net.SIP.Debug;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.Media;
using LumiSoft.Net.Media.Codec;
using LumiSoft.Net.Media.Codec.Audio;
using LumiSoft.Net.RTP;
using LumiSoft.Net.RTP.Debug;
using LumiSoft.Net.STUN.Client;
using LumiSoft.Net.UPnP.NAT;
using LumiSoft.SIP.UA;
using LumiSoft.SIP.UA.Resources;

using LumiSoft.Net.AUTH;
using LumiSoft.Net.SIP.UA;

namespace LumiSoft.SIP.UA.UI
{
    /// <summary>
    /// Application main window.
    /// </summary>
    public class wfrm_Main : Form
    {
        private MenuStrip      m_pMenu        = null;
        private ToolStrip      m_pToolbar     = null;
        private ComboBox       m_pAccounts    = null;
        private Button         m_pCall        = null;
        private ComboBox       m_pCallUriType = null;
        private ComboBox       m_pCallUri     = null;
        private TabControl     m_pCallTab     = null;
        private ListView       m_pCallTab_Contacts_Contacts = null;
        private wctrl_CallList m_pCallTab_Calls_Calls = null;
        private ListView       m_pCallTab_Status_Registrations = null;

        private bool                       m_IsDisposing     = false;
        private bool                       m_IsDebug         = true;
        private Settings                   m_pSettings       = null;

        private SIP_Stack m_pStack = null;
        private SIP_UA m_pUA = null;
        private List<wfrm_IM>              m_pActiveIM       = null;        
        private wfrm_SIP_Debug             m_pDebugFrom      = null;
        private string                     m_NatHandlingType = "";
        private string                     m_StunServer      = "stun.iptel.org";
        private UPnP_NAT_Client            m_pUPnP           = null;
        private Dictionary<int,AudioCodec> m_pAudioCodecs    = null;
        private AudioOutDevice             m_pAudioOutDevice = null;
        private AudioInDevice              m_pAudioInDevice  = null;
        private int                        m_RtpBasePort     = 12000;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="isDebug">Specifies if debug windows showed.</param>
        public wfrm_Main(bool isDebug)
        {
            InitUI();
            m_pStack = new SIP_Stack();
            m_pUA = new SIP_UA(m_pStack);
            m_IsDebug = isDebug;

            m_pSettings = new Settings();
            m_pUA.Stack.UserAgent = "LumiSoft SIP UA 1.0";
            m_pUA.Stack.Error += new EventHandler<ExceptionEventArgs>(m_pStack_Error);
            m_pActiveIM = new List<wfrm_IM>();
                        
            // Show SIP debug UI if in debug mode.
            if(m_IsDebug){
                m_pDebugFrom = new wfrm_SIP_Debug(m_pStack);
                m_pDebugFrom.Show();
            }

            // Remove me:
            m_pCallUri.Items.Add("u1@domaina.com");
            m_pCallUri.Items.Add("u2@domainb.com");
        }
                                                                
        #region method InitUI

        /// <summary>
        /// Creates and initializes UI.
        /// </summary>
        private void InitUI()
        {
            this.ClientSize = new Size(300,400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = ResManager.GetIcon("app.ico",new Size(16,16));
            this.Text = "SIP";
            this.FormClosed += new FormClosedEventHandler(wfrm_Main_FormClosed);
            this.VisibleChanged += new EventHandler(wfrm_Main_VisibleChanged);

            m_pMenu = new MenuStrip();
            m_pMenu.Size = new Size(140,25);
            m_pMenu.Location = new Point(0,0);
            m_pMenu.Dock = DockStyle.None;
            ToolStripMenuItem menu_file = new ToolStripMenuItem("File");
                ToolStripMenuItem menu_file_exit = new ToolStripMenuItem("Exit");
                menu_file_exit.Tag = "exit";
                menu_file_exit.Click += new EventHandler(m_pMenu_ItemClicked);
                menu_file.DropDownItems.Add(menu_file_exit);
            m_pMenu.Items.Add(menu_file);
            ToolStripMenuItem menu_settings = new ToolStripMenuItem("Settings");
                ToolStripMenuItem menu_settings_proxy = new ToolStripMenuItem("Proxy Settings");
                menu_settings_proxy.Tag = "proxy";
                menu_settings_proxy.Click += new EventHandler(m_pMenu_ItemClicked);
                menu_settings.DropDownItems.Add(menu_settings_proxy);
                ToolStripMenuItem menu_settings_credentials = new ToolStripMenuItem("Credentials");
                menu_settings_credentials.Tag = "credentials";
                menu_settings_credentials.Click += new EventHandler(m_pMenu_ItemClicked);
                menu_settings.DropDownItems.Add(menu_settings_credentials);
                ToolStripMenuItem menu_settings_accounts = new ToolStripMenuItem("Accounts");
                menu_settings_accounts.Tag = "accounts";
                menu_settings_accounts.Click += new EventHandler(m_pMenu_ItemClicked);
                menu_settings.DropDownItems.Add(menu_settings_accounts);
                ToolStripMenuItem menu_settings_audio = new ToolStripMenuItem("Audio");
                menu_settings_audio.Tag = "audio";
                menu_settings_audio.Click += new EventHandler(m_pMenu_ItemClicked);
                menu_settings.DropDownItems.Add(menu_settings_audio);
            m_pMenu.Items.Add(menu_settings);

            m_pToolbar = new ToolStrip();
            m_pToolbar.Size = new Size(70,20);
            m_pToolbar.Location = new Point(195,0);
            m_pToolbar.Dock = DockStyle.None;
            m_pToolbar.Renderer = new ToolBarRendererEx();
            //--- Audio button
            ToolStripDropDownButton button_Audio = new ToolStripDropDownButton();
            button_Audio.Name = "audio";
            button_Audio.Image = ResManager.GetIcon("speaker.ico").ToBitmap();            
            foreach(AudioOutDevice device in AudioOut.Devices){
                ToolStripMenuItem item = new ToolStripMenuItem(device.Name);
                item.Checked = (button_Audio.DropDownItems.Count == 0);
                item.Tag = device;
                button_Audio.DropDownItems.Add(item);
            }
            button_Audio.DropDown.ItemClicked += new ToolStripItemClickedEventHandler(m_pToolBar_Audio_ItemClicked);
            m_pToolbar.Items.Add(button_Audio);
            //--- Microphone button
            ToolStripDropDownButton button_Mic = new ToolStripDropDownButton();
            button_Mic.Name = "mic";
            button_Mic.Image = ResManager.GetIcon("mic.ico").ToBitmap();
            foreach(AudioInDevice device in AudioIn.Devices){
                ToolStripMenuItem item = new ToolStripMenuItem(device.Name);
                item.Checked = (button_Mic.DropDownItems.Count == 0);
                item.Tag = device;
                button_Mic.DropDownItems.Add(item);
            }
            button_Mic.DropDown.ItemClicked += new ToolStripItemClickedEventHandler(m_pToolBar_Mic_ItemClicked);
            m_pToolbar.Items.Add(button_Mic);
            // Separator
            m_pToolbar.Items.Add(new ToolStripSeparator());
            // NAT
            ToolStripDropDownButton button_NAT = new ToolStripDropDownButton();
            button_NAT.Name = "nat";
            button_NAT.Image = ResManager.GetIcon("router.ico").ToBitmap();
            button_NAT.DropDown.ItemClicked += new ToolStripItemClickedEventHandler(m_pToolbar_NAT_DropDown_ItemClicked);
            m_pToolbar.Items.Add(button_NAT);

            m_pAccounts = new ComboBox();
            m_pAccounts.Size = new Size(190,20);
            m_pAccounts.Location = new Point(5,40);
            m_pAccounts.DropDownStyle = ComboBoxStyle.DropDownList;

            m_pCall = new Button();
            m_pCall.Size = new Size(45,45);
            m_pCall.Location = new Point(202,40);
            m_pCall.Image = ResManager.GetIcon("call.ico",new Size(24,24)).ToBitmap();
            m_pCall.Click += new EventHandler(m_pCall_Click);

            m_pCallUriType = new ComboBox();
            m_pCallUriType.Size = new Size(45,20);
            m_pCallUriType.Location = new Point(5,65);
            m_pCallUriType.Items.Add("sip:");
            m_pCallUriType.SelectedIndex = 0;

            m_pCallUri = new ComboBox();
            m_pCallUri.Size = new Size(145,20);
            m_pCallUri.Location = new Point(50,65);
                        
            m_pCallTab = new TabControl();
            m_pCallTab.Size = new Size(300,300);
            m_pCallTab.Location = new Point(0,100);
            m_pCallTab.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            m_pCallTab.TabPages.Add("contacts","Contacts");
            m_pCallTab.TabPages.Add("calls","Active Calls");
            m_pCallTab.TabPages.Add("history","History");
            m_pCallTab.TabPages.Add("status","Status");

            #region Contacts tab

            m_pCallTab.TabPages["contacts"].BackColor = Color.White;

            m_pCallTab_Contacts_Contacts = new ListView();
            m_pCallTab_Contacts_Contacts.Size = new Size(260,200);
            m_pCallTab_Contacts_Contacts.Location = new Point(0,0);
            m_pCallTab_Contacts_Contacts.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            m_pCallTab_Contacts_Contacts.BorderStyle = BorderStyle.None;
            m_pCallTab_Contacts_Contacts.View = View.Details;
            m_pCallTab_Contacts_Contacts.FullRowSelect = true;
            m_pCallTab_Contacts_Contacts.HeaderStyle = ColumnHeaderStyle.None;
            m_pCallTab_Contacts_Contacts.MouseUp += new MouseEventHandler(m_pCallTab_Contacts_Contacts_MouseUp);
            m_pCallTab_Contacts_Contacts.Columns.Add("Name",240);

            m_pCallTab.TabPages["contacts"].Controls.Add(m_pCallTab_Contacts_Contacts);

            #endregion

            #region Acive calls tab

            m_pCallTab.TabPages["calls"].BackColor = Color.White;

            m_pCallTab_Calls_Calls = new wctrl_CallList();
            m_pCallTab_Calls_Calls.Dock = DockStyle.Fill;
            m_pCallTab_Calls_Calls.BorderStyle = BorderStyle.None;

            m_pCallTab.TabPages["calls"].Controls.Add(m_pCallTab_Calls_Calls);

            #endregion

            #region Status tab

            m_pCallTab.TabPages["status"].BackColor = Color.White;

            ImageList callTab_Satus_RegistrationsImages = new ImageList();
            callTab_Satus_RegistrationsImages.Images.Add(ResManager.GetIcon("register_ok.ico",new Size(16,16)));
            callTab_Satus_RegistrationsImages.Images.Add(ResManager.GetIcon("register_error.ico",new Size(16,16)));
            callTab_Satus_RegistrationsImages.Images.Add(ResManager.GetIcon("register_registering.ico",new Size(16,16)));

            m_pCallTab_Status_Registrations = new ListView();
            m_pCallTab_Status_Registrations.Size = new Size(260,200);
            m_pCallTab_Status_Registrations.Location = new Point(0,0);
            m_pCallTab_Status_Registrations.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            m_pCallTab_Status_Registrations.BorderStyle = BorderStyle.None;
            m_pCallTab_Status_Registrations.View = View.Details;
            m_pCallTab_Status_Registrations.FullRowSelect = true;
            m_pCallTab_Status_Registrations.ShowItemToolTips = true;
            m_pCallTab_Status_Registrations.SmallImageList = callTab_Satus_RegistrationsImages;
            m_pCallTab_Status_Registrations.Columns.Add("Registraion",180);
            m_pCallTab_Status_Registrations.Columns.Add("Status",80);

            m_pCallTab.TabPages["status"].Controls.Add(m_pCallTab_Status_Registrations);

            #endregion

            this.Controls.Add(m_pMenu);
            this.Controls.Add(m_pToolbar);
            this.Controls.Add(m_pAccounts);
            this.Controls.Add(m_pCallUriType);
            this.Controls.Add(m_pCallUri);
            this.Controls.Add(m_pCall);
            this.Controls.Add(m_pCallTab);

            m_pCallTab.SelectedTab = m_pCallTab.TabPages["status"];
        }
                                                                                                
        #endregion


        #region Events handling

        #region method m_pMenu_ItemClicked

        /// <summary>
        /// This method is called when main menu item clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pMenu_ItemClicked(object sender,EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            if(item.Tag == null){
                return;
            }
            else if(item.Tag.ToString() == "accounts"){
                wfrm_Settings frm = new wfrm_Settings();
                frm.ShowDialog(this);

                /*
                wfrm_Settings_Accounts frm = new wfrm_Settings_Accounts();
                if(frm.ShowDialog(this) == DialogResult.OK){
                    Restart();
                }*/
            }
            else if(item.Tag.ToString() == "exit"){
                this.Close();
            }
        }

        #endregion

        #region method m_pToolBar_Audio_ItemClicked

        /// <summary>
        /// Is called when new audio-out device is selected.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pToolBar_Audio_ItemClicked(object sender,ToolStripItemClickedEventArgs e)
        {   
            try{
                foreach(ToolStripMenuItem item in ((ToolStripDropDownMenu)sender).Items){
                    if(item.Equals(e.ClickedItem)){
                        item.Checked = true;
                    }
                    else{
                        item.Checked = false;
                    }
                }
                
                m_pAudioOutDevice = (AudioOutDevice)e.ClickedItem.Tag;

                foreach(wctrl_Call call in m_pCallTab_Calls_Calls.Calls){
                    call.AudioOutDevice = m_pAudioOutDevice;
                }
            }
            catch(Exception x){
                MessageBox.Show("Error: " + x.Message,"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        #endregion

        #region method m_pToolBar_Mic_ItemClicked

        /// <summary>
        /// Is called when new audio-in device is selected.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pToolBar_Mic_ItemClicked(object sender,ToolStripItemClickedEventArgs e)
        {   
            try{
                foreach(ToolStripMenuItem item in ((ToolStripDropDownMenu)sender).Items){
                    if(item.Equals(e.ClickedItem)){
                        item.Checked = true;
                    }
                    else{
                        item.Checked = false;
                    }
                }
                
                m_pAudioInDevice = (AudioInDevice)e.ClickedItem.Tag;
                
                foreach(wctrl_Call call in m_pCallTab_Calls_Calls.Calls){
                    call.AudioInDevice = m_pAudioInDevice;
                }
            }
            catch(Exception x){
                MessageBox.Show("Error: " + x.Message,"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        #endregion

        #region method m_pToolbar_NAT_DropDown_ItemClicked

        /// <summary>
        /// Is called when new NAT handling method is selected.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pToolbar_NAT_DropDown_ItemClicked(object sender,ToolStripItemClickedEventArgs e)
        {
            if(!e.ClickedItem.Enabled){
                return;
            }
            
            foreach(ToolStripMenuItem item in ((ToolStripDropDownMenu)sender).Items){
                if(item.Equals(e.ClickedItem)){
                    item.Checked = true;
                    m_NatHandlingType = item.Name;
                }
                else{
                    item.Checked = false;
                }
            }
        }

        #endregion

        #region method m_pCall_Click

        /// <summary>
        /// This method is called when call button is pressed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pCall_Click(object sender,EventArgs e)
        {
            if(m_pCallUri.Text == string.Empty){
                MessageBox.Show(this,"Please fill call URI !","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }

            string uri = m_pCallUri.Text;
            if(uri.IndexOf(':') == -1){
                uri = "sip:" + uri;
            }

            Call(((Account)((WComboBoxItem)m_pAccounts.SelectedItem).Tag),new SIP_t_NameAddress(uri));
            m_pCallUri.Text = "";
            m_pCallTab.SelectedTab = m_pCallTab.TabPages["calls"];
        }

        #endregion

        #region method wfrm_Main_FormClosed

        /// <summary>
        /// This method is called when main form is closed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void wfrm_Main_FormClosed(object sender,FormClosedEventArgs e)
        {
            m_IsDisposing = true;
            if(m_pDebugFrom != null){
                m_pDebugFrom.Close();
            }
            if (m_pUA != null) {
                m_pUA.Dispose();
            }
        }

        #endregion

        #region method wfrm_Main_VisibleChanged

        /// <summary>
        /// This method is called when main windows visibility changes.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void wfrm_Main_VisibleChanged(object sender, EventArgs e)
        {
            if(this.Visible){
                LoadSettings();
            }
        }

        #endregion


        #region method m_pCallTab_Contacts_Contacts_MouseUp

        /// <summary>
        /// This method is called when contacts list mouse button is released.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pCallTab_Contacts_Contacts_MouseUp(object sender,MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Right){
                return;
            }

            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem menu_Add = new ToolStripMenuItem("New Contact");
            menu_Add.Click += new EventHandler(delegate(object s1,EventArgs e1){
                // TODO:
            });
            menu.Items.Add(menu_Add);

            if(m_pCallTab_Contacts_Contacts.SelectedItems.Count > 0){
                menu.Items.Add(new ToolStripSeparator());

                ToolStripMenuItem menu_Call = new ToolStripMenuItem("Call");
                menu_Call.Click += new EventHandler(delegate(object s1,EventArgs e1){
                    // TODO:
                    // FIX ME:

                    string uri = m_pCallTab_Contacts_Contacts.SelectedItems[0].Text;
                    if(uri.IndexOf(':') == -1){
                        uri = "sip:" + uri;
                    }
                    m_pCallTab.SelectedTab = m_pCallTab.TabPages["calls"];

                    Call(((Account)((WComboBoxItem)m_pAccounts.SelectedItem).Tag),new SIP_t_NameAddress(uri));
                });
                menu.Items.Add(menu_Call);

                ToolStripMenuItem menu_IM = new ToolStripMenuItem("IM");
                menu_IM.Click += new EventHandler(delegate(object s1,EventArgs e1){
                    // TODO:
                    // FIX ME:

                    string uri = m_pCallTab_Contacts_Contacts.SelectedItems[0].Text;
                    if(uri.IndexOf(':') == -1){
                        uri = "sip:" + uri;
                    }

                });
                menu.Items.Add(menu_IM);
            }

            menu.Show(m_pCallTab_Contacts_Contacts,e.Location);
        }

        #endregion


        #region method m_pCallTab_Calls_Calls_MouseUp

        /// <summary>
        /// This method is called when calls list mouse button is released.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pCallTab_Calls_Calls_MouseUp(object sender,MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Right){
                return;
            }
            /*
            if(m_pCallTab_Calls_Calls.SelectedItems.Count > 0){
                Call call = (Call)m_pCallTab_Calls_Calls.SelectedItems[0].Tag;

                ContextMenuStrip menu = new ContextMenuStrip();
                if(call.State == CallState.WaitingToAccept){
                    // Accept
                    ToolStripMenuItem accept = new ToolStripMenuItem("Accept");
                    accept.Click += new EventHandler(delegate(object s1,EventArgs e1){
                        AcceptCall(call);
                    });
                    menu.Items.Add(accept);                   
                }
                if(call.State == CallState.Active){
                    bool onHold = false;
                    if(call.Tags.ContainsKey("OnHold")){
                        onHold = ((bool)call.Tags["OnHold"]);
                    }

                    if(onHold){
                        // Take off on hold
                        ToolStripMenuItem putOnHold = new ToolStripMenuItem("Take Off On Hold");
                        putOnHold.Image = ResManager.GetIcon("call_hangup.ico",new Size(16,16)).ToBitmap();
                        putOnHold.Click += new EventHandler(delegate(object s1,EventArgs e1){
                            ToggleCallOnHold(call);
                        });
                        menu.Items.Add(putOnHold);
                    }
                    else{
                        // Put on hold
                        ToolStripMenuItem putOnHold = new ToolStripMenuItem("Put On Hold");
                        putOnHold.Image = ResManager.GetIcon("call_hangup.ico",new Size(16,16)).ToBitmap();
                        putOnHold.Click += new EventHandler(delegate(object s1,EventArgs e1){
                            ToggleCallOnHold(call);
                        });
                        menu.Items.Add(putOnHold);
                    }
                }
                // Separator
                if(menu.Items.Count > 0){
                    menu.Items.Add(new ToolStripSeparator());
                }
                // Hang Up
                ToolStripMenuItem hangUp = new ToolStripMenuItem("Hang Up");
                hangUp.Image = ResManager.GetIcon("call_hangup.ico",new Size(16,16)).ToBitmap();
                hangUp.Click += new EventHandler(delegate(object s1,EventArgs e1){
                    call.Terminate("Hang Up");
                });                
                menu.Items.Add(hangUp);
                //--------
                menu.Show(m_pCallTab_Calls_Calls,e.Location);
            }*/
        }
                
        #endregion


        #region method Registration_StateChanged

        /// <summary>
        /// This method is called when registration state has changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void Registration_StateChanged(object sender,EventArgs e)
        {   
            if(m_IsDisposing){                
                return;
            }

            try{
                this.BeginInvoke(
                    new MethodInvoker(delegate(){
                        SIP_UA_Registration registration = (SIP_UA_Registration)sender;
            
                        // Remove registration.
                        if(registration.State == SIP_UA_RegistrationState.Disposed){
                            foreach(ListViewItem it in m_pCallTab_Status_Registrations.Items){
                                if(it.Text == registration.AOR){
                                    it.Remove();
                                    break;
                                }
                            }
                        }
                        // Update registration state.
                        else{
                            ListViewItem regItem = null;
                            foreach(ListViewItem it in m_pCallTab_Status_Registrations.Items){
                                if(it.Text == registration.AOR){
                                    regItem = it;
                                    break;
                                }
                            }

                            // New registration, create it to UI.
                            if(regItem == null){
                                regItem = new ListViewItem(registration.AOR);                    
                                regItem.SubItems.Add("");
                                m_pCallTab_Status_Registrations.Items.Add(regItem);
                            }

                            if(registration.State == SIP_UA_RegistrationState.Registered){
                                regItem.ImageIndex = 0;

                                StringBuilder tooltipText = new StringBuilder();
                                tooltipText.AppendLine("AOR: " + registration.AOR);
                                tooltipText.AppendLine("TTL: " + registration.Expires);
                                foreach(AbsoluteUri contactUri in registration.Contacts){
                                    tooltipText.AppendLine("Contact: " + contactUri.ToString());
                                }

                                regItem.ToolTipText = tooltipText.ToString();
                            }
                            else if(registration.State == SIP_UA_RegistrationState.Error){
                                regItem.ImageIndex = 1;

                                //regItem.ToolTipText
                            }
                            else if(registration.State == SIP_UA_RegistrationState.Registering){
                                regItem.ImageIndex = 2;
                            }
                            else{
                                regItem.ImageIndex = -1;
                            }
                            regItem.SubItems[1].Text = registration.State.ToString();                            
                        }
                    }),
                    null
                );
            }
             catch(Exception x){
                string dummy = x.Message;
                // Skip, UA closing.
            }
        }

        #endregion

        private void m_pUA_IncomingCall(object sender, SIP_UA_Call_EventArgs e)
        {
            SIP_Uri requestUri = (SIP_Uri)e.Call.LocalUri;

            // If request URI is our registration contact, get AOR for it.
            string aor = requestUri.Address;
            foreach (SIP_UA_Registration registration in m_pUA.Registrations)
            {
                foreach (AbsoluteUri contactUri in registration.Contacts)
                {
                    if (requestUri.Equals(contactUri))
                    {
                        aor = registration.AOR;
                        break;
                    }
                }
            }
            Account localAccount = null;
            foreach (WComboBoxItem it in m_pAccounts.Items)
            {
                Account account = (Account)it.Tag;
                if (string.Equals(account.AOR, aor, StringComparison.InvariantCultureIgnoreCase))
                {
                    localAccount = account;
                    break;
                }
            }
            if (localAccount == null)
            {
                e.Call.Reject(SIP_ResponseCodes.x404_Not_Found);

                return;
            }

            SDP_Message sdpOffer = e.Call.RemoteSDP;

            // Check if we can accept any media stream.
            bool canAccept = false;
            foreach(SDP_MediaDescription media in sdpOffer.MediaDescriptions){
                if(CanSupportMedia(media)){
                    canAccept = true;

                    break;
                }
            }
            if(!canAccept){
                e.Call.Reject(SIP_ResponseCodes.x606_Not_Acceptable);
                return;
            }
            

            // We need BeginInvoke here to access UI, we are running on thread pool thread.
            this.BeginInvoke(new MethodInvoker(delegate(){
                m_pCallTab.SelectedTab = m_pCallTab.TabPages["calls"];

                // Create call, all continuing processing done in call class.
                wctrl_Call call = new wctrl_Call(this,m_pAudioOutDevice,m_pAudioInDevice);
                m_pCallTab_Calls_Calls.AddCall(call);
                call.InitIncoming(localAccount,e.Call);
            }));
            

        }

        #region method m_pStack_Error

        /// <summary>
        /// Is called when SIP stack has unhandled error.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pStack_Error(object sender,ExceptionEventArgs e)
        {
            // TODO:

            Console.WriteLine("Error: " + e.Exception.ToString());
        }

        #endregion

        #endregion


        #region method Call

        /// <summary>
        /// Calls to the specified person.
        /// </summary>
        /// <param name="account">Local account to use.</param>
        /// <param name="to">Call target address.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>account</b> or <b>to</b> is null reference.</exception>
        public void Call(Account account,SIP_t_NameAddress to)
        {
            if(account == null){
                throw new ArgumentNullException("account");
            }
            if(to == null){
                throw new ArgumentNullException("to");
            }


            // Create call and start calling.
            wctrl_Call call = new wctrl_Call(this,m_pAudioOutDevice,m_pAudioInDevice);
            m_pCallTab_Calls_Calls.AddCall(call);
            // Move processing to thread pool, otherwise UI hangs.
            ThreadPool.QueueUserWorkItem(delegate(object state){
                call.InitCalling(account,to);
            });   
        }
                                                                
        #endregion

        #region method PutAllCallsOnHold

        /// <summary>
        /// Puts all calls on hold.
        /// </summary>
        internal void PutAllCallsOnHold()
        {
            // Put all active calls on hold.
            foreach(wctrl_Call c in this.Calls){
                if (c.State == SIP_UA_CallState.Active && !c.IsLocalOnHold)
                {
                    //c.PutCallOnHold();
                }
            }

            // Wait calls to switch to on-hold.
            DateTime start = DateTime.Now;
            while(start.AddSeconds(5) > DateTime.Now){
                bool allonHold = true;
                foreach(wctrl_Call c in this.Calls){
                    if (c.State == SIP_UA_CallState.Active && !c.IsLocalOnHold)
                    {
                        allonHold = false;
                    }
                }
                if(allonHold){
                    break;
                }
                else{
                    Thread.Sleep(200);
                }
            }
        }

        #endregion


        #region method LoadSettings

        /// <summary>
        /// Loads settings from xml file.
        /// </summary>
        private void LoadSettings()
        {
            if(File.Exists(Application.StartupPath + "\\Settings\\Settings.xml")){
                this.Settings.Load(Application.StartupPath + "\\Settings\\Settings.xml");
            }

            #region Init audio devices

            if(AudioOut.Devices.Length == 0){
                foreach(Control control in this.Controls){
                    control.Enabled = false;
                }

                MessageBox.Show("Calling not possible, there are no speakers in computer.","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);

                return;
            }

            if(AudioIn.Devices.Length == 0){
                foreach(Control control in this.Controls){
                    control.Enabled = false;
                }

                MessageBox.Show("Calling not possible, there is no microphone in computer.","Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);

                return;
            }

            m_pAudioOutDevice = AudioOut.Devices[0];
            m_pAudioInDevice  = AudioIn.Devices[0];

            m_pAudioCodecs = new Dictionary<int,AudioCodec>();
            m_pAudioCodecs.Add(0,new PCMU());
            m_pAudioCodecs.Add(8,new PCMA());

            #endregion
                        
            #region Get NAT handling methods

            m_pUPnP = new UPnP_NAT_Client();

            STUN_Result stunResult = new STUN_Result(STUN_NetType.UdpBlocked,null);
            try{
                stunResult = STUN_Client.Query(m_StunServer,3478,new IPEndPoint(IPAddress.Any,0));                
            }
            catch{                
            }

            if(stunResult.NetType == STUN_NetType.Symmetric || stunResult.NetType == STUN_NetType.UdpBlocked){
                ToolStripMenuItem item_stun = new ToolStripMenuItem("STUN (" + stunResult.NetType + ")");
                item_stun.Name = "stun";
                item_stun.Enabled = false;
                ((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems.Add(item_stun);
            }
            else{
                ToolStripMenuItem item_stun = new ToolStripMenuItem("STUN (" + stunResult.NetType + ")");
                item_stun.Name = "stun";
                ((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems.Add(item_stun);
            }

            if(m_pUPnP.IsSupported){
                ToolStripMenuItem item_upnp = new ToolStripMenuItem("UPnP");
                item_upnp.Name = "upnp";
                ((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems.Add(item_upnp);
            }
            else{
                ToolStripMenuItem item_upnp = new ToolStripMenuItem("UPnP Not Supported");
                item_upnp.Name = "upnp";
                item_upnp.Enabled = false;
                ((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems.Add(item_upnp);
            }

            //if(!((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems["stun"].Enabled && !((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems["upnp"].Enabled){
            //    MessageBox.Show("Calling may not possible, your firewall or router blocks STUN and doesn't support UPnP.\r\n\r\nSTUN Net Type: " + stunResult.NetType + "\r\n\r\nUPnP Supported: " + m_pUPnP.IsSupported,"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
            //}

            ToolStripMenuItem item_no_nat = new ToolStripMenuItem("No NAT handling");
            item_no_nat.Name = "no_nat";
            ((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems.Add(item_no_nat);

            // Select first enabled item.
            foreach(ToolStripItem it in ((ToolStripDropDownButton)m_pToolbar.Items["nat"]).DropDownItems){
                if(it.Enabled){
                    ((ToolStripMenuItem)it).Checked = true;
                    m_NatHandlingType = it.Name;

                    break;
                }
            }

            #endregion

            m_pUA.Stack.BindInfo = Settings.Bindings;
            
            
            m_pStack.Credentials.AddRange(this.Settings.Credentials.ToArray());
            m_pStack.MinimumExpireTime = 30;
            m_pStack.Start();
            m_pUA.IncomingCall += new EventHandler<SIP_UA_Call_EventArgs>(m_pUA_IncomingCall);
            
            
            foreach(Account account in this.Settings.Accounts){
                if(account.Register){
                    SIP_Uri registrarServer = SIP_Uri.Parse(account.RegistrarServer);

                    SIP_UA_Registration registration = m_pUA.CreateRegistration(
                        registrarServer,
                        account.AOR,
                        AbsoluteUri.Parse(registrarServer.Scheme + ":" + account.AOR.Split('@')[0] + "@auto-allocate"),
                        account.RegisterInterval
                    );
                    this.BeginInvoke(new MethodInvoker(delegate(){
                        registration.StateChanged += new EventHandler(Registration_StateChanged);
                        registration.BeginRegister(true);
                    }));
                    // TODO: If TLS supported, add TLS registration too.
                }

                m_pAccounts.Items.Add(new WComboBoxItem(account.AOR,account));
                
            }


            if(m_pAccounts.Items.Count > 0){
                m_pAccounts.SelectedIndex = 0;
            }
        }

        #endregion

        #region method Restart

        /// <summary>
        /// Starts/restarts SIP UA.
        /// </summary>
        private void Restart()
        {
            // TODO:
        }

        #endregion

        #region method OnError

        /// <summary>
        /// This method is called when unhandled exception occured.
        /// </summary>
        /// <param name="x">Exception.</param>
        private void OnError(Exception x)
        {
            // TODO: FIX ME:

            Console.WriteLine("Error:" + x.ToString());
        }

        #endregion

        internal void Remove_wfrm_Call(wctrl_Call call)
        {
            m_pCallTab_Calls_Calls.RemoveCall(call);
        }

        #region method CanSupportMedia

        /// <summary>
        /// Checks if we can support the specified media.
        /// </summary>
        /// <param name="media">SDP media.</param>
        /// <returns>Returns true if we can support this media, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>media</b> is null reference.</exception>
        internal bool CanSupportMedia(SDP_MediaDescription media)
        {
            if(media == null){
                throw new ArgumentNullException("media");
            }

            if(!string.Equals(media.MediaType,SDP_MediaTypes.audio,StringComparison.InvariantCultureIgnoreCase)){
                return false;
            }
            if(!string.Equals(media.Protocol,"RTP/AVP",StringComparison.InvariantCultureIgnoreCase)){
                return false;
            }

            if(GetOurSupportedAudioCodecs(media).Count > 0){
                return true;
            }

            return false;
        }

        #endregion

        #region method GetOurSupportedAudioCodecs

        /// <summary>
        /// Gets audio codecs which we can support from SDP media stream.
        /// </summary>
        /// <param name="media">SDP media stream.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>media</b> is null reference.</exception>
        /// <returns>Returns audio codecs which support.</returns>
        internal Dictionary<int,AudioCodec> GetOurSupportedAudioCodecs(SDP_MediaDescription media)
        {
            if(media == null){
                throw new ArgumentNullException("media");
            }

            Dictionary<int,AudioCodec> codecs = new Dictionary<int,AudioCodec>();

            // Check for IANA registered payload. Custom range is 96-127 and always must have rtpmap attribute.
            foreach(string format in media.MediaFormats){
                int payload = Convert.ToInt32(format);
                if(payload < 96 && m_pAudioCodecs.ContainsKey(payload)){
                    if(!codecs.ContainsKey(payload)){
                        codecs.Add(payload,m_pAudioCodecs[payload]);
                    }
                }
            }

            // Check rtpmap payloads.
            foreach(SDP_Attribute a in media.Attributes){
                if(string.Equals(a.Name,"rtpmap",StringComparison.InvariantCultureIgnoreCase)){
                    // Example: 0 PCMU/8000
                    string[] parts = a.Value.Split(' ');
                    int    payload   = Convert.ToInt32(parts[0]);
                    string codecName = parts[1].Split('/')[0];

                    foreach(AudioCodec codec in m_pAudioCodecs.Values){
                        if(string.Equals(codec.Name,codecName,StringComparison.InvariantCultureIgnoreCase)){
                            if(!codecs.ContainsKey(payload)){
                                codecs.Add(payload,codec);
                            }
                        }
                    }
                }
            }

            return codecs;
        }

        #endregion

        #region method CreateRtpSession

        /// <summary>
        /// Creates new RTP session.
        /// </summary>
        /// <param name="rtpMultimediaSession">RTP multimedia session.</param>
        /// <returns>Returns created RTP session or null if failed to create RTP session.</returns>
        /// <exception cref="ArgumentNullException">Is raised <b>rtpMultimediaSession</b> is null reference.</exception>
        internal RTP_Session CreateRtpSession(RTP_MultimediaSession rtpMultimediaSession)
        {
            if(rtpMultimediaSession == null){
                throw new ArgumentNullException("rtpMultimediaSession");
            }

            //--- Search RTP IP -------------------------------------------------------//
            IPAddress rtpIP = null;
            foreach(IPAddress ip in Dns.GetHostAddresses("")){
                if(ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork){
                    rtpIP = ip;
                    break;
                }
            }
            if(rtpIP == null){
                throw new Exception("None of the network connection is available.");
            }
            //------------------------------------------------------------------------//

            // Search free ports for RTP session.
            for(int i=0;i<100;i+=2){
                try{
                    return rtpMultimediaSession.CreateSession(new RTP_Address(rtpIP,m_RtpBasePort,m_RtpBasePort + 1),new RTP_Clock(1,8000));
                }
                catch{
                    m_RtpBasePort += 2;
                }
            }
            
            return null;
        }

        #endregion

        #region method HandleNAT

        /// <summary>
        /// Handles NAT and stores RTP data to <b>mediaStream</b>.
        /// </summary>
        /// <param name="mediaStream">SDP media stream.</param>
        /// <param name="rtpSession">RTP session.</param>
        /// <returns>Returns true if NAT handled ok, otherwise false.</returns>
        internal bool HandleNAT(SDP_MediaDescription mediaStream,RTP_Session rtpSession)
        {
            if(mediaStream == null){
                throw new ArgumentNullException("mediaStream");
            }
            if(rtpSession == null){
                throw new ArgumentNullException("rtpSession");
            }

            IPEndPoint   rtpPublicEP   = null;
            IPEndPoint   rtcpPublicEP  = null;

            // We have public IP.
            if(!Net_Utils.IsPrivateIP(rtpSession.LocalEP.IP)){
                rtpPublicEP  = rtpSession.LocalEP.RtpEP;
                rtcpPublicEP = rtpSession.LocalEP.RtcpEP;
            }
            // No NAT handling.
            else if(m_NatHandlingType == "no_nat"){
                rtpPublicEP  = rtpSession.LocalEP.RtpEP;
                rtcpPublicEP = rtpSession.LocalEP.RtcpEP;
            }
            // Use STUN.
            else if(m_NatHandlingType == "stun"){
                rtpSession.StunPublicEndPoints(m_StunServer,3478,out rtpPublicEP,out rtcpPublicEP);
            }
            // Use UPnP.
            else if(m_NatHandlingType == "upnp"){
                // Try to open UPnP ports.
                if(m_pUPnP.IsSupported){
                    int rtpPublicPort  = rtpSession.LocalEP.RtpEP.Port;
                    int rtcpPublicPort = rtpSession.LocalEP.RtcpEP.Port;

                    try{
                        UPnP_NAT_Map[] maps = m_pUPnP.GetPortMappings();
                        while(true){
                            bool conficts = false;
                            // Check that some other application doesn't use that port.
                            foreach(UPnP_NAT_Map map in maps){
                                // Existing map entry conflicts.
                                if(Convert.ToInt32(map.ExternalPort) == rtpPublicPort || Convert.ToInt32(map.ExternalPort) == rtcpPublicPort){
                                    rtpPublicPort  += 2;
                                    rtcpPublicPort += 2;
                                    conficts = true;

                                    break;
                                }
                            }
                            if(!conficts){
                                break;
                            }
                        }

                        m_pUPnP.AddPortMapping(true,"LS RTP","UDP",null,rtpPublicPort,rtpSession.LocalEP.RtpEP,0);
                        m_pUPnP.AddPortMapping(true,"LS RTCP","UDP",null,rtcpPublicPort,rtpSession.LocalEP.RtcpEP,0);
                                        
                        IPAddress publicIP = m_pUPnP.GetExternalIPAddress();

                        rtpPublicEP  = new IPEndPoint(publicIP,rtpPublicPort);
                        rtcpPublicEP = new IPEndPoint(publicIP,rtcpPublicPort);

                        mediaStream.Tags.Add("upnp_rtp_map",new UPnP_NAT_Map(true,"UDP","",rtpPublicPort.ToString(),rtpSession.LocalEP.IP.ToString(),rtpSession.LocalEP.RtpEP.Port,"LS RTP",0));
                        mediaStream.Tags.Add("upnp_rtcp_map",new UPnP_NAT_Map(true,"UDP","",rtcpPublicPort.ToString(),rtpSession.LocalEP.IP.ToString(),rtpSession.LocalEP.RtcpEP.Port,"LS RTCP",0));                    
                    }
                    catch{                        
                    }
                }
            }
      
            if(rtpPublicEP != null && rtcpPublicEP != null){
                mediaStream.Port = rtpPublicEP.Port;
                if((rtpPublicEP.Port + 1) != rtcpPublicEP.Port){
                    // Remove old rport attribute, if any.
                    for(int i=0;i<mediaStream.Attributes.Count;i++){
                        if(string.Equals(mediaStream.Attributes[i].Name,"rport",StringComparison.InvariantCultureIgnoreCase)){
                            mediaStream.Attributes.RemoveAt(i);
                            i--;
                        }
                    }
                    mediaStream.Attributes.Add(new SDP_Attribute("rport",rtcpPublicEP.Port.ToString()));
                }
                mediaStream.Connection = new SDP_Connection("IN","IP4",rtpPublicEP.Address.ToString());

                return true;
            }
            
            return false;
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets UA settings.
        /// </summary>
        public Settings Settings
        {
            get{ return m_pSettings; }
        }
        
        /// <summary>
        /// Gets SIP UA.
        /// </summary>
        public SIP_UA SIP_UA
        {
            get{ return m_pUA; }
        }
        
        /// <summary>
        /// Gets audio codecs.
        /// </summary>
        public Dictionary<int,AudioCodec> AudioCodecs
        {
            get{ return m_pAudioCodecs; }
        }

        /// <summary>
        /// Gets call list.
        /// </summary>
        public wctrl_Call[] Calls
        {
            get{ return m_pCallTab_Calls_Calls.Calls; }
        }


        /// <summary>
        /// Gets if debug mode.
        /// </summary>
        internal bool IsDebug
        {
            get{ return m_IsDebug; }
        }

        /// <summary>
        /// Gets UPnP NAT client.
        /// </summary>
        internal UPnP_NAT_Client UPnP 
        {
            get{ return m_pUPnP; }
        }
                
        #endregion

    }
}
