using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CourseProgect_Planeta35.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;
            switch (status)
            {
                case "Проверено":
                    return new SolidColorBrush(Color.FromRgb(139, 155, 76)); // зелёный
                case "Отсутствует":
                    return new SolidColorBrush(Color.FromRgb(237, 106, 90)); // красный
                case "Повреждено":
                    return new SolidColorBrush(Color.FromRgb(255, 180, 0)); // оранжевый
                default:
                    return new SolidColorBrush(Color.FromRgb(224, 224, 224)); // серый
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
