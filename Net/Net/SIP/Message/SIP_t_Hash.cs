using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace LumiSoft.Net.SIP.Message
{
    /// <summary>
    /// Implements SIP_t_Hash + parameters value.
    /// </summary>
    public class SIP_t_Hash : SIP_t_ValueWithParams
    {
        private string m_Hash = "";
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_Hash()
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP_t_NameAddress + parameters value.</param>
        public SIP_t_Hash(string value)
        {
            Parse(value);
        }


        #region method Parse

        /// <summary>
        /// Parses this from specified value.
        /// </summary>
        /// <param name="value">Hash + params value.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>value</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public void Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Parse(new StringReader(value));
        }

        /// <summary>
        /// Parses this from Hash param string.
        /// </summary>
        /// <param name="reader">Reader what contains Hash param string.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            m_Hash = reader.QuotedReadToDelimiter(new char[] { ';', ',' }, false);

            // Parse parameters.
            this.ParseParameters(reader);
        }

        #endregion

        #region method ToStringValue

        /// <summary>
        /// Converts this to valid value string.
        /// </summary>
        /// <returns></returns>
        public override string ToStringValue()
        {
            StringBuilder retVal = new StringBuilder();

            // Add address
            retVal.Append(m_Hash);

            // Add parameters
            foreach (SIP_Parameter parameter in this.Parameters)
            {
                if (parameter.Value != null)
                {
                    retVal.Append(";" + parameter.Name + "=" + parameter.Value);
                }
                else
                {
                    retVal.Append(";" + parameter.Name);
                }
            }

            return retVal.ToString();
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets or sets Hash.
        /// </summary>
        public string Value
        {
            get { return m_Hash; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property Hash value may not be null or empty !");
                }

                m_Hash = value;
            }
        }

        #endregion

    }
}
