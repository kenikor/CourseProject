using CourseProgect_Planeta35.Models;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace CourseProgect_Planeta35.Pages
{
    public partial class CheckoutPage : UserControl
    {
        private readonly User CurrentUser;
        private ObservableCollection<CartItem> _cart;
        private string _paymentMethod = "card";
        private static readonly Regex _digitsOnly = new Regex("^[0-9]+$");
        private bool _isFormatting;

        public CheckoutPage(ObservableCollection<CartItem> cart, User user)
        {
            InitializeComponent();

            _cart = cart;
            CurrentUser = user;
            LoadSummary();
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }

        private void LoadSummary()
        {
            SummaryList.ItemsSource = _cart;

            decimal total = _cart.Sum(c => c.Subtotal);
            TotalText.Text = $"{total:N0} ₽";
        }

        private void Card_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "card";
            CardPanel.Visibility = Visibility.Visible;
        }

        private void Cash_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "cash";
            CardPanel.Visibility = Visibility.Collapsed;
        }

        private void Invoice_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "invoice";
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

            string orderNumber = GenerateOrderNumber();

            MessageBox.Show(
                $"Заказ оформлен!\n\nНомер заказа: {orderNumber}",
                "Успешно",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            _cart.Clear();
            AddressBox.Text = "";
            LoadSummary();

            var navService = NavigationService.GetNavigationService(this);
            if (navService != null && navService.CanGoBack)
            {
                navService.GoBack();
            }
        }

        private void CVV_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (!Regex.IsMatch(e.Text, "^[0-9]+$"))
            {
                e.Handled = true;
                return;
            }

            if (textBox.Text.Length >= 3)
            {
                e.Handled = true;
            }
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

        private void Expiry_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void Expiry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormatting) return;

            var textBox = (TextBox)sender;

            string digits = Regex.Replace(textBox.Text, @"\D", "");

            if (digits.Length > 4)
                digits = digits.Substring(0, 4);

            string formatted = digits;

            if (digits.Length > 2)
            {
                formatted = digits.Insert(2, "/");
            }

            int caret = textBox.CaretIndex;

            _isFormatting = true;
            textBox.Text = formatted;
            _isFormatting = false;

            textBox.CaretIndex = Math.Min(formatted.Length, caret);
        }

        private void CardHolder_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[a-zA-Z ]+$");
        }

        private void CardHolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            int caret = textBox.CaretIndex;

            string upper = textBox.Text.ToUpper();

            if (textBox.Text != upper)
            {
                textBox.Text = upper;
                textBox.CaretIndex = caret;
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

            if (digits.Length > 16)
                digits = digits.Substring(0, 16);

            string formatted = Regex.Replace(digits, ".{4}", "$0 ").TrimEnd();

            int caretIndex = textBox.CaretIndex;

            _isFormatting = true;
            textBox.Text = formatted;
            _isFormatting = false;

            textBox.CaretIndex = Math.Min(formatted.Length, caretIndex);
        }
    }
}