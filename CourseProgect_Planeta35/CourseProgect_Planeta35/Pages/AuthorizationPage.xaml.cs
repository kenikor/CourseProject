using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace CourseProgect_Planeta35.Pages
{
    public partial class AuthorizationPage : System.Windows.Controls.Page
    {
        private bool IsLogin = true;

        public AuthorizationPage()
        {
            InitializeComponent();
            LoginTab.IsChecked = true;

            StartLiquidAnimation();
        }

        private void StartLiquidAnimation()
        {
            AnimateBrush(Brush1, 12, 0.0, 1.0);
            AnimateBrush(Brush2, 16, 1.0, 0.0);
            AnimateBrush(Brush3, 9, 0.2, 0.9);
        }

        private void AnimateBrush(RadialGradientBrush brush, double seconds, double from, double to)
        {
            var originAnim = new PointAnimation
            {
                From = new Point(from, from),
                To = new Point(to, to),
                Duration = TimeSpan.FromSeconds(seconds),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            var centerAnim = new PointAnimation
            {
                From = new Point(0.4, 0.4),
                To = new Point(0.6, 0.6),
                Duration = TimeSpan.FromSeconds(seconds * 1.3),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            brush.BeginAnimation(RadialGradientBrush.GradientOriginProperty, originAnim);
            brush.BeginAnimation(RadialGradientBrush.CenterProperty, centerAnim);
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

            System.Windows.Controls.Page mainPage = null;

            if (IsLogin)
            {
                var user = db.Users.FirstOrDefault(u => u.Username == email);
                if (user == null || user.PasswordHash != passwordHash)
                {
                    ShowError("Неверный логин или пароль");
                    return;
                }

                // Устанавливаем текущего пользователя
                App.CurrentUser = user;

                // Создаём страницу
                mainPage = new MainPage(user);
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

                // Авторизуем сразу после регистрации
                App.CurrentUser = newUser;

                MessageBox.Show($"Пользователь {name} успешно зарегистрирован!");

                mainPage = new MainPage(newUser);
            }

            // Навигация на главную страницу
            if (mainPage != null)
            {
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
