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

namespace TwitchAPIHelix.Users
{
    /// <summary>
    /// An entry containing data about a user
    /// </summary>
    [DataContract]
    public class UserEntry
    {
        /// <summary>
        /// User’s ID
        /// </summary>
        [DataMember]
        public string id;

        /// <summary>
        /// User’s login name
        /// </summary>
        [DataMember]
        public string login;

        /// <summary>
        /// User’s display name
        /// </summary>
        [DataMember]
        public string display_name;

        /// <summary>
        /// User’s type: "staff", "admin", "global_mod", or ""
        /// </summary>
        [DataMember]
        public string type;

        /// <summary>
        /// User’s broadcaster type: "partner", "affiliate", or ""
        /// </summary>
        [DataMember]
        public string broadcaster_type;

        /// <summary>
        /// User’s channel description
        /// </summary>
        [DataMember]
        public string description;

        /// <summary>
        /// URL of the user’s profile image
        /// </summary>
        [DataMember]
        public string profile_image_url;

        /// <summary>
        /// URL of the user’s offline image
        /// </summary>
        [DataMember]
        public string offline_image_url;

        /// <summary>
        /// Total number of views of the user’s channel
        /// </summary>
        [DataMember]
        public int view_count;

        /// <summary>
        /// User’s email address. Returned if the request includes the user:read:email scope
        /// </summary>
        [DataMember]
        public string email;

        /// <summary>
        /// Default Constructor
        /// </summary>
        internal UserEntry()
        {
        }
    }
}
