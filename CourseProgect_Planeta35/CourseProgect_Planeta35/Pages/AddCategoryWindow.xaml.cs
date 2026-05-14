using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CourseProgect_Planeta35.Pages
{
    public partial class AddCategoryWindow : Window
    {
        public string CategoryName { get; private set; }
        public string CategoryDescription { get; private set; }
        public string ColorHex { get; private set; }

        public AddCategoryWindow()
        {
            InitializeComponent();
        }

        public AddCategoryWindow(string name, string color) : this()
        {
            NameBox.Text = name;

            if (!string.IsNullOrWhiteSpace(color))
            {
                try
                {
                    CategoryColorPicker.SelectedColor =
                        (Color)ColorConverter.ConvertFromString(color);
                }
                catch { }
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Название обязательно");
                return;
            }

            var color = CategoryColorPicker.SelectedColor ?? Colors.Green;

            CategoryName = NameBox.Text.Trim();
            CategoryDescription = DescriptionBox.Text.Trim();
            ColorHex = color.ToString();

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}