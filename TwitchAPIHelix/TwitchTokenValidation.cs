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
using System.Runtime.Serialization;

namespace TwitchAPIHelix
{
    /// <summary>
    /// Represents an entry containing data about a valid Twitch token
    /// </summary>
    [DataContract]
    public class TwitchTokenValidation
    {
        /// <summary>
        /// The client ID of the authorized Twitch application that created the token
        /// </summary>
        [DataMember]
        public string client_id;

        /// <summary>
        /// The login name of the user
        /// </summary>
        [DataMember]
        public string login;

        /// <summary>
        /// The authorized scopes for this token
        /// </summary>
        [DataMember]
        public string[] scopes;

        /// <summary>
        /// The user ID attached to this token
        /// </summary>
        [DataMember]
        public string user_id;

        /// <summary>
        /// Default constructor
        /// </summary>
        internal TwitchTokenValidation()
        {
        }
    }
}