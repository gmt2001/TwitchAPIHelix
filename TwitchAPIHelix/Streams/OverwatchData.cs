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
    /// Represents the streamer's Overwatch data
    /// </summary>
    [DataContract]
    public class OverwatchData
    {
        /// <summary>
        /// Overwatch metadata about the broadcaster
        /// </summary>
        [DataMember]
        public OverwatchPlayer broadcaster;

        /// <summary>
        /// Default Constructor
        /// </summary>
        internal OverwatchData()
        {
        }
    }
}
