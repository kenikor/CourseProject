using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            ClearAddForm();
            editingUser = null;
            AddPanel.Visibility = Visibility.Visible;
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

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is User user)
            {
                editingUser = user;

                NameBox.Text = user.FullName;
                EmailBox.Text = user.Username;

                RoleBox.SelectedItem = _dbContext.Roles.FirstOrDefault(r => r.Id == user.RoleId);
                DepartmentBox.SelectedValue = user.DepartmentId;

                AddPanel.Visibility = Visibility.Visible;
            }
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
