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
    /// Represents a date range
    /// </summary>
    [DataContract]
    public class DateRange
    {
        /// <summary>
        /// Start of the date range for the returned data in RFC 3339 format
        /// </summary>
        [DataMember]
        public string started_at;

        /// <summary>
        /// End of the date range for the returned data in RFC 3339 format
        /// </summary>
        [DataMember]
        public string ended_at;

        /// <summary>
        /// Default constructor
        /// </summary>
        internal DateRange()
        {
        }
    }
}
