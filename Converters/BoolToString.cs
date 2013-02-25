using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace DraftAdmin.Converters
{
    public class BoolToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = "";

            try
            {
                var x = bool.Parse(value.ToString());
                if (x)
                {
                    val = "Stop Cycle";
                }
                else
                {
                    val = "Start Cycle";
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

