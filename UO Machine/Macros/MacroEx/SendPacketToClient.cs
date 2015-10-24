﻿/* Copyright (C) 2009 Matthew Geyer
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

using UOMachine.IPC;
using UOMachine.Data;

namespace UOMachine.Macros
{
    public static partial class MacroEx
    {
        /// <summary>
        /// Send packet directly to client's packet receive function.
        /// </summary>
        /// <param name="client">Target client.</param>
        /// <param name="packet">PacketWriter to send.</param>
        public static void SendPacketToClient(int client, PacketWriter packet)
        {
            SendPacketToClient(0, packet.ToArray());
        }

        /// <summary>
        /// Send packet directly to client's packet receive function.
        /// </summary>
        /// <param name="client">Target client.</param>
        /// <param name="packet">Raw packets to send.</param>
        public static void SendPacketToClient(int client, byte[] packet)
        {
            ClientInfo ci;
            if (ClientInfoCollection.GetClient(client, out ci))
                Network.SendCommand(ci.IPCServerIndex, Command.SendPacket, ci.ClientSendCaveAddress.ToInt32(), (byte)PacketType.Client, packet);
        }
    }
}