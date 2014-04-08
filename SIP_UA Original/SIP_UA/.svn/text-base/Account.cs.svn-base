using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LumiSoft.SIP.UA
{
    /// <summary>
    /// Thsi class represents SIP UA account.
    /// </summary>
    public class Account
    {
        private string m_DisplayName      = "";
        private string m_UserName         = "";
        private string m_AOR              = "";
        private bool   m_UseProxy         = false;
        private string m_ProxyServer      = "";
        private bool   m_Register         = false;
        private string m_RegistrarServer  = "";
        private int    m_RegisterInterval = 300;
        private string m_StunServer       = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="displayName">Display name.</param>
        /// <param name="userName">User login name.</param>
        /// <param name="aor">Address of record. For example: user@domain.com.</param>
        /// <param name="useProxy">Specifies if proxy server is used for the specified account.</param>
        /// <param name="proxyServer">Proxy server name or IP address.</param>
        /// <param name="register">Specifies if account registers to registrar server.</param>
        /// <param name="registrarServer">Registrar server name or IP address.</param>
        /// <param name="registerInterval">Regiser refreshing interval.</param>
        /// <param name="stunServer">STUN server name or IP address.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>userName</b>, <b>aor</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public Account(string displayName,string userName,string aor,bool useProxy,string proxyServer,bool register,string registrarServer,int registerInterval,string stunServer)
        {            
            this.DisplayName      = displayName;
            this.UserName         = userName;
            this.AOR              = aor;
            this.UseProxy         = useProxy;
            this.ProxyServer      = proxyServer;
            this.Register         = register;
            this.RegistrarServer  = registrarServer;
            this.RegisterInterval = registerInterval;
            this.StunServer       = stunServer;
        }


        #region Properties implementation

        /// <summary>
        /// Gets or sets display name.
        /// </summary>
        public string DisplayName
        {
            get{ return m_DisplayName; }

            set{
                if(value == null){
                    value = "";
                }

                m_DisplayName = value;
            }
        }

        /// <summary>
        /// Gets user login name.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value passed.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid value is passed.</exception>
        public string UserName
        {
            get{ return m_UserName; }

            set{
                if(value == null){
                    throw new ArgumentNullException("UserName");
                }
                if(value == string.Empty){
                    throw new ArgumentException("Property 'UserName' value must be specified.");
                }

                m_UserName = value;
            }
        }

        /// <summary>
        /// Gets or sets address of record. For example: user@domain.com.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value passed.</exception>
        /// <exception cref="ArgumentException">Is raised when invalid value is passed.</exception>
        public string AOR
        {
            get{ return m_AOR; }

            set{
                if(value == null){
                    throw new ArgumentNullException("AOR");
                }
                if(value == string.Empty){
                    throw new ArgumentException("Property 'AOR' value must be specified.");
                }

                m_AOR = value;
            }
        }

        /// <summary>
        /// Gets or sets if proxy server is used for the specified account.
        /// </summary>
        public bool UseProxy
        {
            get{ return m_UseProxy; }

            set{ m_UseProxy = value; }
        }
        
        /// <summary>
        /// Gets or stes proxy server name or IP address.
        /// </summary>
        public string ProxyServer
        {
            get{ return m_ProxyServer; }

            set{
                if(value == null){
                    value = "";
                }

                m_ProxyServer = value;
            }
        }

        /// <summary>
        /// Gets or sets if account registers to registrar server.
        /// </summary>
        public bool Register
        {
            get{ return m_Register; }

            set{ m_Register = value; }
        }

        /// <summary>
        /// Gets or sets registrar server name or IP address.
        /// </summary>
        public string RegistrarServer
        {
            get{ return m_RegistrarServer; }

            set{
                if(value == null){
                    value = "";
                }

                m_RegistrarServer = value;
            }
        }

        /// <summary>
        /// Gets or sets regiser refreshing interval.
        /// </summary>
        public int RegisterInterval
        {
            get{ return m_RegisterInterval; }

            set{
                m_RegisterInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets STUN server name or IP address.
        /// </summary>
        public string StunServer
        {
            get{ return m_StunServer; }

            set{
                if(value == null){
                    value = "";
                }

                m_StunServer = value;
            }
        }

        #endregion

    }
}
