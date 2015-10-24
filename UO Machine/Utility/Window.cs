using System.Threading;
using UOMachine.UI.Misc;

namespace UOMachine.Utility
{
    public partial class Window
    {
        private class HexDumpData
        {
            private string m_Title;
            private byte[] m_Data;

            public string Title
            {
                get { return m_Title; }
            }

            public byte[] Data
            {
                get { return m_Data; }
            }

            public HexDumpData( string title, byte[] data )
            {
                m_Title = title;
                m_Data = data;
            }
        };

        /// <summary>
        /// Show Hex Output Window For Byte Array
        /// </summary>
        /// <param name="data">Byte array</param>
        public static void HexDump( byte[] data )
        {
            HexDump( data, null );
        }

        /// <summary>
        /// Show Hex Output Window For Byte Array
        /// </summary>
        /// <param name="data">Byte array</param>
        /// <param name="title">Window title</param>
        public static void HexDump( byte[] data, string title )
        {
            Thread thread = new Thread( new ParameterizedThreadStart( ( obj ) =>
            {
                HexDumpData hddata = obj as HexDumpData;
                HexDumpWindow hd = new HexDumpWindow( hddata.Title, hddata.Data );
                hd.Show();
                System.Windows.Threading.Dispatcher.Run();
            } ) );

            thread.SetApartmentState( ApartmentState.STA );
            thread.IsBackground = true;
            thread.Start( new HexDumpData( title, data ) );
        }
    }
}
