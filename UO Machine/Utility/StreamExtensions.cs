using System.IO;
using System.Runtime.InteropServices;

namespace UOMachine.Utility
{
    public static class StreamExtensions
    {
        public static T ReadStruct<T>( this Stream stream ) where T : struct
        {
            int size = Marshal.SizeOf( typeof( T ) );
            byte[] buffer = new byte[size];
            stream.Read( buffer, 0, size );
            GCHandle pinnedBuffer = GCHandle.Alloc( buffer, GCHandleType.Pinned );
            T structure = (T) Marshal.PtrToStructure( pinnedBuffer.AddrOfPinnedObject(), typeof( T ) );
            pinnedBuffer.Free();
            return structure;
        }
    }
}
