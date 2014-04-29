/* Copyright (C) 2014 John Scott
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

using System;

namespace UOMachine.Macros
{
    public static partial class MacroEx
    {
        /// <summary>
        /// Send item properties request for single serial.
        /// </summary>
        /// <param name="client">Target client.</param>
        /// <param name="serial">Item serial.</param>
        public static void RequestItemProperties(int client, int serial)
        {
            RequestItemProperties(client, new int[] { serial });
        }

        /// <summary>
        /// Send item properties request for multiple serials.
        /// </summary>
        /// <param name="client">Target client.</param>
        /// <param name="serials">int array of target serials.</param>
        public static void RequestItemProperties(int client, int[] serials)
        {
            int size = (serials.Length*4) + 3;
            byte[] packet = new byte[size];
            packet[0] = 0xD6;
            packet[1] = (byte)(size >> 8);
            packet[2] = (byte)size;
            int i = 3;
            foreach (int serial in serials)
            {
                packet[i] = (byte)(serial >> 24);
                packet[i + 1] = (byte)(serial >> 16);
                packet[i + 2] = (byte)(serial >> 8);
                packet[i + 3] = (byte)serial;
                i+=4;
            }
            MacroEx.SendPacketToServer(client, packet);
        }
    }
}