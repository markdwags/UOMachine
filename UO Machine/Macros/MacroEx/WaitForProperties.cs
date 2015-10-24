/* Copyright (C) 2015 John Scott
 * 
 * This file is part of UO Machine.
 * 
 * UO Machine is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * UO Machine is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with UO Machine.  If not, see <http://www.gnu.org/licenses/>. */

namespace UOMachine.Macros
{
    public static partial class MacroEx
    {
        /// <summary>
        /// Pause until properties are received for specified serial.
        /// </summary>
        /// <param name="client">Client index.</param>
        /// <param name="serial">Serial to receive properties for.</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>True if properties received or false if not.</returns>
        public static bool WaitForProperties(int client, int serial, int timeout)
        {
            ClientInfo ci;
            bool result = false;
            if (ClientInfoCollection.GetClient( client, out ci ))
                result = ci.WaitForProperties( serial, timeout );
            return result;
        }
    }
}