using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using System;
using System.Windows;
using System.Windows.Media;

namespace CourseProgect_Planeta35.Pages
{
    public partial class AddCategoryWindow : Window
    {
        public string CategoryName { get; private set; }
        public string ColorHex { get; private set; }

        // Конструктор для добавления новой категории
        public AddCategoryWindow()
        {
            InitializeComponent();
            ColorBox.TextChanged += (s, e) => UpdateColorPreview();
        }

        // Конструктор для редактирования существующей категории
        public AddCategoryWindow(string name, string color) : this()
        {
            NameBox.Text = name;
            ColorBox.Text = color;
        }

        private void UpdateColorPreview()
        {
            try
            {
                ColorPreview.Fill = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(ColorBox.Text));
            }
            catch
            {
                ColorPreview.Fill = Brushes.Transparent;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // Мини-валидация
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Название категории обязательно!");
                return;
            }

            // Устанавливаем публичные свойства, чтобы их можно было получить снаружи
            CategoryName = NameBox.Text.Trim();
            ColorHex = ColorBox.Text.Trim();

            DialogResult = true; // окно закрывается, возвращая true
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
