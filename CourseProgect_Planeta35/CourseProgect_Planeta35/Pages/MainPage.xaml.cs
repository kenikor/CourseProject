// MainPage.xaml.cs
using CourseProgect_Planeta35.Controls;
using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CourseProgect_Planeta35.Pages
{
    public partial class MainPage : Page
    {
        private readonly User CurrentUser;
        private List<AssetCategory> AllCategories;
        private List<CourseProgect_Planeta35.Controls.InventoryItem> AllInventoryItems;


        public MainPage(User user)
        {
            InitializeComponent();
            CurrentUser = user;

            SetupAdminPanel();
            LoadDataFromDb();
            LoadDashboard();

            BtnDashboard.IsChecked = true;
            ConfigureInterface(user);
        }

        private void SetupAdminPanel()
        {
            AdminPanel.Visibility = (CurrentUser.RoleId == 1) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadDashboard()
        {
            ContentFrame.Content = new MainControl(CurrentUser);
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
                    .SelectMany(c => c.Assets, (c, asset) => new CourseProgect_Planeta35.Controls.InventoryItem
                    {
                        Asset = asset,
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки из БД: " + ex.Message);
                AllCategories = new List<AssetCategory>();
                AllInventoryItems = new List<CourseProgect_Planeta35.Controls.InventoryItem>();
            }
        }

        private void LoadInventory()
        {
            ContentFrame.Content = new InventoryListControl(CurrentUser);
        }
        private void ConfigureInterface(User user)
        {
            if (user == null) return;

            BtnProcurement.Visibility = Visibility.Collapsed;
            AdminPanel.Visibility = Visibility.Collapsed;

            switch (user.RoleId)
            {
                case 1: // admin
                    BtnProcurement.Visibility = Visibility.Visible;
                    AdminPanel.Visibility = Visibility.Visible;
                    break;

                case 2: // manager
                    BtnProcurement.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void MenuButton_Checked(object sender, RoutedEventArgs e)
        {
            var clicked = sender as ToggleButton;
            if (clicked == null) return;

            var allButtons = new ToggleButton[]
            {
                BtnDashboard, BtnInventory, BtnCheck, BtnReports, BtnProcurement
            };

            if (CurrentUser.RoleId == 1)
            {
                allButtons = new ToggleButton[]
                {
                    BtnDashboard, BtnInventory, BtnCheck, BtnReports,
                    BtnCategories, BtnDepartments, BtnUsers, BtnProcurement
                };
            }

            if (CurrentUser.RoleId == 3)
            {
                allButtons = new ToggleButton[] { BtnDashboard, BtnInventory, BtnCheck, BtnReports };

                foreach (var btn in allButtons)
                {
                    if (btn != clicked)
                        btn.IsChecked = false;
                }

                if (clicked == BtnDashboard) LoadDashboard();
                else if (clicked == BtnInventory) LoadInventory();

                return;
            }

            allButtons = new ToggleButton[]
            {
                BtnDashboard, BtnInventory, BtnCheck, BtnReports,
                BtnCategories, BtnDepartments, BtnUsers, BtnProcurement
            };

            foreach (var btn in allButtons)
            {
                if (btn != clicked)
                    btn.IsChecked = false;
            }

            if (clicked == BtnDashboard) LoadDashboard();
            else if (clicked == BtnInventory) LoadInventory();
            else if (clicked == BtnCheck)
            {
                ContentFrame.Content = new InventoryCheckControl(CurrentUser);
            }
            else if (clicked == BtnReports)
            {
                ContentFrame.Content = new ReportsControl(CurrentUser, new AppDbContext());
            }
            else if (clicked == BtnCategories)
            {
                ContentFrame.Content = new CategoriesControl(CurrentUser);
            }
            else if (clicked == BtnDepartments)
            {
                ContentFrame.Content = new DepartmentsControl(CurrentUser);
            }
            else if (clicked == BtnUsers)
            {
                ContentFrame.Content = new UserAddControl(CurrentUser);
            }
            else if (clicked == BtnProcurement)
            {
                ContentFrame.Content = new ProcurementPage(CurrentUser);
            }

            foreach (var btn in allButtons)
            {
                if (btn != clicked)
                    btn.IsChecked = false;
            }

            // Загружаем соответствующий контент
            if (clicked == BtnDashboard) LoadDashboard();
            else if (clicked == BtnInventory) LoadInventory();
            else if (clicked == BtnCheck)
            {
                ContentFrame.Content = new InventoryCheckControl(CurrentUser);
            }
            else if (clicked == BtnReports)
            {
                ContentFrame.Content = new ReportsControl(CurrentUser, new AppDbContext());
            }
            else if (clicked == BtnCategories)
            {
                ContentFrame.Content = new CategoriesControl(CurrentUser);
            }
            else if (clicked == BtnDepartments)
            {
                ContentFrame.Content = new DepartmentsControl(CurrentUser);
            }
            else if (clicked == BtnProcurement)
            {
                ContentFrame.Content = new DepartmentsControl(CurrentUser);
            }
            else if (clicked == BtnUsers)
            {
                ContentFrame.Content = new UserAddControl(CurrentUser);
            };
        }

        private void BtnLogout_Checked(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;

            var authPage = new AuthorizationPage();

            if (this.NavigationService != null)
            {
                NavigationService.Navigate(authPage);
            }
            else
            {
                Window.GetWindow(this).Content = authPage;
            }
        }
    }
}