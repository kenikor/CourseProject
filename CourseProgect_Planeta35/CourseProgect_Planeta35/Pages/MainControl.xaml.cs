using CourseProgect_Planeta35.Controls;
using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CourseProgect_Planeta35
{
    public partial class MainControl : UserControl
    {
        private readonly AppDbContext _db;
        public User CurrentUser { get; }
        public List<Asset> Items { get; set; }
        public List<AssetCategory> Categories { get; set; }

        public MainControl(User user)
        {
            InitializeComponent();
            CurrentUser = user;
            _db = new AppDbContext();

            // Загружаем данные
            Categories = _db.AssetCategories.ToList();
            Items = user.RoleId == 1 ? _db.Assets.ToList() : _db.Assets.Where(a => a.ResponsibleId == user.Id).ToList();

            LoadStats();
            LoadCategories();

            // Показываем админ-кнопки только для админа
            AdminButtonsPanel.Visibility = CurrentUser.RoleId == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadStats()
        {
            UserNameTextBlock.Text = $"Добро пожаловать, {CurrentUser.FullName ?? CurrentUser.Username}!";

            TotalObjectsText.Text = Items.Count.ToString();
            ActiveObjectsText.Text = Items.Count(i => i.Status == "В эксплуатации").ToString();
            MaintenanceObjectsText.Text = Items.Count(i => i.Status == "На обслуживании").ToString();
            TotalValueText.Text = $"{Items.Sum(i => i.Cost ?? 0):N0} ₽";
        }

        private void LoadCategories()
        {
            CategoriesPanel.Children.Clear();
            int total = Items.Count;

            foreach (var cat in Categories)
            {
                var catItems = Items.Where(i => i.CategoryId == cat.Id).ToList();
                if (catItems.Count == 0) continue;

                double percentage = total > 0 ? (double)catItems.Count / total * 100 : 0;

                StackPanel sp = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };

                StackPanel header = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch };
                header.Children.Add(new TextBlock { Text = cat.Name, Width = 160, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3C2F1F")), FontWeight = FontWeights.SemiBold });
                header.Children.Add(new TextBlock { Text = $"{catItems.Count} объектов", Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7A35")) });

                sp.Children.Add(header);

                sp.Children.Add(new ProgressBar
                {
                    Minimum = 0,
                    Maximum = 100,
                    Value = percentage,
                    Height = 10,
                    Foreground = new SolidColorBrush(GetColorForCategory(cat.Id)),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F1E9"))
                });

                CategoriesPanel.Children.Add(sp);
            }
        }

        private Color GetColorForCategory(int id)
        {
            string[] palette = { "#8B9B4C", "#C5C895", "#B89968", "#6B7A35" };
            return (Color)ColorConverter.ConvertFromString(palette[id % palette.Length]);
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new AppDbContext())
            {
                var categories = db.AssetCategories.ToList();
                foreach (var category in categories)
                    db.Entry(category).Collection(c => c.Assets).Load();

                var inventoryItems = new List<InventoryItem>();
                foreach (var category in categories)
                    foreach (var asset in category.Assets)
                        inventoryItems.Add(new InventoryItem { Asset = asset });

                ContentFrame.Content = new InventoryListControl(CurrentUser);
            }
        }


        private void Check_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Провести инвентаризацию");
        private void Reports_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Открыть отчёты");
        private void Categories_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Админ: Категории");
        private void Departments_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Админ: Подразделения");
        private void Users_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Админ: Пользователи");
    }
}
