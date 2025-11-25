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
        private List<InventoryItem> AllInventoryItems;


        public MainPage(User user)
        {
            InitializeComponent();
            CurrentUser = user;

            SetupAdminPanel();
            LoadDataFromDb();  
            LoadDashboard();

            BtnDashboard.IsChecked = true;
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

                // Загружаем все категории с активами и ответственными
                AllCategories = db.AssetCategories
                    .Include(c => c.Assets)
                        .ThenInclude(a => a.Responsible)
                    .Where(c => c.Assets.Any()) // только категории с активами
                    .AsNoTracking()
                    .ToList();

                // Загружаем все InventoryItems
                AllInventoryItems = AllCategories
                    .SelectMany(c => c.Assets, (c, asset) => new InventoryItem
                    {
                        Asset = asset,
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки из БД: " + ex.Message);
                AllCategories = new List<AssetCategory>();
                AllInventoryItems = new List<InventoryItem>();
            }
        }

        private void LoadInventory()
        {
            ContentFrame.Content = new InventoryListControl(CurrentUser);
        }

        private void MenuButton_Checked(object sender, RoutedEventArgs e)
        {
            var clicked = sender as ToggleButton;
            if (clicked == null) return;

            var allButtons = new ToggleButton[]
            {
                BtnDashboard, BtnInventory, BtnCheck, BtnReports
            };

            if (CurrentUser.RoleId == 1)
            {
                allButtons = new ToggleButton[]
                {
                    BtnDashboard, BtnInventory, BtnCheck, BtnReports,
                    BtnCategories, BtnDepartments, BtnUsers
                };
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
            else if (clicked == BtnReports) MessageBox.Show("Отчёты открыты");
            else if (clicked == BtnCategories) MessageBox.Show("Категории доступны админу");
            else if (clicked == BtnDepartments) MessageBox.Show("Подразделения доступны админу");
            else if (clicked == BtnUsers) MessageBox.Show("Пользователи доступны админу");
        }
    }
}
