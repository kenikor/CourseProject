// MainPage.xaml.cs
using CourseProgect_Planeta35.Controls;
using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
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
            using (var db = new AppDbContext())
            {
                // Загружаем все категории вместе с активами
                AllCategories = db.AssetCategories
                                  .Where(c => c.Assets.Any()) // только категории с активами
                                  .ToList();

                // Загружаем все InventoryItems
                AllInventoryItems = new List<InventoryItem>();

                foreach (var category in AllCategories)
                {
                    db.Entry(category).Collection(c => c.Assets).Load(); // загружаем активы категории
                    foreach (var asset in category.Assets)
                    {
                        // Если у тебя в InventoryItem нужны другие поля, заполняем их
                        AllInventoryItems.Add(new InventoryItem
                        {
                            Asset = asset,
                            // Например, ResponsiblePersonId = asset.ResponsibleId ?? 0
                        });
                    }
                }
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

            // Сброс всех кнопок, кроме той, которую выбрали
            var allButtons = new ToggleButton[]
            {
                BtnDashboard, BtnInventory, BtnCheck, BtnReports
            };

            // Если админ, добавляем админские кнопки
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
            else if (clicked == BtnCheck) MessageBox.Show("Инвентаризация открыта");
            else if (clicked == BtnReports) MessageBox.Show("Отчёты открыты");
            else if (clicked == BtnCategories) MessageBox.Show("Категории доступны админу");
            else if (clicked == BtnDepartments) MessageBox.Show("Подразделения доступны админу");
            else if (clicked == BtnUsers) MessageBox.Show("Пользователи доступны админу");
        }
    }
}
