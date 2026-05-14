using CourseProgect_Planeta35.Controls;
using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using UIInventoryItem = CourseProgect_Planeta35.Models.InventoryItem;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CourseProgect_Planeta35.Pages
{
    public partial class MainPage : Page
    {
        private readonly User CurrentUser;
        private List<AssetCategory> AllCategories;
        private List<UIInventoryItem> AllInventoryItems;

        public MainPage(User user)
        {
            InitializeComponent();
            CurrentUser = user;

            SetupAdminPanel();
            ConfigureInterface(user);
            LoadDataFromDb();
            LoadDashboard();
            StartLiquidAnimation();

            BtnDashboard.IsChecked = true;
        }
        private void StartLiquidAnimation()
        {
            var brush1 = (RadialGradientBrush)FindResource("Brush1");
            var brush2 = (RadialGradientBrush)FindResource("Brush2");
            var brush3 = (RadialGradientBrush)FindResource("Brush3");

            AnimateBrush(brush1, 12, 0.0, 1.0);
            AnimateBrush(brush2, 16, 1.0, 0.0);
            AnimateBrush(brush3, 9, 0.2, 0.9);
        }

        private void AnimateBrush(RadialGradientBrush brush, double seconds, double from, double to)
        {
            var originAnim = new PointAnimation
            {
                From = new Point(from, from),
                To = new Point(to, to),
                Duration = TimeSpan.FromSeconds(seconds),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            var centerAnim = new PointAnimation
            {
                From = new Point(0.4, 0.4),
                To = new Point(0.6, 0.6),
                Duration = TimeSpan.FromSeconds(seconds * 1.3),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            brush.BeginAnimation(RadialGradientBrush.GradientOriginProperty, originAnim);
            brush.BeginAnimation(RadialGradientBrush.CenterProperty, centerAnim);
        }

        private void SetupAdminPanel()
        {
            AdminPanel.Visibility = (CurrentUser.RoleId == 1)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ConfigureInterface(User user)
        {
            if (user == null) return;

            BtnProcurement.Visibility = Visibility.Collapsed;
            BtnInventory.Visibility = Visibility.Collapsed;
            BtnReports.Visibility = Visibility.Collapsed;

            if (user.RoleId == 1) // admin
            {
                BtnProcurement.Visibility = Visibility.Visible;
                BtnInventory.Visibility = Visibility.Visible;
                BtnReports.Visibility = Visibility.Visible;
                AdminPanel.Visibility = Visibility.Visible;
            }
            else if (user.RoleId == 2) // manager
            {
                BtnProcurement.Visibility = Visibility.Visible;
            }
        }

        private void LoadDashboard()
        {
            ContentFrame.Content = new MainControl(CurrentUser);
        }

        private void LoadInventory()
        {
            ContentFrame.Content = new InventoryListControl(CurrentUser);
        }

        private void LoadDataFromDb()
        {
            try
            {
                using var db = new AppDbContext();

                AllCategories = db.AssetCategories
                    .Include(c => c.Assets)
                        .ThenInclude(a => a.Responsible)
                    .Where(c => c.Assets.Any())
                    .AsNoTracking()
                    .ToList();

                AllInventoryItems = AllCategories
                    .SelectMany(c => c.Assets, (c, asset) => new UIInventoryItem
                    {
                        Asset = asset
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки из БД: " + ex.Message);
                AllCategories = new List<AssetCategory>();
                AllInventoryItems = new List<UIInventoryItem>();
            }
        }

        private void MenuButton_Checked(object sender, RoutedEventArgs e)
        {
            var clicked = sender as ToggleButton;
            if (clicked == null) return;

            var allButtons = new List<ToggleButton>
            {
                BtnDashboard,
                BtnInventory,
                BtnCheck,
                BtnReports
            };

            if (CurrentUser.RoleId == 1)
            {
                allButtons.AddRange(new[]
                {
                    BtnCategories,
                    BtnDepartments,
                    BtnUsers,
                    BtnProcurement,
                    BtnReports
                });
            }
            else if (CurrentUser.RoleId == 2)
            {
                allButtons.Add(BtnProcurement);
            }

            foreach (var btn in allButtons)
            {
                if (btn != clicked)
                    btn.IsChecked = false;
            }

            // Навигация
            if (clicked == BtnDashboard)
                LoadDashboard();

            else if (clicked == BtnInventory)
                LoadInventory();

            else if (clicked == BtnCheck)
                ContentFrame.Content = new InventoryCheckControl(CurrentUser);

            else if (clicked == BtnReports)
                ContentFrame.Content = new ReportsControl(CurrentUser, new AppDbContext());

            else if (clicked == BtnCategories)
                ContentFrame.Content = new CategoriesControl(CurrentUser);

            else if (clicked == BtnDepartments)
                ContentFrame.Content = new DepartmentsControl(CurrentUser);

            else if (clicked == BtnUsers)
                ContentFrame.Content = new UserAddControl(CurrentUser);

            else if (clicked == BtnProcurement)
                ContentFrame.Content = new ProcurementPage(CurrentUser);
        }

        private void BtnLogout_Checked(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;

            var authPage = new AuthorizationPage();

            if (NavigationService != null)
                NavigationService.Navigate(authPage);
            else
                Window.GetWindow(this).Content = authPage;
        }
    }
}