using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using System.Windows;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;


namespace CourseProgect_Planeta35.Pages
{
    /// <summary>
    /// Логика взаимодействия для AssetCheckWindow.xaml
    /// </summary>
    public partial class AssetCheckWindow : Window
    {
        private Asset Asset;
        public AssetCheckWindow(Asset asset)
        {
            InitializeComponent();
            Asset = asset;

            LoadData();
        }

        private void LoadData()
        {
            TitleText.Text = Asset.Name;
            AssetImage.Source = !string.IsNullOrEmpty(Asset.ImagePath)
                ? new BitmapImage(new Uri(Asset.ImagePath, UriKind.RelativeOrAbsolute))
                : null;

            CategoryText.Text = Asset.Category?.Name ?? "—";
            SerialText.Text = Asset.InventoryNumber ?? "—";
            ResponsibleText.Text = Asset.Responsible?.FullName ?? "—";
            StatusText.Text = Asset.Status ?? "—";

            if (Asset.IsChecked)
            {
                BtnMarkChecked.Background = new SolidColorBrush(System.Windows.Media.Colors.Gray);
                BtnMarkChecked.Content = "Проверено";
            }

            NotesBox.Text = Asset.Notes ?? "";
        }

        private void BtnMarkChecked_Click(object sender, RoutedEventArgs e)
        {
            Asset.IsChecked = true;

            BtnMarkChecked.Background = new SolidColorBrush(System.Windows.Media.Colors.Gray);
            BtnMarkChecked.Content = "Проверено";
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var a = db.Assets.FirstOrDefault(x => x.Id == Asset.Id);
                    if (a != null)
                    {
                        a.IsChecked = Asset.IsChecked;
                        a.Notes = NotesBox.Text;
                        db.SaveChanges();
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}