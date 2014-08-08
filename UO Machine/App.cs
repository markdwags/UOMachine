using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using UOMachine.Resources;

namespace UOMachine
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo( "zh" );
            try
            {
                Utility.Log.Initialize( "UOMLog" + DateTime.Now.ToString( " [MM-dd-yyyy HH.mm.ss] " ) + ".txt" );
                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                Utility.Log.LogMessage( ex );
                UOM.Dispose();
                MessageBox.Show( Strings.Exceptioncaught );
            }
        }
    }

}
