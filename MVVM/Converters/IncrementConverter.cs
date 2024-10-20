using System;
using System.Globalization;
using System.Windows.Data;

namespace Desktoper.MVVM.Converters
{
    public sealed class IncrementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int i ? ++i : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}