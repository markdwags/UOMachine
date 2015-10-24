using System.Runtime.InteropServices;

namespace UnitTests
{
    public static class NativeMethods
    {
        [StructLayout(LayoutKind.Explicit, Size = 4)]
        public struct in_addr
        {
            [FieldOffset(0)]
            public byte s_b1;
            [FieldOffset(1)]
            public byte s_b2;
            [FieldOffset(2)]
            public byte s_b3;
            [FieldOffset(3)]
            public byte s_b4;

            [FieldOffset(0)]
            public ushort s_w1;
            [FieldOffset(2)]
            public ushort s_w2;


            /// <summary>
            /// can be used for most tcp & ip code
            /// </summary>
            public uint s_addr { get { return s_b1; } }

            /// <summary>
            /// host on imp
            /// </summary>
            public byte s_host { get { return s_b2; } }

            /// <summary>
            /// network
            /// </summary>
            public byte s_net { get { return s_b1; } }

            /// <summary>
            /// imp
            /// </summary>
            public ushort s_imp { get { return s_w2; } }

            /// <summary>
            /// imp #
            /// </summary>
            public byte s_impno { get { return s_b4; } }

            /// <summary>
            /// logical host
            /// </summary>
            public byte s_lh { get { return s_b3; } }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct sockaddr_in
        {
            public short sin_family;
            public ushort sin_port;
            public in_addr sin_addr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] sin_zero;
        }

        [System.Runtime.InteropServices.DllImport("wsock32.dll")]
        internal static extern ushort ntohs(ushort netshort);
    }
}