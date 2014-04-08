using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using LumiSoft.Net;
using LumiSoft.Net.SIP.Message;
using LumiSoft.Net.SIP.Stack;
using LumiSoft.Net.SIP.UA;
using LumiSoft.SIP.UA.Resources;

namespace LumiSoft.SIP.UA.UI
{
    /// <summary>
    /// Instant messageing window.
    /// </summary>
    public class wfrm_IM : Form
    {
        private SplitContainer m_pSplitter       = null;
        private ToolStrip      m_pToolbar        = null;
        private HtmlRichTextBox m_pAllText        = null;
        private Label          m_pLastMessage    = null;
        private ToolStrip      m_pWriteToolbar   = null;
        private TabControl     m_pWriteTab       = null;
        private RichTextBox    m_pSendText1      = null;
        private RichTextBox    m_pSendText2      = null;

        private wfrm_Main         m_pCommunicator = null;
        private Account           m_pAccount      = null;
        private SIP_t_NameAddress m_FromURI       = null;
        private DateTime          m_LastMessage;
        private bool              m_IsTyping      = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ua">Owner SIP communicator.</param>
        /// <param name="account">Local SIP account.</param>
        /// <param name="targetURI">IM sender(remote user) URI.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ua</b>,<b>account</b> or <b>targetURI</b> is null reference.</exception>
        internal wfrm_IM(wfrm_Main ua,Account account,SIP_t_NameAddress targetURI)
        {
            if(ua == null){
                throw new ArgumentNullException("ua");
            }
            if(account == null){
                throw new ArgumentNullException("account");
            }
            if(targetURI == null){
                throw new ArgumentNullException("targetURI");
            }

            m_pCommunicator = ua;
            m_pAccount      = account;
            m_FromURI       = targetURI;

            InitUI();
        }

        #region method InitUI

        /// <summary>
        /// Creates and initializes UI.
        /// </summary>
        private void InitUI()
        {
            this.ClientSize = new Size(300,400);
            this.Icon = ResManager.GetIcon("im.ico",new Size(16,16));
            this.Text = m_FromURI.ToStringValue();
            this.Activated += new EventHandler(wfrm_IM_Activated);

            m_pSplitter = new SplitContainer();
            m_pSplitter.Size = new Size(300,400);
            m_pSplitter.Dock = DockStyle.Fill;
            m_pSplitter.Orientation = Orientation.Horizontal;            
            m_pSplitter.SplitterWidth = 7;
            m_pSplitter.SplitterDistance = 300;
            m_pSplitter.Panel2MinSize = 80;

            m_pToolbar = new ToolStrip();
            m_pToolbar.Dock = DockStyle.None;
            m_pToolbar.Location = new Point(5,2);
            m_pToolbar.Renderer = new ToolBarRendererEx();
            m_pToolbar.GripStyle = ToolStripGripStyle.Hidden;
            ToolStripButton b = new ToolStripButton();            
            b.AutoSize = false;
            b.Size = new Size(28,28);
            b.ImageScaling = ToolStripItemImageScaling.None;
            b.Image = ResManager.GetIcon("blocked.ico",new Size(24,24)).ToBitmap();
            m_pToolbar.Items.Add(b);
            m_pSplitter.Panel1.Controls.Add(m_pToolbar);

            m_pAllText = new HtmlRichTextBox();
            m_pAllText.Size = new Size(290,245);
            m_pAllText.Location = new Point(5,35);
            m_pAllText.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            m_pAllText.ReadOnly = true;
            m_pAllText.BackColor = Color.White;
            m_pAllText.BorderStyle = BorderStyle.None;
            m_pSplitter.Panel1.Controls.Add(m_pAllText);

            m_pLastMessage = new Label();
            m_pLastMessage.Size = new Size(290,20);
            m_pLastMessage.Location = new Point(5,280);
            m_pLastMessage.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_pLastMessage.TextAlign = ContentAlignment.MiddleLeft;
            m_pLastMessage.ForeColor = Color.Gray;
            m_pLastMessage.BackColor = Color.White;
            m_pSplitter.Panel1.Controls.Add(m_pLastMessage);

            m_pWriteToolbar = new ToolStrip();            
            m_pWriteToolbar.Dock = DockStyle.None;
            m_pWriteToolbar.Location = new Point(100,1);
            m_pWriteToolbar.Renderer = new ToolBarRendererEx();
            m_pWriteToolbar.GripStyle = ToolStripGripStyle.Hidden;
            ToolStripComboBox writeToolbar_WriteTextType = new ToolStripComboBox();
            writeToolbar_WriteTextType.Name = "write_type";
            writeToolbar_WriteTextType.AutoSize = false;
            writeToolbar_WriteTextType.Width = 50;
            writeToolbar_WriteTextType.DropDownStyle = ComboBoxStyle.DropDownList;
            writeToolbar_WriteTextType.SelectedIndexChanged += new EventHandler(m_pWriteTextType_SelectedIndexChanged);
            writeToolbar_WriteTextType.Items.Add("html");
            writeToolbar_WriteTextType.Items.Add("plain");            
            m_pWriteToolbar.Items.Add(writeToolbar_WriteTextType); 
            m_pWriteToolbar.Items.Add(new ToolStripSeparator()); 
            ToolStripComboBox writeToolbar_fontSize = new ToolStripComboBox();
            writeToolbar_fontSize.Name = "font_size";
            writeToolbar_fontSize.AutoSize = false;
            writeToolbar_fontSize.Width = 35;
            writeToolbar_fontSize.DropDownStyle = ComboBoxStyle.DropDownList;
            writeToolbar_fontSize.SelectedIndexChanged += new EventHandler(WriteToolbar_FontSize_SelectedIndexChanged);
            writeToolbar_fontSize.Items.Add("8");
            writeToolbar_fontSize.Items.Add("10");
            writeToolbar_fontSize.Items.Add("12");
            writeToolbar_fontSize.Items.Add("14");
            writeToolbar_fontSize.Items.Add("16");
            m_pWriteToolbar.Items.Add(writeToolbar_fontSize);            
            ToolStripButton writeToolbar_bold = new ToolStripButton(ResManager.GetIcon("bold.ico",new Size(16,16)).ToBitmap());
            writeToolbar_bold.Name = "bold";
            writeToolbar_bold.Tag = "bold";
            m_pWriteToolbar.Items.Add(writeToolbar_bold);
            ToolStripButton writeToolbar_italic = new ToolStripButton(ResManager.GetIcon("italic.ico",new Size(16,16)).ToBitmap());
            writeToolbar_italic.Name = "italic";
            writeToolbar_italic.Tag = "italic";
            m_pWriteToolbar.Items.Add(writeToolbar_italic);
            ToolStripButton writeToolbar_underline = new ToolStripButton(ResManager.GetIcon("underline.ico",new Size(16,16)).ToBitmap());
            writeToolbar_underline.Name = "underline";
            writeToolbar_underline.Tag = "underline";
            m_pWriteToolbar.Items.Add(writeToolbar_underline);
            ToolStripButton writeToolbar_fontbackcolor = new ToolStripButton(CreateFontColorIcon(Color.Black));
            writeToolbar_fontbackcolor.Name = "fontbackcolor";
            writeToolbar_fontbackcolor.Tag = "fontbackcolor";
            m_pWriteToolbar.Items.Add(writeToolbar_fontbackcolor);
            m_pWriteToolbar.ItemClicked += new ToolStripItemClickedEventHandler(m_pWriteToolbar_ItemClicked);
            m_pSplitter.Panel2.Controls.Add(m_pWriteToolbar);
                        
            m_pWriteTab = new TabControl();
            m_pWriteTab.Size = new Size(303,88);
            m_pWriteTab.Location = new Point(0,7);
            m_pWriteTab.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            m_pWriteTab.SelectedIndexChanged += new EventHandler(m_pWriteTab_SelectedIndexChanged);
            // Write tab "1"
            TabPage write_tab_1 = new TabPage("1");
            m_pSendText1 = new RichTextBox();
            m_pSendText1.Dock = DockStyle.Fill;
            m_pSendText1.BorderStyle = BorderStyle.None;
            m_pSendText1.KeyPress += new KeyPressEventHandler(m_pSendText_KeyPress);
            m_pSendText1.SelectionChanged += new EventHandler(WriteText_SelectionChanged);
            write_tab_1.Controls.Add(m_pSendText1);
            m_pWriteTab.TabPages.Add(write_tab_1);
            // Write tab "2"
            TabPage write_tab_2 = new TabPage("2");
            m_pSendText2 = new RichTextBox();
            m_pSendText2.Dock = DockStyle.Fill;
            m_pSendText2.BorderStyle = BorderStyle.None;
            m_pSendText2.KeyPress += new KeyPressEventHandler(m_pSendText_KeyPress);
            m_pSendText2.SelectionChanged += new EventHandler(WriteText_SelectionChanged);
            write_tab_2.Controls.Add(m_pSendText2);
            m_pWriteTab.TabPages.Add(write_tab_2);
            m_pSplitter.Panel2.Controls.Add(m_pWriteTab);
                                                                                                                        
            this.Controls.Add(m_pSplitter);

            writeToolbar_fontSize.SelectedIndex = 0;
            writeToolbar_WriteTextType.SelectedIndex = 0;
        }
                                                                                                                                                
        #endregion


        #region Events Handling

        #region method m_pWriteTextType_SelectedIndexChanged

        /// <summary>
        /// This method is called when write text type has changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pWriteTextType_SelectedIndexChanged(object sender,EventArgs e)
        {
            if(m_pWriteToolbar.Items[0].Text == "html"){
                for(int i=1;i<m_pWriteToolbar.Items.Count;i++){
                    m_pWriteToolbar.Items[i].Enabled = true;
                }
            }
            else{
                for(int i=1;i<m_pWriteToolbar.Items.Count;i++){
                    m_pWriteToolbar.Items[i].Enabled = false;
                }
            }
        }

        #endregion

        #region method m_pWriteToolbar_ItemClicked

        /// <summary>
        /// This method is called when write toolbar item is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pWriteToolbar_ItemClicked(object sender,ToolStripItemClickedEventArgs e)
        {
            RichTextBox activeWriteText = null;
            if(m_pWriteTab.SelectedIndex == 0){
                activeWriteText = m_pSendText1;
            }
            else{
                activeWriteText = m_pSendText2;         
            }

            if(e.ClickedItem.Tag == null){
            }
            else if(e.ClickedItem.Tag.ToString() == "bold"){
                if(((ToolStripButton)m_pWriteToolbar.Items["bold"]).Checked){
                    ((ToolStripButton)m_pWriteToolbar.Items["bold"]).Checked = false;
                }
                else{
                    ((ToolStripButton)m_pWriteToolbar.Items["bold"]).Checked = true;
                }              
            }
            else if(e.ClickedItem.Tag.ToString() == "italic"){
                if(((ToolStripButton)m_pWriteToolbar.Items["italic"]).Checked){
                    ((ToolStripButton)m_pWriteToolbar.Items["italic"]).Checked = false;
                }
                else{
                    ((ToolStripButton)m_pWriteToolbar.Items["italic"]).Checked = true;                
                }               
            }
            else if(e.ClickedItem.Tag.ToString() == "underline"){
                if(((ToolStripButton)m_pWriteToolbar.Items["underline"]).Checked){
                    ((ToolStripButton)m_pWriteToolbar.Items["underline"]).Checked = false;
                }
                else{
                    ((ToolStripButton)m_pWriteToolbar.Items["underline"]).Checked = true;                
                }              
            }
            else if(e.ClickedItem.Tag.ToString() == "fontbackcolor"){
                ColorDialog dlg = new ColorDialog();
                if(dlg.ShowDialog(this) == DialogResult.OK){
                    activeWriteText.SelectionColor = dlg.Color;

                    ((ToolStripButton)m_pWriteToolbar.Items["fontbackcolor"]).Image = CreateFontColorIcon(dlg.Color);
                }                
            }

            FontStyle style = FontStyle.Regular;
            if(((ToolStripButton)m_pWriteToolbar.Items["bold"]).Checked){
                style |= FontStyle.Bold;
            }
            if(((ToolStripButton)m_pWriteToolbar.Items["italic"]).Checked){
                style |= FontStyle.Italic;
            }
            if(((ToolStripButton)m_pWriteToolbar.Items["underline"]).Checked){
                style |= FontStyle.Underline;
            }
            // FIX: fontname
            activeWriteText.SelectionFont = new Font(this.Font.Name,Convert.ToInt32(((ToolStripComboBox)m_pWriteToolbar.Items["font_size"]).Text),style);
        }

        #endregion

        #region method WriteToolbar_FontSize_SelectedIndexChanged

        /// <summary>
        /// This emthod is called when write font size changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void WriteToolbar_FontSize_SelectedIndexChanged(object sender,EventArgs e)
        {
            RichTextBox activeWriteText = null;
            if(m_pWriteTab.SelectedIndex == 0){
                activeWriteText = m_pSendText1;
            }
            else{
                activeWriteText = m_pSendText2;         
            }

            activeWriteText.SelectionFont = new Font(activeWriteText.SelectionFont.Name,Convert.ToInt32(((ToolStripComboBox)m_pWriteToolbar.Items["font_size"]).Text),activeWriteText.SelectionFont.Style);
            activeWriteText.Focus();
        }

        #endregion

        #region method m_pWriteTab_SelectedIndexChanged

        /// <summary>
        /// This method is called when write active tab has chaned.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void m_pWriteTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(m_pWriteTab.SelectedIndex == 0){
                m_pSendText1.Focus();
            }
            else{
                m_pSendText2.Focus();
            }

            WriteText_SelectionChanged(null,null);
        }

        #endregion

        #region method WriteText_SelectionChanged

        /// <summary>
        /// This method is called when wrtite text1 or text2 selection has changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void WriteText_SelectionChanged(object sender,EventArgs e)
        {
            RichTextBox activeWriteText = null;
            if(m_pWriteTab.SelectedIndex == 0){
                activeWriteText = m_pSendText1;
            }
            else{
                activeWriteText = m_pSendText2;         
            }
			
            //((ToolStripComboBox)m_pToolbar.Items[0]).Text  = m_pTextbox.SelectionFont.Name;
            ((ToolStripComboBox)m_pWriteToolbar.Items["font_size"]).Text    = ((int)activeWriteText.SelectionFont.Size).ToString();
            ((ToolStripButton)m_pWriteToolbar.Items["bold"]).Checked        = activeWriteText.SelectionFont.Bold;
            ((ToolStripButton)m_pWriteToolbar.Items["italic"]).Checked      = activeWriteText.SelectionFont.Italic;
            ((ToolStripButton)m_pWriteToolbar.Items["underline"]).Checked   = activeWriteText.SelectionFont.Underline;
            ((ToolStripButton)m_pWriteToolbar.Items["fontbackcolor"]).Image = CreateFontColorIcon(activeWriteText.SelectionColor);
        }

        #endregion

        #region method m_pSendText_KeyPress

        private void m_pSendText_KeyPress(object sender,KeyPressEventArgs e)
        {                                    
            if(e.KeyChar == 13 && Control.ModifierKeys == 0){
                SendActiveText();    
            }
            else if(!m_IsTyping){
                m_IsTyping = true;

                SendIsComposing("active");
            }
            else if(m_IsTyping && m_pSendText1.Text.Length == 0 && m_pSendText2.Text.Length == 0){
                m_IsTyping = false;

                SendIsComposing("idle");
            }
        }

        #endregion


        #region method wfrm_IM_Activated

        /// <summary>
        /// This method is called when IM form has activated.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void wfrm_IM_Activated(object sender,EventArgs e)
        {
            if(m_pWriteTab.SelectedIndex == 0){
                m_pSendText1.Focus();
            }
            else{
                m_pSendText2.Focus();
            }
        }

        #endregion


        #region method MessageSender_ResponseReceived

        /// <summary>
        /// This method is called when MESSAGE sending has completed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data.</param>
        private void MessageSender_ResponseReceived(object sender,SIP_ResponseReceivedEventArgs e)
        {
            // Eeport errors to user if any.
            if(e.Response.StatusCodeType != SIP_StatusCodeType.Success){
                this.Invoke(
                    new MethodInvoker(delegate(){
                        m_pAllText.SelectionColor = Color.Red;
                        m_pAllText.AppendText("Error : " + e.Response.StatusCode_ReasonPhrase + "\n ");
                    }),
                    null
                );
            }
        }

        #endregion

        #endregion


        #region method ProcessIM

        /// <summary>
        /// This method is called on thread pool thread when new IM message arrives.
        /// </summary>
        /// <param name="request">SIP MESSAGE request.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>request</b> is null reference.</exception>
        internal void ProcessIM(SIP_Request request)
        {
            if(request == null){
                throw new ArgumentNullException("request");
            }

            this.Invoke(
                new MethodInvoker(delegate(){
                    ProcessIM_UI_Thread(request);
                }),
                null
            );
        }

        private void ProcessIM_UI_Thread(SIP_Request request)
        {
            try{
                if(request.ContentType.ToLower() == "application/im-iscomposing+xml"){
                    /* RFC 3994.
                        draft-ietf-simple-iscomposing-01.txt
                        
                        <?xml version="1.0" encoding="UTF-8"?>
                        <isComposing xmlns="urn:ietf:params:xml:ns:im-iscomposing"
                            <state>active</state>
                            <contenttype>text/plain</contenttype>
                            <timeout>90</timeout>
                            <lastactivity>2003-01-27T10:43:00Z</lastactivity>
                        </isComposing>
                    */
                    DataSet ds = new DataSet();
                    ds.ReadXml(new MemoryStream(request.Data));

                    if(ds.Tables["isComposing"].Rows[0]["state"].ToString().ToLower() == "active"){
                        m_pLastMessage.Text = GetDisplayName(request.From.Address) + " is typing a message.";
                    }
                    else if(ds.Tables["isComposing"].Rows[0]["state"].ToString().ToLower() == "idle"){
                        // Show last received message date.
                        m_pLastMessage.Text = "Last message received: " + m_LastMessage.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
                else if(request.ContentType.ToLower() == "text/plain"){
                    m_LastMessage = DateTime.Now;

                    m_pAllText.SelectionColor = Color.Gray;
                    m_pAllText.SelectionFont = new Font(m_pAllText.SelectionFont,FontStyle.Bold);
                    m_pAllText.AppendText(GetDisplayName(request.From.Address) + ":\n ");
                    m_pAllText.SelectionFont = new Font(m_pAllText.SelectionFont,FontStyle.Regular);
                    m_pAllText.AppendText(Encoding.UTF8.GetString(request.Data) + "\n");

                    m_pLastMessage.Text = "Last message received: " + m_LastMessage.ToString("yyyy-MM-dd HH:mm:ss");

                    /*
                    try{
                        Win32.FlashWindow(this.Handle,true);
                    }
                    catch{
                    }*/

                    ScrollToEnd();
                }
                else if(request.ContentType.ToLower() == "text/html"){
                    m_LastMessage = DateTime.Now;

                    m_pAllText.SelectionColor = Color.Gray;
                    m_pAllText.SelectionFont = new Font(m_pAllText.SelectionFont,FontStyle.Bold);
                    m_pAllText.AppendText(GetDisplayName(request.From.Address) + ":\n ");
                    m_pAllText.SelectionFont = new Font(m_pAllText.SelectionFont,FontStyle.Regular);
                    m_pAllText.AddHTML(Encoding.UTF8.GetString(request.Data) + "\n");

                    m_pLastMessage.Text = "Last message received: " + m_LastMessage.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else{
                    // TODO: We should reject content types what we don't support.

                    //MessageBox.Show(request.ContentType);
                }                
            }
            catch(Exception x){
                MessageBox.Show(x.ToString());
            }
        }

        #endregion

        #region method SendActiveText

        /// <summary>
        /// Sends currently typed text from active wrie tab.
        /// </summary>
        private void SendActiveText()
        {
            try{
                string      text           = "";
                RichTextBox activeSendText = null;
                if(m_pWriteTab.SelectedIndex == 0){
                    activeSendText = m_pSendText1;
                    text = m_pSendText1.Text;
                }
                else{
                    activeSendText = m_pSendText2;
                    text = m_pSendText2.Text;                
                }

                if(text.Length == 0){
                    return;
                }
                m_IsTyping = false;

                SIP_Request request = m_pCommunicator.SIP_Stack.CreateRequest(SIP_Methods.MESSAGE,m_FromURI,new SIP_t_NameAddress(m_pAccount.DisplayName,AbsoluteUri.Parse("sip:" + m_pAccount.AOR)));
                if(m_pAccount.UseProxy){
                    request.Route.Add("<" + m_pAccount.ProxyServer + ";lr>");
                }
                if(m_pWriteToolbar.Items[0].Text == "html"){
                    request.ContentType = "text/html";
                    request.Data = Encoding.UTF8.GetBytes(RtfToHtml(activeSendText));
                }
                else{
                    request.ContentType = "text/plain";
                    request.Data = Encoding.UTF8.GetBytes(text);
                }        
             
                SIP_RequestSender sender = m_pCommunicator.SIP_Stack.CreateRequestSender(request);
                sender.ResponseReceived += new EventHandler<SIP_ResponseReceivedEventArgs>(MessageSender_ResponseReceived);
                sender.Start();

                m_pAllText.SelectionColor = Color.Green;
                m_pAllText.SelectionFont = new Font(m_pAllText.SelectionFont,FontStyle.Bold);
                m_pAllText.AppendText("this:\n ");
                m_pAllText.SelectionFont = new Font(m_pAllText.SelectionFont,FontStyle.Regular);           
                m_pAllText.SelectedRtf = activeSendText.Rtf;            
                ScrollToEnd();
            
                activeSendText.SelectAll();
                activeSendText.SelectedText = "";
                activeSendText.Focus();
            }
            catch(Exception x){
                MessageBox.Show(this,"Error: " + x.ToString(),"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }
                
        #endregion
                        

        #region method SendIsComposing

        /// <summary>
        /// Sends isComposing status message to remote party.
        /// </summary>
        /// <param name="state">Current composing state.</param>
        private void SendIsComposing(string state)
        {
            // RFC 3995.

            string statusMessage =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
            "<isComposing xmlns='urn:ietf:params:xml:ns:im-iscomposing'  xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>\r\n" +
            "   <state>" + state + "</state>\r\n" +
            "</isComposing>\r\n";

            SIP_Request request = m_pCommunicator.SIP_Stack.CreateRequest(SIP_Methods.MESSAGE,m_FromURI,new SIP_t_NameAddress(m_pAccount.DisplayName,AbsoluteUri.Parse("sip:" + m_pAccount.AOR)));
            request.ContentType = "application/im-iscomposing+xml";
            if(m_pAccount.UseProxy){
                request.Route.Add("<" + m_pAccount.ProxyServer + ";lr>");
            }
            request.Data = Encoding.UTF8.GetBytes(statusMessage);
            SIP_RequestSender sender = m_pCommunicator.SIP_Stack.CreateRequestSender(request);
            sender.Start();
        }

        #endregion

        #region method ScrollToEnd

        /// <summary>
        /// Scrolls to the end of conversation text,
        /// </summary>
        private void ScrollToEnd()
        {
            Message msg = Message.Create(m_pAllText.Handle,0x0115,new IntPtr(0x7),new IntPtr(0));
            Control.ReflectMessage(m_pAllText.Handle,ref msg).ToString();
            m_pAllText.ScrollToCaret();
        }

        #endregion

        #region method GetDisplayName

        /// <summary>
        /// Gets suitable display name.
        /// </summary>
        /// <param name="address">SIP address.</param>
        /// <returns>Returns dispaly name.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>address</b> is null reference.</exception>
        private string GetDisplayName(SIP_t_NameAddress address)
        {
            if(address == null){
                throw new ArgumentNullException("address");
            }

            if(address.DisplayName == string.Empty){
                return address.Uri.Value;
            }
            else{
                return address.DisplayName;
            }
        }

        #endregion

        #region method RtfToHtml

        /// <summary>
        /// Converts RTF text to HTML text.
        /// </summary>
        /// <param name="textbox">Textbox text to convert to html.</param>
        /// <returns>Returns html text.</returns>
        private string RtfToHtml(RichTextBox textbox)
        {
            if(textbox == null){
                throw new ArgumentNullException("textbox");
            }

            StringBuilder retVal = new StringBuilder();
            retVal.Append("<html>\r\n");

            // Go to text start.
            textbox.SelectionStart  = 0;
            textbox.SelectionLength = 1;

            Font  currentFont           = textbox.SelectionFont;
            Color currentSelectionColor = textbox.SelectionColor;
            Color currentBackColor      = textbox.SelectionBackColor;

            int numberOfSpans = 0;
            int startPos = 0;
            while(textbox.Text.Length > textbox.SelectionStart){  
                textbox.SelectionStart++;
                textbox.SelectionLength = 1;
                                
                // Font style or size or color or back color changed
                if(textbox.Text.Length == textbox.SelectionStart || (currentFont.Name != textbox.SelectionFont.Name || currentFont.Size != textbox.SelectionFont.Size || currentFont.Style != textbox.SelectionFont.Style || currentSelectionColor != textbox.SelectionColor || currentBackColor != textbox.SelectionBackColor)){
                    string currentTextBlock = textbox.Text.Substring(startPos,textbox.SelectionStart - startPos);
       
                    //--- Construct text bloth html -----------------------------------------------------------------//
                    // Make colors to html color syntax: #hex(r)hex(g)hex(b)
                    string htmlSelectionColor = "#" + currentSelectionColor.R.ToString("X2") + currentSelectionColor.G.ToString("X2") + currentSelectionColor.B.ToString("X2");
                    string htmlBackColor      = "#" + currentBackColor.R.ToString("X2") + currentBackColor.G.ToString("X2") + currentBackColor.B.ToString("X2");
                    string textStyleStartTags = "";
                    string textStyleEndTags   = "";
                    if(currentFont.Bold){
                        textStyleStartTags += "<b>";
                        textStyleEndTags   += "</b>";
                    }
                    if(currentFont.Italic){
                        textStyleStartTags += "<i>";
                        textStyleEndTags   += "</i>";
                    }
                    if(currentFont.Underline){
                        textStyleStartTags += "<u>";
                        textStyleEndTags   += "</u>";
                    }           
                    retVal.Append("<span\n style=\"color:" + htmlSelectionColor + "; font-size:" + currentFont.Size + "pt; font-family:" + currentFont.FontFamily.Name + "; background-color:" + htmlBackColor + ";\">" + textStyleStartTags + currentTextBlock.Replace("\n","<br/>") + textStyleEndTags);
                    //-----------------------------------------------------------------------------------------------//

                    startPos              = textbox.SelectionStart;
                    currentFont           = textbox.SelectionFont;
                    currentSelectionColor = textbox.SelectionColor;
                    currentBackColor      = textbox.SelectionBackColor;
                    numberOfSpans++;
                }
            }

            for(int i=0;i<numberOfSpans;i++){
                retVal.Append("</span>");
            }

            retVal.Append("\r\n</html>\r\n");

            return retVal.ToString();
        }

        #endregion

        #region method CreateFontColorIcon

        /// <summary>
        /// Creates font color icon for specified color.
        /// </summary>
        /// <param name="color">Color for what to create icon.</param>
        /// <returns></returns>
        private Bitmap CreateFontColorIcon(Color color)
        {
            Bitmap bmp = ResManager.GetIcon("fontcolor.ico").ToBitmap();
            for(int x=0;x<bmp.Width;x++){
                for(int y=12;y<bmp.Height;y++){
                   bmp.SetPixel(x,y,color);
                }
            }

            return bmp;
        }

        #endregion

        #region method CreateFontBackColorIcon

        /// <summary>
        /// Creates font back ground color icon for specified color.
        /// </summary>
        /// <param name="color">Color for what to create icon.</param>
        /// <returns></returns>
        private Bitmap CreateFontBackColorIcon(Color color)
        {
            Bitmap bmp = ResManager.GetIcon("fontbackcolor.ico").ToBitmap();
            for(int x=0;x<bmp.Width;x++){
                for(int y=12;y<bmp.Height;y++){
                   bmp.SetPixel(x,y,color);
                }
            }

            return bmp;
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets IM remote user URI.
        /// </summary>
        public SIP_t_NameAddress TargetURI
        {
            get{ return m_FromURI; }
        }

        #endregion

    }
}
