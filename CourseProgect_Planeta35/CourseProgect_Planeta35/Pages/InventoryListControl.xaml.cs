using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProgect_Planeta35.Controls
{
    public partial class InventoryListControl : UserControl
    {
        private readonly User CurrentUser;

        private List<InventoryItem> InventoryItems;   // теперь НЕ readonly
        private List<AssetCategory> Categories;       // тоже не readonly

        public InventoryListControl(User user)
        {
            InitializeComponent();

            CurrentUser = user ?? throw new ArgumentNullException(nameof(user));

            LoadDataFromDB();
            LoadFilters();
            LoadItems();
        }

        /// <summary>
        /// Загружаем реальные данные из БД
        /// </summary>
        private void LoadDataFromDB()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    // Загружаем категории
                    Categories = db.AssetCategories
                        .AsNoTracking()
                        .Include(c => c.Assets)
                        .ToList();

                    // Загружаем все Assets + Category + Responsible
                    var assets = db.Assets
                        .AsNoTracking()
                        .Include(a => a.Category)
                        .Include(a => a.Responsible)
                        .ToList();

                    // Конвертируем в InventoryItem
                    InventoryItems = assets
                        .Select(a => new InventoryItem
                        {
                            Asset = a
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки из БД: " + ex.Message);
                Categories = new List<AssetCategory>();
                InventoryItems = new List<InventoryItem>();
            }
        }


        private void LoadFilters()
        {
            CategoryFilter.Items.Clear();
            CategoryFilter.Items.Add("Все");

            foreach (var c in Categories)
            {
                if (c != null && !string.IsNullOrWhiteSpace(c.Name))
                    CategoryFilter.Items.Add(c.Name);
            }

            CategoryFilter.SelectedIndex = 0;
        }


        private void LoadItems()
        {
            if (InventoryItems == null) return;

            // ВСЕГДА показываем весь инвентарь
            var items = InventoryItems;

            string searchText = SearchBox.Text?.ToLower() ?? "";
            string categoryText = CategoryFilter.SelectedItem?.ToString() ?? "Все";
            string statusText = (StatusFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все";

            var filtered = items
                .Where(x =>
                    x.Asset != null &&
                    (string.IsNullOrEmpty(searchText) ||
                     (x.Asset.Name?.ToLower().Contains(searchText) ?? false) ||
                     (x.Asset.Status?.ToLower().Contains(searchText) ?? false)) &&
                    (categoryText == "Все" ||
                     x.Asset.Category?.Name == categoryText) &&
                    (statusText == "Все" || x.Asset.Status == statusText)
                )
                .ToList();

            ItemsGrid.ItemsSource = filtered;
            SubTitleText.Text = $"{filtered.Count} объектов найдено";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddAssetWindow
            {
                Owner = Window.GetWindow(this)
            };
            if (addWindow.ShowDialog() == true)
            {
                var newItem = new InventoryItem { Asset = addWindow.NewAsset };
                InventoryItems.Add(newItem);
                LoadItems();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is InventoryItem item && item.Asset != null)
            {
                if (MessageBox.Show($"Удалить {item.Asset.Name ?? "Не указано"}?", "Удаление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var db = new AppDbContext())
                        {
                            var asset = db.Assets.FirstOrDefault(a => a.Id == item.Asset.Id);
                            if (asset != null)
                            {
                                db.Assets.Remove(asset);
                                db.SaveChanges();
                            }
                        }

                        InventoryItems.Remove(item);
                        LoadItems();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка удаления: " + ex.Message);
                    }
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => LoadItems();
        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadItems();
        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadItems();
    }
}
