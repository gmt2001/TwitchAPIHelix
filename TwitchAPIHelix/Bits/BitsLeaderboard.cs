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

namespace TwitchAPIHelix.Bits
{
    /// <summary>
    /// Represents a bits leaderboard
    /// </summary>
    [DataContract]
    public class BitsLeaderboard
    {
        /// <summary>
        /// An array of leaderboard entries
        /// </summary>
        [DataMember]
        public BitsLeaderboardEntry[] data;

        /// <summary>
        /// The date range being queried
        /// </summary>
        [DataMember]
        public DateRange date_range;

        /// <summary>
        /// Total number of results (users) returned. This is count or the total number of entries in the leaderboard, whichever is less
        /// </summary>
        [DataMember]
        public int total;

        /// <summary>
        /// Default constructor
        /// </summary>
        internal BitsLeaderboard()
        {
        }
    }
}
