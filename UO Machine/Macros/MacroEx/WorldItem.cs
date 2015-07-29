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
        /// Send WorldItem packet to client.
        /// </summary>
        /// <param name="client">Target client.</param>
        /// <param name="serial">Item serial.</param>
        /// <param name="itemid">Item ID.</param>
        /// <param name="x">X Location.</param>
        /// <param name="y">Y Location.</param>
        /// <param name="z">Z Location.</param>
        /// <param name="hue">Item hue.</param>
        public static void WorldItem(int client, int serial, int itemid, int x, int y, int z, int hue)
        {
            byte[] packet = new byte[26];
            packet[0] = 0xF3;
            packet[2] = 0x01;
            packet[4] = (byte)(serial >> 24);
            packet[5] = (byte)(serial >> 16);
            packet[6] = (byte)(serial >> 8);
            packet[7] = (byte)serial;
            itemid &= 0xFFFF;
            packet[8] = (byte)(itemid >> 8);
            packet[9] = (byte)itemid;
            int amount = 1;
            packet[11] = (byte)(amount >> 8);
            packet[12] = (byte)amount;
            packet[13] = (byte)(amount >> 8);
            packet[14] = (byte)amount;
            packet[15] = (byte)(x >> 8);
            packet[16] = (byte)x;
            packet[17] = (byte)(y >> 8);
            packet[18] = (byte)y;
            packet[19] = (byte)z;
            //byte - light
            packet[21] = (byte)(hue >> 8);
            packet[22] = (byte)hue;
            MacroEx.SendPacketToClient(client, packet);
        }
    }
}
