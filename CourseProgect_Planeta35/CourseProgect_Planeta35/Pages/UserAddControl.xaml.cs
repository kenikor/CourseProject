using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CourseProgect_Planeta35.Pages
{
    public partial class UserAddControl : UserControl
    {
        private readonly AppDbContext _dbContext;
        private readonly User CurrentUser;
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
        private User editingUser = null;

        public UserAddControl(User user)
        {
            InitializeComponent();

            _dbContext = new AppDbContext();
            CurrentUser = user ?? throw new ArgumentNullException(nameof(user));

            LoadUsers();
            UsersList.ItemsSource = Users;

            RoleBox.ItemsSource = _dbContext.Roles.ToList();
            RoleBox.DisplayMemberPath = "Name";
            RoleBox.SelectedValuePath = "Id";

            DepartmentBox.ItemsSource = _dbContext.Departments.ToList();
            DepartmentBox.DisplayMemberPath = "Name";
            DepartmentBox.SelectedValuePath = "Id";

            this.Loaded += (s, e) => StartBackgroundAnimation();
        }

        private void StartBackgroundAnimation()
        {
            AnimateBrush(BgBrush1, 40, 0.2, 0.6);
            AnimateBrush(BgBrush2, 50, 0.8, 0.4);
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

        private void LoadUsers()
        {
            Users.Clear();
            var userEntities = _dbContext.Users
                .ToList();

            foreach (var u in userEntities)
                Users.Add(u);
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new UserAddWindow(_dbContext);
            addWindow.Owner = Window.GetWindow(this);

            if (addWindow.ShowDialog() == true)
            {
                Users.Add(addWindow.ResultUser);
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is User user)
            {
                var editWindow = new UserAddWindow(_dbContext, user);
                editWindow.Owner = Window.GetWindow(this);

                if (editWindow.ShowDialog() == true)
                {
                    // Обновляем отображаемый список
                    int idx = Users.IndexOf(user);
                    Users[idx] = editWindow.ResultUser;
                }
            }
        }

        private void CancelAdd_Click(object sender, RoutedEventArgs e)
        {
            ClearAddForm();
            AddPanel.Visibility = Visibility.Collapsed;
        }

        private void SaveNewUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                MessageBox.Show("Имя и Email обязательны.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedRole = RoleBox.SelectedItem as Role;
            if (selectedRole == null)
            {
                MessageBox.Show("Выберите роль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var departmentId = DepartmentBox.SelectedValue as int?;

            if (editingUser == null)
            {
                var newUser = new User
                {
                    FullName = NameBox.Text,
                    Username = EmailBox.Text,
                    PasswordHash = "",
                    RoleId = selectedRole.Id,
                    DepartmentId = departmentId
                };

                _dbContext.Users.Add(newUser);
                _dbContext.SaveChanges();
                Users.Add(newUser);
            }
            else
            {
                editingUser.FullName = NameBox.Text;
                editingUser.Username = EmailBox.Text;
                editingUser.RoleId = selectedRole.Id;
                editingUser.DepartmentId = departmentId;

                _dbContext.SaveChanges();
            }

            ClearAddForm();
            AddPanel.Visibility = Visibility.Collapsed;
            editingUser = null;
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is User user)
            {
                var result = MessageBox.Show($"Удалить пользователя {user.FullName}?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _dbContext.Users.Remove(user);
                    _dbContext.SaveChanges();
                    Users.Remove(user);
                }
            }
        }

        private void ClearAddForm()
        {
            NameBox.Text = "";
            EmailBox.Text = "";
            RoleBox.SelectedIndex = -1;
            DepartmentBox.SelectedIndex = -1;
        }
    }
}
