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
    /// Represents a stream's metadata
    /// </summary>
    [DataContract]
    public class StreamMetadataEntry
    {
        /// <summary>
        /// User ID of the streamer (broadcaster)
        /// </summary>
        [DataMember]
        public string user_id;

        /// <summary>
        /// Login name corresponding to <see cref="user_id"/>
        /// </summary>
        [DataMember]
        public string user_name;

        /// <summary>
        /// ID of the game being played on the stream: 488552 (Overwatch), 138585 (Hearthstone), or null (neither Overwatch nor Hearthstone metadata is available)
        /// </summary>
        [DataMember]
        public string game_id;

        /// <summary>
        /// Object containing the Overwatch metadata, if available; otherwise, null
        /// </summary>
        [DataMember]
        public OverwatchData overwatch;

        /// <summary>
        /// Object containing the Hearthstone metadata, if available; otherwise, null
        /// </summary>
        [DataMember]
        public HearthstoneData hearthstone;

        /// <summary>
        /// Default Constructor
        /// </summary>
        internal StreamMetadataEntry()
        {
        }
    }
}
