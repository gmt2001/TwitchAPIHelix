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
    /// Represents a Hearthstone hero
    /// </summary>
    [DataContract]
    public class HearthstoneHero
    {
        /// <summary>
        /// Type of the Hearthstone hero
        /// </summary>
        [DataMember]
        public string type;

        /// <summary>
        /// Class of the Hearthstone hero
        /// </summary>
        [DataMember(Name = "class")]
        public string _class;

        /// <summary>
        /// Name of the Hearthstone hero
        /// </summary>
        [DataMember]
        public string name;

        /// <summary>
        /// Default Constructor
        /// </summary>
        internal HearthstoneHero()
        {
        }
    }
}
