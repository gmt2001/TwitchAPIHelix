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

namespace TwitchAPIHelix.Streams
{
    /// <summary>
    /// An entry containing data about a stream
    /// </summary>
    [DataContract]
    public class StreamEntry
    {
        /// <summary>
        /// Stream ID
        /// </summary>
        [DataMember]
        public string id;

        /// <summary>
        /// ID of the user who is streaming
        /// </summary>
        [DataMember]
        public string user_id;

        /// <summary>
        /// Login name corresponding to <see cref="user_id"/>
        /// </summary>
        [DataMember]
        public string user_name;

        /// <summary>
        /// ID of the game being played on the stream
        /// </summary>
        [DataMember]
        public string game_id;

        /// <summary>
        /// Array of community IDs
        /// </summary>
        [DataMember]
        public string[] community_ids;

        /// <summary>
        /// Stream type: "live" or "" (in case of error)
        /// </summary>
        [DataMember]
        public string type;

        /// <summary>
        /// Stream title
        /// </summary>
        [DataMember]
        public string title;

        /// <summary>
        /// Number of viewers watching the stream at the time of the query
        /// </summary>
        [DataMember]
        public int viewer_count;

        /// <summary>
        /// UTC timestamp in RFC 3339 format
        /// </summary>
        [DataMember]
        public string started_at;

        /// <summary>
        /// Stream language
        /// </summary>
        [DataMember]
        public string language;

        /// <summary>
        /// Thumbnail URL of the stream. All image URLs have variable width and height. You can replace {width} and {height} with any values to get that size image
        /// </summary>
        [DataMember]
        public string thumbnail_url;

        /// <summary>
        /// Default Constructor
        /// </summary>
        internal StreamEntry()
        {
        }
    }
}
