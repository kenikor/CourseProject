using CourseProgect_Planeta35.Models;
using CourseProgect_Planeta35.Data;
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
using System.Text.RegularExpressions;
using System.Windows.Shapes;

namespace CourseProgect_Planeta35.Pages
{
    /// <summary>
    /// Логика взаимодействия для CheckoutPage.xaml
    /// </summary>
    public partial class CheckoutPage : UserControl
    {
        private ProcurementPage _procPage;
        private readonly User CurrentUser;
        private ObservableCollection<CartItem> _cart;
        private string _paymentMethod = "card";
        private static readonly Regex _digitsOnly = new Regex("^[0-9]+$");
        private bool _isFormatting;

        public CheckoutPage(ObservableCollection<CartItem> cart)
        {
            InitializeComponent();

            _cart = cart;

            LoadSummary();
        }

        private void LoadSummary()
        {
            SummaryList.ItemsSource = _cart;

            decimal total = _cart.Sum(c => c.Subtotal);
            TotalText.Text = $"Итого: {total:N0} ₽";
        }

        private void Card_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "card";
            CardPanel.Visibility = Visibility.Visible;
        }

        private void Invoice_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "invoice";
            CardPanel.Visibility = Visibility.Collapsed;
        }

        private void Cash_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "cash";
            CardPanel.Visibility = Visibility.Collapsed;
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (!_cart.Any())
            {
                MessageBox.Show("Корзина пустая");
                return;
            }

            if (string.IsNullOrWhiteSpace(AddressBox.Text))
            {
                MessageBox.Show("Введите адрес доставки");
                return;
            }

            MessageBox.Show("Заказ оформлен!");

            _cart.Clear();

            var nav = System.Windows.Navigation.NavigationService.GetNavigationService(this);
            if (nav != null && nav.CanGoBack)
            {
                nav.GoBack();
            }
            else
            {
                Window.GetWindow(this)?.Close();
            }
        }

        private void DigitsOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_digitsOnly.IsMatch(e.Text);
        }

        private void DigitsOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));

                if (!_digitsOnly.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void CardNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void CardNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormatting) return;

            var textBox = (TextBox)sender;

            string digits = Regex.Replace(textBox.Text, @"\D", "");

            if (digits.Length > 19)
                digits = digits.Substring(0, 19);

            string formatted = Regex.Replace(digits, ".{4}", "$0 ").TrimEnd();

            int caretIndex = textBox.CaretIndex;

            _isFormatting = true;
            textBox.Text = formatted;
            _isFormatting = false;

            textBox.CaretIndex = Math.Min(formatted.Length, caretIndex);
        }
    }
}
