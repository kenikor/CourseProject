using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using CourseProgect_Planeta35.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CourseProgect_Planeta35.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProcurementPage.xaml
    /// </summary>
    public partial class ProcurementPage : UserControl
    {
        private List<ProcurementItem> _allItems;
        public ObservableCollection<CartItem> Cart { get; set; } = new ObservableCollection<CartItem>();
        public ProcurementPage(User user)
        {
            InitializeComponent();
            LoadData();
            LoadFilters();
            ApplyFilters();

            OpenCart();
        }
        
        private void OpenCart()
        {
            CartColumn.Width = new GridLength(300);
        }

        private void LoadData()
        {
            using (var db = new AppDbContext())
            {
                _allItems = db.ProcurementItems.ToList();
            }
        }

        private void LoadFilters()
        {
            CategoryFilter.Items.Clear();
            CategoryFilter.Items.Add("Все категории");
            var cats = _allItems.Select(i => i.Category).Distinct();
            foreach (var c in cats) CategoryFilter.Items.Add(c);
            CategoryFilter.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            if (_allItems == null) return;

            string search = SearchBox.Text.ToLower();
            string category = CategoryFilter.SelectedItem?.ToString();

            var filtered = _allItems.Where(i =>
                (string.IsNullOrEmpty(search) || i.Name.ToLower().Contains(search)) &&
                (category == "Все категории" || i.Category == category)
            ).ToList();

            ItemsList.ItemsSource = filtered;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var item = _allItems.FirstOrDefault(i => i.Id == btn.Tag.ToString());

            if (item != null)
            {
                var existing = Cart.FirstOrDefault(c => c.Item.Id == item.Id);
                if (existing != null)
                    existing.Quantity++;
                else
                    Cart.Add(new CartItem { Item = item, Quantity = 1 });

                UpdateCartUI();
            }
        }

        private void UpdateCartUI()
        {
            CartListBox.ItemsSource = null;
            CartListBox.ItemsSource = Cart;

            decimal total = Cart.Sum(c => c.Subtotal);
            int count = Cart.Sum(c => c.Quantity);

            TotalSumText.Text = $"Итого: {total:N0} ₽";
            CartCountText.Text = count.ToString();
            BadgeBorder.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ToggleCart_Click(object sender, RoutedEventArgs e)
        {
            CartColumn.Width = (CartColumn.Width.Value == 0) ? new GridLength(300) : new GridLength(0);
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (!Cart.Any()) return;

            var main = Application.Current.MainWindow as MainWindow;
            if (main != null)
            {
                main.MainFrame.Content = new CheckoutPage(Cart);
            }

            CloseCart();
        }

        private void CloseCart()
        {
            CartColumn.Width = new GridLength(0);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
    }
}
