using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace DraftAdmin.Converters
{
    public class BoolToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Brush bluebrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#003D81"));
            Brush gb = new LinearGradientBrush();
            Brush yellowbrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FACE1F"));
            bool b = (bool)value;
            return b ? bluebrush : yellowbrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
