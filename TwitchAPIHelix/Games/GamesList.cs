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

namespace TwitchAPIHelix.Games
{
    /// <summary>
    /// Represents an array of games
    /// </summary>
    [DataContract]
    public class GamesList
    {
        /// <summary>
        /// An array of games
        /// </summary>
        [DataMember]
        public GameEntry[] data;

        /// <summary>
        /// A cursor value, to be used in a subsequent request to specify the starting point of the next set of results
        ///
        /// <para>This field is only available on GetTopGames requests</para>
        /// </summary>
        [DataMember]
        public Pagination pagination;

        /// <summary>
        /// Default constructor
        /// </summary>
        internal GamesList()
        {
        }
    }
}