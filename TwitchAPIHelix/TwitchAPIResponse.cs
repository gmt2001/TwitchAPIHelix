/*
 *  TwitchAPIHelix - A library to access the Twitch Helix API for .NET 4.7/Mono
 *  Copyright (C) 2018 gmt2001 - https://keybase.io/gmt2001
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published
 *  by the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Runtime.Serialization;

namespace TwitchAPIHelix
{
    /// <summary>
    /// Base class representing a response from the Twitch API
    /// </summary>
    [DataContract]
    public class TwitchAPIResponse
    {
        /// <summary>
        /// String representation of the type class name
        /// </summary>
        public string _typeof
        {
            get;
            internal set;
        }

        /// <summary>
        /// If true, the HTTP request completed successfully, though Twitch may have still returned an error (such as unauthorized)
        /// </summary>
        public bool _success
        {
            get;
            internal set;
        }

        /// <summary>
        /// String form of the <see cref="TwitchAPIHelix.request_type"/> used for this request
        /// </summary>
        public string _type
        {
            get;
            internal set;
        }

        /// <summary>
        /// The Uri queried
        /// </summary>
        public Uri _url
        {
            get;
            internal set;
        }

        /// <summary>
        /// Any post data sent with rhe request
        /// </summary>
        public string _post
        {
            get;
            internal set;
        }

        /// <summary>
        /// The HTTP status code returned by the server
        /// </summary>
        public int _httpCode
        {
            get;
            internal set;
        }

        /// <summary>
        /// If an exception was thrown during the request, the class name of the exception, otherwise an empty string
        /// </summary>
        public string _exception
        {
            get;
            internal set;
        }

        /// <summary>
        /// If an exception was thrown during the request, the message of the exception, otherwise an empty string
        /// </summary>
        public string _exceptionMessage
        {
            get;
            internal set;
        }

        /// <summary>
        /// The raw data returned by the server
        /// </summary>
        public string _rawData
        {
            get;
            internal set;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        internal TwitchAPIResponse()
        {
            _typeof = "TwitchAPIResponse";
            _success = false;
            _type = "";
            _url = new Uri("");
            _post = "";
            _httpCode = 0;
            _exception = "";
            _exceptionMessage = "";
            _rawData = "";
        }

        /// <summary>
        /// Copies the base class values from another TwitchAPIResponse
        /// </summary>
        /// <param name="res">The TwitchAPIResponse to copy from</param>
        internal void CopyResponse(TwitchAPIResponse res)
        {
            _success = res._success;
            _type = res._type;
            _url = res._url;
            _post = res._post;
            _httpCode = res._httpCode;
            _exception = res._exception;
            _exceptionMessage = res._exceptionMessage;
            _rawData = res._rawData;
        }
    }
}
