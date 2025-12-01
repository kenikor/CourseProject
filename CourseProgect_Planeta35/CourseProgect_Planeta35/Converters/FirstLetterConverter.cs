using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CourseProgect_Planeta35.Converters
{
    public class FirstLetterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            return string.IsNullOrEmpty(str) ? "?" : str.Substring(0, 1).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class RoleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var role = value as string;
            return role switch
            {
                "Администратор" => Brushes.Crimson,
                "Сотрудник" => Brushes.Green,
                _ => Brushes.Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class RandomBrushConverter : IValueConverter
    {
        private static readonly Brush[] colors = new Brush[]
        {
            Brushes.Teal,
            Brushes.Orange,
            Brushes.MediumPurple,
            Brushes.Crimson,
            Brushes.DarkCyan
        };
        private static readonly Random rnd = new Random();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return colors[rnd.Next(colors.Length)];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}