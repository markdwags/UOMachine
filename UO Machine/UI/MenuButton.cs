using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UOMachine.UI
{
    public class MenuButton : Button
    {
        public static readonly DependencyProperty ImageProperty =
              DependencyProperty.RegisterAttached( "Image", typeof( DrawingImage ), typeof( MenuButton ), new PropertyMetadata( default( DrawingImage ) ) );

        public static void SetImage( UIElement element, DrawingImage value )
        {
            element.SetValue( ImageProperty, value );
        }

        public static DrawingImage GetImage( UIElement element )
        {
            return (DrawingImage)element.GetValue( ImageProperty );
        }
    }
}
