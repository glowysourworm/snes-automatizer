using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

using snes_automatizer.Model;

namespace snes_automatizer.Converter
{
    public class OutputMessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return value;

            var severity = (MessageSeverity)value;

            switch (severity)
            {
                case MessageSeverity.Info:
                    return Colors.White;
                case MessageSeverity.Warning:
                    return Color.FromArgb(0xAF, 0xFF, 0xFF, 0x00);
                case MessageSeverity.Error:
                    return Color.FromArgb(0xAF, 0x00, 0x00, 0xFF);
                case MessageSeverity.CommandLineApplicationStdOut:
                    return Color.FromArgb(0x8F, 0x00, 0x00, 0x00);
                case MessageSeverity.CommandLineApplicationError:
                    return Color.FromArgb(0x8F, 0x00, 0x00, 0xFF);
                default:
                    throw new Exception("Unhandled MessageSeverity type in OutputMessageBackgroundConverter.cs");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
