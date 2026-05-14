using CourseProgect_Planeta35.Models;
using CourseProgect_Planeta35.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows.Input;

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

        // 🔢 Генерация номера заказа
        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }

        // 📧 Отправка письма
        private void SendEmail(string toEmail, string orderNumber, string orderDetails)
        {
            var fromAddress = new MailAddress("yourmail@gmail.com", "Planeta35");
            var toAddress = new MailAddress(toEmail);

            const string fromPassword = "YOUR_APP_PASSWORD"; // 🔴 ВСТАВЬ APP PASSWORD

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            string subject = $"Ваш заказ {orderNumber}";

            string body = $@"
Спасибо за заказ!

Номер заказа: {orderNumber}

Состав заказа:
{orderDetails}

Адрес доставки:
{AddressBox.Text}

Способ оплаты:
{_paymentMethod}

Мы скоро свяжемся с вами 🚀
";

            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            smtp.Send(message);
        }

        // 🧾 Сбор заказа
        private string BuildOrderDetails()
        {
            var sb = new StringBuilder();

            foreach (var item in _cart)
            {
                sb.AppendLine($"{item.Item.Name} x{item.Quantity} = {item.Subtotal:N0} ₽");
            }

            return sb.ToString();
        }

        // 📦 Загрузка корзины
        private void LoadSummary()
        {
            SummaryList.ItemsSource = _cart;

            decimal total = _cart.Sum(c => c.Subtotal);
            TotalText.Text = $"{total:N0} ₽";
        }

        // 💳 Методы оплаты
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

        // 🚀 Оформление заказа
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

            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.Username))
            {
                MessageBox.Show("Email пользователя не найден");
                return;
            }

            string orderNumber = GenerateOrderNumber();
            string details = BuildOrderDetails();
            string userEmail = CurrentUser.Username;

            try
            {
                SendEmail(userEmail, orderNumber, details);

                MessageBox.Show($"Заказ оформлен!\nНомер: {orderNumber}");

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
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка отправки: " + ex.Message);
            }
        }

        // 🔢 Только цифры
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

        // 💳 Формат карты
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