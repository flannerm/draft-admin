using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace DraftAdmin.Converters
{
    public class BoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility val = Visibility.Hidden;

            try
            {
                var x = bool.Parse(value.ToString());

                if (x)
                {
                    val = Visibility.Visible;
                }
            }
            catch (Exception)
            {
            }
            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
