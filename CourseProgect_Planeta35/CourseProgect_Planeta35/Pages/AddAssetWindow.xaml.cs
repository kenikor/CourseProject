using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;

namespace CourseProgect_Planeta35.Controls
{
    public partial class AddAssetWindow : Window
    {
        public Asset NewAsset { get; private set; }

        public AddAssetWindow()
        {
            InitializeComponent();

            using (var db = new AppDbContext())
            {
                // Заполняем категории
                CategoryBox.ItemsSource = db.AssetCategories.Select(c => c.Name).ToList();

                // Заполняем подразделения
                var departments = db.Departments.ToList();
                LocationBox.ItemsSource = departments;
                LocationBox.DisplayMemberPath = "Name";  // показываем имя
                LocationBox.SelectedValuePath = "Id";    // сохраняем Id
            }

            CategoryBox.SelectedIndex = 0;
            StatusBox.SelectedIndex = 0;
            LocationBox.SelectedIndex = 0;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Название обязательно");
                return;
            }

            if (App.CurrentUser == null)
            {
                MessageBox.Show("Ошибка: пользователь не авторизован.");
                return;
            }

            if (!(LocationBox.SelectedValue is int departmentId))
            {
                MessageBox.Show("Выберите корректный отдел");
                return;
            }

            using (var db = new AppDbContext())
            {
                var category = db.AssetCategories
                    .FirstOrDefault(c => c.Name == CategoryBox.SelectedItem.ToString());

                NewAsset = new Asset
                {
                    Name = NameBox.Text,
                    CategoryId = category?.Id ?? 0,
                    Status = StatusBox.SelectedItem != null
                        ? ((ComboBoxItem)StatusBox.SelectedItem).Content.ToString()
                        : "В эксплуатации",
                    InventoryNumber = SerialBox.Text,
                    PurchaseDate = PurchaseDateBox.SelectedDate,
                    Cost = decimal.TryParse(CostBox.Text, out var cost) ? cost : 0,
                    DepartmentId = departmentId,
                    Description = DescriptionBox.Text,
                    ResponsibleId = App.CurrentUser.Id,
                    ImagePath = string.IsNullOrWhiteSpace(ImagePathBox.Text)
                        ? null
                        : ImagePathBox.Text
                };

                db.Assets.Add(NewAsset);
                db.SaveChanges();
            }

            DialogResult = true;
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp|Все файлы|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                ImagePathBox.Text = dialog.FileName;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
