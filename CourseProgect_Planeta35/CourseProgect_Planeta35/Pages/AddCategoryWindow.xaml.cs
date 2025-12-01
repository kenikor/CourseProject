using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using System.Windows;
using System.Windows.Media;

namespace CourseProgect_Planeta35.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddCategoryWindow.xaml
    /// </summary>
    public partial class AddCategoryWindow : Window
    {
        public string CategoryName { get; private set; }
        public string ColorHex { get; private set; }

        public AddCategoryWindow()
        {
            InitializeComponent();
            ColorBox.TextChanged += (s, e) => UpdateColorPreview();
        }

        private void UpdateColorPreview()
        {
            try
            {
                ColorPreview.Fill = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(ColorBox.Text));
            }
            catch { }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // Мини-валидация, чтобы не было пустого имени
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Название категории обязательно!");
                return;
            }

            try
            {
                using (var db = new AppDbContext()) 
                {
                    var newCategory = new AssetCategory
                    {
                        Name = NameBox.Text.Trim(),
                        Description = DescriptionBox.Text.Trim(),
                        Color = ColorBox.Text.Trim()
                    };

                    db.AssetCategories.Add(newCategory);
                    db.SaveChanges();
                }

                MessageBox.Show("Категория добавлена!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении категории:\n{ex.Message}");
            }
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
