using CourseProgect_Planeta35.Controls;
using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using CourseProgect_Planeta35.Pages;
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

        public event Action<string> NavigateRequested;

        public MainControl(User user)
        {
            InitializeComponent();
            CurrentUser = user;
            _db = new AppDbContext();

            // ВСЕГДА показываем полный инвентарь
            Items = _db.Assets.ToList();

            // Категории
            Categories = _db.AssetCategories.ToList();

            LoadStats();
            LoadCategories();

            // Админ-кнопки только для админа
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

        private Color GetColorForCategory(int id)
        {
            string[] palette =
            {
                "#8B9B4C", // основной зелёный
                "#C5C895", // светлый
                "#B89968", // тёплый
                "#6B7A35"  // тёмный
            };

            return (Color)ColorConverter.ConvertFromString(
                palette[id % palette.Length]
            );
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
        private void Navigate(string page)
        {
            var mainPage = Window.GetWindow(this) as MainWindow;

            if (mainPage?.MainFrame != null)
            {
                switch (page)
                {
                    case "Inventory":
                        mainPage.MainFrame.Content = new InventoryListControl(CurrentUser);
                        break;
                    case "Check":
                        mainPage.MainFrame.Content = new InventoryCheckControl(CurrentUser);
                        break;
                    case "Reports":
                        mainPage.MainFrame.Content = new ReportsControl(CurrentUser, new AppDbContext());
                        break;
                    case "Categories":
                        mainPage.MainFrame.Content = new CategoriesControl(CurrentUser);
                        break;
                    case "Departments":
                        mainPage.MainFrame.Content = new DepartmentsControl(CurrentUser);
                        break;
                    case "Users":
                        mainPage.MainFrame.Content = new UserAddControl(CurrentUser);
                        break;
                }
            }
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Inventory");
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Check");
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Reports");
        }

        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Categories");
        }

        private void Departments_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Departments");
        }

        private void Users_Click(object sender, RoutedEventArgs e)
        {
            Navigate("Users");
        }
    }
}
