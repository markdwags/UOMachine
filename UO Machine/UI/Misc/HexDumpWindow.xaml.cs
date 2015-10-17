using System.Text;
using System.Windows;

namespace UOMachine.UI.Misc
{
    /// <summary>
    /// Interaction logic for HexDump.xaml
    /// </summary>
    internal partial class HexDumpWindow: Window
    {
        public HexDumpWindow()
        {
            InitializeComponent();
        }

        public HexDumpWindow( string title, byte[] data)
        {
            if (title != null)
                m_Title = title;
            m_Data = data;
            SetData();
            InitializeComponent();
        }

        private byte[] m_Data;
        private string m_Title = "Hex Dump";
        private string m_BinaryData;
        private string m_TextData;

        public string Status
        {
            get {
                if (m_Data == null)
                    return "";
                return "Length: " + m_Data.Length;
            }
        }

        public string WindowTitle
        {
            get { return m_Title; }
        }

        public string BinaryData
        {
            get { return m_BinaryData; }
        }

        public string TextData
        {
            get { return m_TextData; }
        }

        public void SetData()
        {
            if (m_Data == null)
                return;
            StringBuilder binaryBuilder = new StringBuilder();
            StringBuilder textBuilder = new StringBuilder();

            byte b1, b2;
            for (int i = 0; i < m_Data.Length; i++)
            {
                b1 = (byte) ( m_Data[i] >> 4 );
                b2 = (byte) ( m_Data[i] & 0xF );
                binaryBuilder.Append( (char) ( b1 > 9 ? b1 + 0x37 : b1 + 0x30 ) );
                binaryBuilder.Append( (char) ( b2 > 9 ? b2 + 0x37 : b2 + 0x30 ) );
                binaryBuilder.Append( ' ' );
                b1 = m_Data[i];

                if (b1 < 0x20 || b1 == 0xB7 || b1 == 0xFF)
                    b1 = (byte) '.';

                textBuilder.Append( (char) b1 );
                if (( i + 1 ) % 16 == 0)
                {
                    binaryBuilder.Remove( binaryBuilder.Length - 1, 1 );
                    binaryBuilder.AppendLine();
                    textBuilder.AppendLine();
                }
            }

            m_BinaryData = binaryBuilder.ToString();
            m_TextData = textBuilder.ToString();
        }
    }
}
