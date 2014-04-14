﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.SIP.Message
{
    /// <summary>
    /// Implements SIP "DiffieHellman" value.
    /// </summary>
    /// <remarks>
    /// <code>
    ///     DiffieHellman = word
    /// </code>
    /// </remarks>
    public class SIP_t_DiffieHellman : SIP_t_Value
    {
        private string m_DiffieHellman = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SIP_t_DiffieHellman()
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">SIP_t_DiffieHellman + parameters value.</param>
        public SIP_t_DiffieHellman(string value)
        {
            Parse(value);
        }


        #region method Parse

        /// <summary>
        /// Parses "DiffieHellman" from specified value.
        /// </summary>
        /// <param name="value">SIP "DiffieHellman" value.</param>
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
        /// Parses "DiffieHellman" from specified reader.
        /// </summary>
        /// <param name="reader">Reader from where to parse.</param>
        /// <exception cref="ArgumentNullException">Raised when <b>reader</b> is null.</exception>
        /// <exception cref="SIP_ParseException">Raised when invalid SIP message.</exception>
        public override void Parse(StringReader reader)
        {
            // callid = word [ "@" word ]

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Get Method
            string word = reader.ReadWord();
            if (word == null)
            {
                throw new SIP_ParseException("Invalid 'DiffieHellman' value");
            }
            m_DiffieHellman = word;
        }

        #endregion

        #region method ToStringValue

        /// <summary>
        /// Converts this to valid "DiffieHellman" value.
        /// </summary>
        /// <returns>Returns "DiffieHellman" value.</returns>
        public override string ToStringValue()
        {
            return m_DiffieHellman;
        }

        #endregion


        #region Properties Implementation

        /// <summary>
        /// Gets or sets DiffieHellman.
        /// </summary>
        public string Value
        {
            get { return m_DiffieHellman; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Property DiffieHellman value may not be null or empty !");
                }

                m_DiffieHellman = value;
            }
        }

        #endregion

    }
}
