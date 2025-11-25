using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseProgect_Planeta35.Controls
{
    public partial class InventoryCheckControl : UserControl
    {
        private readonly User CurrentUser;
        private List<InventoryItem> InventoryItems;
        private InventoryItem _selectedItem;
        private string _checkStatus = "present";

        public InventoryCheckControl(User currentUser)
        {
            InitializeComponent();
            CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            LoadDataFromDB();
            LoadItems();
        }

        private void LoadDataFromDB()
        {
            try
            {
                using var db = new AppDbContext();

                // Подгружаем Assets с категориями и ответственными
                var assets = db.Assets
                    .Include(a => a.Category)
                    .Include(a => a.Responsible)
                    .ToList();

                InventoryItems = assets
                    .Where(a => CurrentUser.RoleId == 1 || a.ResponsibleId == CurrentUser.Id)
                    .Select(a => new InventoryItem { Asset = a })
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки из БД: " + ex.Message);
                InventoryItems = new List<InventoryItem>();
            }
        }

        private void LoadItems()
        {
            if (InventoryItems == null) return;

            string query = SearchBox.Text?.ToLower() ?? "";

            var filtered = InventoryItems
                .Where(i => string.IsNullOrEmpty(query) ||
                            (i.Asset.Name?.ToLower().Contains(query) ?? false))
                .ToList();

            ItemsGrid.ItemsSource = filtered;
            SubTitleText.Text = $"{filtered.Count} объектов найдено";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadItems();
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is InventoryItem item)
            {
                _selectedItem = item;
                DetailPanel.Visibility = Visibility.Visible;
                ItemNameText.Text = item.Asset.Name;
                ItemCategoryText.Text = item.Asset.Category?.Name ?? "";
                NotesBox.Text = "";
                _checkStatus = "present";
            }
        }

        private void Status_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string status)
                _checkStatus = status;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;

            try
            {
                using var db = new AppDbContext();
                var check = new InventoryCheck
                {
                    ItemId = _selectedItem.Asset.Id,
                    Status = _checkStatus,
                    Notes = string.IsNullOrWhiteSpace(NotesBox.Text) ? null : NotesBox.Text,
                    CheckDate = DateTime.Now,
                    CheckedById = CurrentUser.Id
                };
                db.InventoryChecks.Add(check);
                db.SaveChanges();

                MessageBox.Show($"Объект \"{_selectedItem.Asset.Name}\" отмечен как {_checkStatus}!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }

            DetailPanel.Visibility = Visibility.Collapsed;
            _selectedItem = null;
            LoadItems();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DetailPanel.Visibility = Visibility.Collapsed;
            _selectedItem = null;
        }
    }
}
