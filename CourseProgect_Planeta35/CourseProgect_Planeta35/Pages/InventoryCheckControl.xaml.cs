using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using CourseProgect_Planeta35.Pages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CourseProgect_Planeta35.Controls
{
    public partial class InventoryCheckControl : UserControl
    {
        private readonly User CurrentUser;
        private List<InventoryItem> AllItems = new();

        public InventoryCheckControl(User currentUser)
        {
            InitializeComponent();
            LoadData();
            LoadItems();
            UpdateProgress(); // сразу показать 0/total
            CurrentUser = currentUser;
        }

        private void LoadData()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var assets = db.Assets
                        .AsNoTracking()
                        .Include(x => x.Category)
                        .Include(x => x.Responsible)
                        .ToList();

                    AllItems = assets.Select(a => new InventoryItem { Asset = a }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
                AllItems = new();
            }
        }

        private void LoadItems()
        {
            string search = SearchBox.Text?.ToLower() ?? "";

            var filtered = AllItems.Where(x =>
                x.Asset != null &&
                (string.IsNullOrEmpty(search) ||
                 (x.Asset.Name?.ToLower().Contains(search) ?? false) ||
                 (x.Asset.InventoryNumber?.ToLower().Contains(search) ?? false))
                ).ToList();

            ItemsList.ItemsSource = filtered;

            FoundCountText.Text = $"Найдено объектов: {filtered.Count}";
            UpdateProgress();
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is InventoryItem item && item.Asset != null)
            {
                var win = new AssetCheckWindow(item.Asset)
                {
                    Owner = Window.GetWindow(this)
                };

                if (win.ShowDialog() == true)
                {
                    LoadData();
                    LoadItems();
                }
            }
        }

        private void UpdateProgress()
        {
            int total = AllItems.Count;
            int checkedCount = AllItems.Count(x => x.Asset != null && x.Asset.IsChecked);

            // raw numbers
            double progress = total == 0 ? 0 : (double)checkedCount / total;
            int percent = (int)(progress * 100);

            ProgressPercentText.Text = $"{percent}%";

            // анимация прогресс-бара
            double targetWidth = 140 * progress;

            ProgressBarFill.Width = targetWidth;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadItems();
        }

        // При смене чекбокса — сохраняем в БД
        private void CheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is CheckBox cb)
                {
                    if (cb.DataContext is InventoryItem item && item?.Asset != null)
                    {
                        bool newValue = cb.IsChecked == true;

                        // Сохраняем прямо в БД
                        using (var db = new AppDbContext())
                        {
                            var asset = db.Assets.FirstOrDefault(a => a.Id == item.Asset.Id);
                            if (asset != null)
                            {
                                asset.IsChecked = newValue;
                                db.SaveChanges();
                            }
                        }

                        // Обновляем локальную модель (чтобы UI и расчет прогресса были корректны)
                        var local = AllItems.FirstOrDefault(x => x.Asset.Id == item.Asset.Id);
                        if (local != null)
                            local.Asset.IsChecked = newValue;

                        UpdateProgress();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении статуса: " + ex.Message);
            }
        }
    }
}
