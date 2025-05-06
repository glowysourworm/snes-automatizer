using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace snes_automatizer.Converter
{
    public class LeftEllipsisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return value;

            var stringValue = value as string;

            if (stringValue == null)
                return value;

            if (stringValue.Length >= 50)
                return "..." + stringValue.Substring(stringValue.Length - 50, 50);

            return stringValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
