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
using System.Collections.Generic;
using System.Globalization;

namespace NdefLibrary.Ndef
{
    /// <summary>
    /// Store longitude and latitude on a tag, to allow the user
    /// to view a map when tapping the tag.
    /// </summary>
    /// <remarks>
    /// Geo tags are not standardized by the NFC forum, therefore,
    /// this class supports three different types of writing the location
    /// to a tag.
    /// 
    /// * GeoUri: write URI based on the "geo:" URI scheme, as specified
    /// by RFC5870, available at: http://geouri.org/
    /// 
    /// * BingMaps: Uses the URI scheme defined by the Maps application
    /// on Windows 8.
    /// 
    /// * DriveTo / WalkTo: URI schemes supported by Windows Phone 8 and
    /// used in apps to launch an installed navigation app to navigate
    /// to a specified position. An app to handle DriveTo request should
    /// be present by default on all WP8 phones; WalkTo not necessarily.
    /// 
    /// * NokiaMapsUri: write URI based on a Nokia Maps link, following the
    /// "http://m.ovi.me/?c=..." scheme of the Nokia/Ovi Maps Rendering API.
    /// Depending on the target device, the phone / web service should then
    /// redirect to the best maps representation.
    /// On Symbian, the phone will launch the Nokia Maps client. On a
    /// desktop computer, the full Nokia Maps web experience will open.
    /// On other phones, the HTML 5 client may be available.
    /// 
    /// * WebRedirect: uses the web service at NfcInteractor.com to
    /// check the OS of the phone, and then redirect to the best way
    /// of showing maps to the user.
    /// Note the limitations and terms of use of the web service. For
    /// real world deployment, outside of development and testing, it's
    /// recommended to host the script on your own web server.
    /// Find more information at nfcinteractor.com.
    /// 
    /// As this class is based on the Smart URI base class, the
    /// payload is formatted as a URI record initially. When first
    /// adding Smart Poster information (like a title), the payload
    /// instantly transforms into a Smart Poster.
    /// </remarks>
    public class NdefGeoRecord : NdefSmartUriRecord
    {
        /// <summary>
        /// Format of the URI on the geo tag.
        /// </summary>
        public enum NfcGeoType
        {
            /// <summary>
            /// Geo URI scheme, as defined in RFC 5870 (http://tools.ietf.org/html/rfc5870).
            /// </summary>
            GeoUri = 0,
            /// <summary>
            /// Bing Maps URI scheme, used for Maps on Windows 8 (http://msdn.microsoft.com/en-us/library/windows/apps/jj635237.aspx)
            /// </summary>
            BingMaps,
            /// <summary>
            /// Nokia Maps HTTP URL to show maps in the browser.
            /// </summary>
            NokiaMapsUri,
            /// <summary>
            /// Web redirection script that uses the appropriate URI format depending on the browser's user agent.
            /// </summary>
            WebRedirect,
            /// <summary>
            /// Drive-to URI scheme for Windows Phone 8 (http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj710324%28v=vs.105%29.aspx)
            /// </summary>
            MsDriveTo,
            /// <summary>
            /// Walk-to URI scheme for Windows Phone 8 (http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj710324%28v=vs.105%29.aspx)
            /// </summary>
            MsWalkTo,
        }

        /// <summary>
        /// Default URIs for the different formats to store the geo tag.
        /// </summary>
        /// <remarks>
        /// URI that will be used for this record. Parameter {0} will be replaced with
        /// the latitude, {1} with the longitude.
        /// </remarks>
        private static readonly Dictionary<NfcGeoType, string> GeoTagTypeUris = new Dictionary<NfcGeoType, string>
                                                                                    {
                                                                                        { NfcGeoType.GeoUri, "geo:{0},{1}" },
                                                                                        { NfcGeoType.BingMaps, "bingmaps:?cp={0}~{1}" },
                                                                                        { NfcGeoType.NokiaMapsUri, "http://m.ovi.me/?c={0},{1}" },
                                                                                        { NfcGeoType.WebRedirect, "http://NfcInteractor.com/m?c={0},{1}" },
                                                                                        { NfcGeoType.MsDriveTo, "ms-drive-to:?destination.latitude={0}&destination.longitude={1}"},
                                                                                        { NfcGeoType.MsWalkTo, "ms-walk-to:?destination.latitude={0}&destination.longitude={1}"},
                                                                                    };

        private GeoCoordinate _coordinate;
        /// <summary>
        /// Geo coordinate that is encoded in the Geo URI. Only Latitude and Longitude is used.
        /// </summary>
        /// <remarks>Can't expose the Coordinate to public, as we wouldn't be notified
        /// about changes to its longitude or latitude properties.</remarks>
        private GeoCoordinate Coordinate
        {
            get { return _coordinate ?? (_coordinate = new GeoCoordinate()); }
            set
            {
                _coordinate = value;
                UpdatePayload();
            }
        }

        /// <summary>
        /// Longitude of the coordinate to encode in the Geo URI.
        /// </summary>
        public double Longitude
        {
            get { return Coordinate.Longitude; }
            set { Coordinate.Longitude = value; UpdatePayload(); }
        }

        /// <summary>
        /// Latitude of the coordinate to encode in the Geo URI.
        /// </summary>
        public double Latitude
        {
            get { return Coordinate.Latitude; }
            set { Coordinate.Latitude = value; UpdatePayload(); }
        }

        private NfcGeoType _geoType;
        /// <summary>
        /// Format to use for encoding the coordinate into a URI.
        /// </summary>
        public NfcGeoType GeoType
        {
            get { return _geoType; }
            set
            {
                _geoType = value;
                UpdatePayload();
            }
        }

        /// <summary>
        /// Format the URI of the SmartUri base class.
        /// </summary>
        private void UpdatePayload()
        {
            if (Coordinate == null) return;
            // Make sure we always use the "en" culture to have "." as the decimal separator.
            var culture = new CultureInfo("en");
            Uri = string.Format((IFormatProvider)culture.GetFormat(typeof(NumberFormatInfo)), GeoTagTypeUris[GeoType], Latitude, Longitude);
        }
    }
}
