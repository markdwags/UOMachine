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
        /// Displays text over specified item serial.
        /// </summary>
        /// <param name="client">Target client.</param>
        /// <param name="serial">Item serial.</param>
        /// <param name="graphic">Item graphic.</param>
        /// <param name="messageType">Message type (default 0).</param>
        /// <param name="hue">Hue (default 0x3B2)</param>
        /// <param name="text">Text to display.</param>
        public static void OverheadMessage(int client, int serial, int graphic, int messageType, int hue, string text)
        {
            int size = 48 + ((text.Length+1) * 2);
            int font = 3;
            if (hue == 0) hue = 0x3B2;

            byte[] packet = new byte[size];
            packet[0] = 0xAE;
            packet[1] = (byte)(size >> 8);
            packet[2] = (byte)size;
            packet[3] = (byte)(serial >> 24);
            packet[4] = (byte)(serial >> 16);
            packet[5] = (byte)(serial >> 8);
            packet[6] = (byte)serial;
            packet[7] = (byte)(graphic >> 8);
            packet[8] = (byte)graphic;
            packet[9] = (byte)messageType;
            packet[10] = (byte)(hue >> 8);
            packet[11] = (byte)hue;
            packet[12] = (byte)(font >> 8);
            packet[13] = (byte)font;
            packet[14] = (byte)'E';
            packet[15] = (byte)'N';
            packet[16] = (byte)'U';
            byte[] textBytes = System.Text.UnicodeEncoding.BigEndianUnicode.GetBytes(text + '\0');
            Buffer.BlockCopy(textBytes, 0, packet, 48, (text.Length*2));
            MacroEx.SendPacketToClient(client, packet);
        }
    }
}