using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CourseProgect_Planeta35.Pages
{
    public partial class AuthorizationPage : Page
    {
        private bool IsLogin = true;

        public AuthorizationPage()
        {
            InitializeComponent();
            LoginTab.IsChecked = true;
        }

        private void LoginTab_Checked(object sender, RoutedEventArgs e)
        {
            if (LoginTab.IsChecked == true)
            {
                RegisterTab.IsChecked = false;
                RegisterNameField.Visibility = Visibility.Collapsed;
                IsLogin = true;
            }
        }

        private void RegisterTab_Checked(object sender, RoutedEventArgs e)
        {
            if (RegisterTab.IsChecked == true)
            {
                LoginTab.IsChecked = false;
                RegisterNameField.Visibility = Visibility.Visible;
                IsLogin = false;
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string name = NameBox.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Заполните все поля!");
                return;
            }

            using var db = new AppDbContext();

            string passwordHash = ComputeSha256Hash(password);

            if (IsLogin)
            {
                var user = db.Users.FirstOrDefault(u => u.Username == email);
                if (user == null || user.PasswordHash != passwordHash)
                {
                    ShowError("Неверный логин или пароль");
                    return;
                }

                var mainPage = new MainPage(user);

                if (this.NavigationService != null)
                {
                    NavigationService.Navigate(mainPage);
                }
                else
                {
                    Window.GetWindow(this).Content = mainPage;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(name))
                {
                    ShowError("Введите имя");
                    return;
                }

                if (db.Users.Any(u => u.Username == email))
                {
                    ShowError("Пользователь с таким email уже существует");
                    return;
                }

                var newUser = new User
                {
                    Username = email,
                    PasswordHash = passwordHash,
                    FullName = name,
                    RoleId = 2 // Сотрудник
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                MessageBox.Show($"Пользователь {name} успешно зарегистрирован!");
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        // Метод для вычисления SHA-256 хэша строки
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2")); // Преобразуем байты в hex
                return builder.ToString();
            }
        }

        private void GuestLogin_Click(object sender, RoutedEventArgs e)
        {
            var guestUser = new User
            {
                Username = "guest",
                FullName = "Гость",
                RoleId = 3 // допустим, роль гостя
            };

            var mainPage = new MainPage(guestUser);

            if (this.NavigationService != null)
            {
                NavigationService.Navigate(mainPage);
            }
            else
            {
                Window.GetWindow(this).Content = mainPage;
            }
        }
    }
}
