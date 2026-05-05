using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CourseProgect_Planeta35.Converters
{    
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b)
                ? new SolidColorBrush(Color.FromRgb(82, 166, 106))
                : new SolidColorBrush(Color.FromRgb(139, 58, 58));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
