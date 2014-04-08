using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Data;

using LumiSoft.Net;

namespace LumiSoft.SIP.UA
{
    /// <summary>
    /// This class holds SIP UA settings.
    /// </summary>
    public class Settings
    {
        private IPBindInfo[]            m_pBindings    = null;
        private bool                    m_UseProxy     = false;
        private string                  m_ProxyServer  = "";
        private List<NetworkCredential> m_pCredentials = null;
        private List<Account>           m_pAccounts    = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Settings()
        {
            m_pBindings = new IPBindInfo[0];
            m_pCredentials = new List<NetworkCredential>();
            m_pAccounts = new List<Account>();
        }


        #region method Load

        /// <summary>
        /// Loads settings from the specified file.
        /// </summary>
        /// <param name="file">File name with optional path.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>file</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Load(string file)
        {
            if(file == null){
                throw new ArgumentNullException("file");
            }
            if(file == string.Empty){
                throw new ArgumentException("Argument 'file' value must be specified.");
            }

            // TODO:
            DataSet dsSettings = new DataSet("Settings");
            DataTable dtBindings = dsSettings.Tables.Add("Bindings");
            dtBindings.Columns.Add("Protocol");
            dtBindings.Columns.Add("IPAddress");
            dtBindings.Columns.Add("Port");
            DataTable dtCredentials = dsSettings.Tables.Add("Credentials");
            dtCredentials.Columns.Add("Domain");
            dtCredentials.Columns.Add("UserName");
            dtCredentials.Columns.Add("Password");
            DataTable dtAccounts = dsSettings.Tables.Add("Accounts");
            dtAccounts.Columns.Add("DisplayName");
            dtAccounts.Columns.Add("UserName");
            dtAccounts.Columns.Add("AOR");
            dtAccounts.Columns.Add("UseProxy");
            dtAccounts.Columns.Add("ProxyServer");
            dtAccounts.Columns.Add("Register");
            dtAccounts.Columns.Add("RegistrarServer");
            dtAccounts.Columns.Add("RegisterInterval");
            dtAccounts.Columns.Add("StunServer");
            dsSettings.ReadXml(file);
                 
            List<IPBindInfo> bindings = new List<IPBindInfo>();
            foreach(DataRow dr in dtBindings.Rows){
                bindings.Add(new IPBindInfo(
                    "",
                    (BindInfoProtocol)Enum.Parse(typeof(BindInfoProtocol),DataUtils.GetValueString(dr,"Protocol")),
                    IPAddress.Parse(DataUtils.GetValueString(dr,"IPAddress")),
                    DataUtils.GetValueInt(dr,"Port")
                ));
            }
            m_pBindings = bindings.ToArray();
            foreach(DataRow dr in dtCredentials.Rows){
                m_pCredentials.Add(new NetworkCredential(
                    DataUtils.GetValueString(dr,"UserName"),
                    DataUtils.GetValueString(dr,"Password"),
                    DataUtils.GetValueString(dr,"Domain")
                ));
            }
            foreach(DataRow dr in dtAccounts.Rows){
                m_pAccounts.Add(new Account(
                    DataUtils.GetValueString(dr,"DisplayName"),
                    DataUtils.GetValueString(dr,"UserName"),
                    DataUtils.GetValueString(dr,"AOR"),
                    DataUtils.GetValueBool(dr,"UseProxy"),
                    DataUtils.GetValueString(dr,"ProxyServer"),
                    DataUtils.GetValueBool(dr,"Register"),
                    DataUtils.GetValueString(dr,"RegistrarServer"),
                    DataUtils.GetValueInt(dr,"RegisterInterval",3600),
                    DataUtils.GetValueString(dr,"StunServer")
                ));
            }
        }

        #endregion

        #region method Save

        /// <summary>
        /// Saves settings to the specified file.
        /// </summary>
        /// <param name="file">File name with optional path.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>file</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void Save(string file)
        {
            if(file == null){
                throw new ArgumentNullException("file");
            }
            if(file == string.Empty){
                throw new ArgumentException("Argument 'file' value must be specified.");
            }

            // TODO:
            DataSet dsSettings = new DataSet("dsSettings");
            DataTable dtGeneral = dsSettings.Tables.Add("General");
            DataTable dtCredentials = dsSettings.Tables.Add("Credentials");
            dtCredentials.Columns.Add("Domain");
            dtCredentials.Columns.Add("UserName");
            dtCredentials.Columns.Add("Password");
            foreach(NetworkCredential credential in m_pCredentials){
                DataRow dr = dtCredentials.NewRow();
                dr["Domain"]   = credential.Domain;
                dr["UserName"] = credential.UserName;
                dr["Password"] = credential.Password;
                dtCredentials.Rows.Add(dr);
            }
            DataTable dtAccounts = dsSettings.Tables.Add("Accounts");
            dtAccounts.Columns.Add("DisplayName");
            dtAccounts.Columns.Add("AOR");
            dtAccounts.Columns.Add("UseProxy");
            dtAccounts.Columns.Add("ProxyServer");
            dtAccounts.Columns.Add("Register");
            dtAccounts.Columns.Add("RegistrarServer");
            foreach(Account account in m_pAccounts){
                DataRow dr = dtAccounts.NewRow();
                dr["DisplayName"]     = account.DisplayName;
                dr["AOR"]             = account.AOR;
                dr["UseProxy"]        = account.UseProxy;
                dr["ProxyServer"]     = account.ProxyServer;
                dr["Register"]        = account.Register;
                dr["RegistrarServer"] = account.RegistrarServer;
                dtAccounts.Rows.Add(dr);
            }

            dsSettings.WriteXml(file);
        }

        #endregion


        #region Properties implementation

        /// <summary>
        /// Gets or sets listening info.
        /// </summary>
        public IPBindInfo[] Bindings
        {
            get{ return m_pBindings; }

            set{ m_pBindings = value; }
        }

        /// <summary>
        /// Gets or sets if outbound proxy is used.
        /// </summary>
        public bool UseProxy
        {
            get{ return m_UseProxy; }

            set{ m_UseProxy = value; }
        }

        /// <summary>
        /// Gets or sets proxy server name or IP address.
        /// </summary>
        public string ProxyServer
        {
            get{ return m_ProxyServer; }

            set{ m_ProxyServer = value; }
        }

        /// <summary>
        /// Gets credentials collection.
        /// </summary>
        public List<NetworkCredential> Credentials
        {
            get{ return m_pCredentials; }
        }

        /// <summary>
        /// Gets accounts collection.
        /// </summary>
        public List<Account> Accounts
        {
            get{ return m_pAccounts; }
        }

        #endregion

    }
}
