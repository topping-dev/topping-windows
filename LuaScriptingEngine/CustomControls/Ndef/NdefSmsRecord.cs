﻿/****************************************************************************
**
** Copyright (C) 2012 Andreas Jakl.
** All rights reserved.
**
** Extension to the NDEF handling classes.
**
** Created by Andreas Jakl (2012).
** More information: http://ndef.codeplex.com/
**
** GNU Lesser General Public License Usage
** This file may be used under the terms of the GNU Lesser General Public
** License version 2.1 as published by the Free Software Foundation and
** appearing in the file LICENSE.LGPL included in the packaging of this
** file. Please review the following information to ensure the GNU Lesser
** General Public License version 2.1 requirements will be met:
** http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html.
**
** GNU General Public License Usage
** Alternatively, this file may be used under the terms of the GNU General
** Public License version 3.0 as published by the Free Software Foundation
** and appearing in the file LICENSE.GPL included in the packaging of this
** file. Please review the following information to ensure the GNU General
** Public License version 3.0 requirements will be met:
** http://www.gnu.org/copyleft/gpl.html.
**
****************************************************************************/

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Convenience class for formatting SMS information into
    /// an NDEF record, depending on added info either URI or Smart Poster.
    /// </summary>
    /// <remarks>
    /// Tapping a tag with SMS information stored on it should trigger
    /// a dialog in the phone to send the proposed SMS. This can for
    /// example be used to interact with information services that then
    /// reply by sending more info per SMS, or for payment services that
    /// send a premium SMS to purchase items or content.
    /// 
    /// Add the recipient number and the SMS message body to the class,
    /// which takes care of properly encoding the information into the
    /// correct URI.
    /// 
    /// As this class is based on the Smart URI base class, the
    /// payload is formatted as a URI record initially. When first
    /// adding Smart Poster information (like a title), the payload
    /// instantly transforms into a Smart Poster.
    /// </remarks>
    public class NdefSmsRecord : NdefSmartUriRecord
    {
        /// <summary>
        /// URI scheme in use for this record.
        /// </summary>
        private const string SmsScheme = "sms:";

        private string _smsBody;
        private string _smsNumber;
        
        /// <summary>
        /// The body of the SMS message.
        /// </summary>
        /// <remarks>
        /// Note: the URI content has to be encoded in UTF-8, which should
        /// be done by the URI class. No further transformation
        /// is necessary for the SMS use case (e.g., to 7-bit US-ASCII, as
        /// would be recommended for Internet URLs).
        /// This class will take care of Uri Escaping the body text through
        /// System.Uri.EscapeDataString().
        /// </remarks>
        public string SmsBody
        {
            get { return _smsBody; }
            set
            {
                _smsBody = value;
                UpdatePayload();
            }
        }

        /// <summary>
        /// The number the reading phone is supposed to send the short message to.
        /// Recommended to store in international format, e.g., +431234...
        /// </summary>
        public string SmsNumber
        {
            get { return _smsNumber; }
            set
            {
                _smsNumber = value;
                UpdatePayload();
            }
        }

        /// <summary>
        /// Create an empty Sms record. You need to set the number and body
        /// to create a URI and make this record valid.
        /// </summary>
        public NdefSmsRecord()
        {
        }

        /// <summary>
        /// Create a SMS record based on another SMS record, or Smart Poster / URI
        /// record that have a Uri set that corresponds to the sms: URI scheme.
        /// </summary>
        /// <param name="other">Other record to copy the data from.</param>
        public NdefSmsRecord(NdefRecord other)
            : base(other)
        {
            ParseUriToData(Uri);
        }

        /// <summary>
        /// Deletes any details currently stored in the SMS record 
        /// and re-initializes them by parsing the contents of the provided URI.
        /// </summary>
        /// <remarks>The URI has to be formatted according to the sms: URI scheme,
        /// and include both the number and the message body.</remarks>
        private void ParseUriToData(string uri)
        {
            // Extract product name and serial number from the payload
            var pattern = new Regex(@"sms:(?<smsNumber>.*)\?body=(?<smsBody>.*)");
            var match = pattern.Match(uri);
            // Assign extracted data to member variables
            _smsNumber = match.Groups["smsNumber"].Value;
            _smsBody = System.Uri.UnescapeDataString(match.Groups["smsBody"].Value);
            UpdatePayload();
        }

        /// <summary>
        /// Format the URI of the SmartUri base class.
        /// </summary>
        private void UpdatePayload()
        {
            if (SmsNumber != null)
                Uri = SmsScheme + SmsNumber + (!string.IsNullOrEmpty(SmsBody) ? "?body=" + System.Uri.EscapeDataString(SmsBody) : String.Empty);
        }

        /// <summary>
        /// Checks if the record sent via the parameter is indeed an Sms record.
        /// Checks the type and type name format and if the URI starts with the correct scheme.
        /// </summary>
        /// <param name="record">Record to check.</param>
        /// <returns>True if the record has the correct type, type name format and payload
        /// to be an Sms record, false if it's a different record.</returns>
        public new static bool IsRecordType(NdefRecord record)
        {
            if (record.Type == null) return false;
            if (record.TypeNameFormat == TypeNameFormatType.NfcRtd && record.Payload != null)
            {
                if (record.Type.SequenceEqual(NdefUriRecord.UriType))
                {
                    var testRecord = new NdefUriRecord(record);
                    return testRecord.Uri.StartsWith(SmsScheme);
                }
                if (record.Type.SequenceEqual(SmartPosterType))
                {
                    var testRecord = new NdefSpRecord(record);
                    return testRecord.Uri.StartsWith(SmsScheme);
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the contents of the record are valid; throws an exception if
        /// a problem is found, containing a textual description of the issue.
        /// </summary>
        /// <exception cref="NdefException">Thrown if no valid NDEF record can be
        /// created based on the record's current contents. The exception message 
        /// contains further details about the issue.</exception>
        /// <returns>True if the record contents are valid, or throws an exception
        /// if an issue is found.</returns>
        public override bool CheckIfValid()
        {
            // First check the basics
            if (!base.CheckIfValid()) return false;

            // Check specific content of this record
            if (string.IsNullOrEmpty(SmsNumber))
            {
                throw new NdefException("NdefExceptionMessages.ExSmsNumberEmpty");
            }
            if (string.IsNullOrEmpty(SmsBody))
            {
                throw new NdefException("NdefExceptionMessages.ExSmsBodyEmpty");
            }
            return true;
        }
    }
}
