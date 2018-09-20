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

namespace TwitchAPIHelix
{
    /// <summary>
    /// Executes requests using the Twitch Helix API
    /// </summary>
    public class TwitchAPIHelix
    {
        /// <summary>
        /// Either a valid Client ID or a valid OAuth token from Twitch
        /// </summary>
        private string clientidOrOauth;
        /// <summary>
        /// If true, clientidOrOauth is an OAuth token
        /// </summary>
        private bool isOauth;
        /// <summary>
        /// User agent for requests
        /// </summary>
        private static readonly string user_agent = "TwitchAPIHelix/1.0";
        /// <summary>
        /// Request timeout
        /// </summary>
        private static readonly int timeout = 5 * 1000;
        /// <summary>
        /// Valid HTTP methods for requests
        /// </summary>
        private enum request_type
        {
            GET, POST, PUT, DELETE
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientidOrOauth">Either a valid Client ID or a valid OAuth token from Twitch</param>
        /// <param name="isOauth">Set to true if the first parameter contains an OAuth token</param>
        public TwitchAPIHelix(string clientidOrOauth, bool isOauth)
        {
            this.clientidOrOauth = clientidOrOauth.Replace("oauth:", "");
            this.isOauth = isOauth;
        }
    }
}
