using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOMachine.Data
{
    internal delegate void OnPacketReceive(int client, PacketReader reader);

    internal class PacketHandler
    {
        private int m_PacketID;
        private int m_Length;
        private OnPacketReceive m_OnReceive;

        public int PacketID
        {
            get { return m_PacketID; }
            set { m_PacketID = value; }
        }

        public int Length
        {
            get { return m_Length; }
            set { m_Length = value; }
        }

        public OnPacketReceive OnReceive
        {
            get { return m_OnReceive; }
            set { m_OnReceive = value; }
        }

        public PacketHandler(int packetId, int length, OnPacketReceive onReceive)
        {
            m_PacketID = packetId;
            m_Length = length;
            m_OnReceive = onReceive;
        }
    }
}
