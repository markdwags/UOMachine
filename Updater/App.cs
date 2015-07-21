using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Updater
{
    public partial class Program
    {
        [STAThread]
        public static void Main()
        {
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo( "zh" );
            try
            {
                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
            catch
            {
                MessageBox.Show( "Exception caught" );
            }
        }
    }
}
