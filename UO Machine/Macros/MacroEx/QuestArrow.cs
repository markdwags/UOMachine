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


namespace UOMachine.Macros
{
    public static partial class MacroEx
    {
        /// <summary>
        /// Send quest arrow packet to client.
        /// </summary>
        /// <param name="client">Target client.</param>
        /// <param name="active">Turn on or off.</param>
        /// <param name="x">X location</param>
        /// <param name="y">Y location</param>
        /// <param name="serial">Target serial or -1</param>
        public static void QuestArrow(int client, bool active, int x, int y, int serial)
        {
            byte[] packet = new byte[10];
            packet[0] = 0xBA;
            if (active)
            {
                packet[1] = 0x01;
                packet[2] = (byte)(x >> 8);
                packet[3] = (byte)x;
                packet[4] = (byte)(y >> 8);
                packet[5] = (byte)y;
            }
            packet[6] = (byte)(serial >> 24);
            packet[7] = (byte)(serial >> 16);
            packet[8] = (byte)(serial >> 8);
            packet[9] = (byte)serial;
            MacroEx.SendPacketToClient(client, packet);
        }
    }
}