using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using System.Linq;
using System.Windows;

namespace CourseProgect_Planeta35.Pages
{
    public partial class UserAddWindow : Window
    {
        private readonly AppDbContext _db;
        private readonly User editingUser;

        public User ResultUser { get; private set; }

        public UserAddWindow(AppDbContext db, User userToEdit = null)
        {
            InitializeComponent();

            _db = db;
            editingUser = userToEdit;

            RoleBox.ItemsSource = _db.Roles.ToList();
            DepartmentBox.ItemsSource = _db.Departments.ToList();

            if (editingUser != null)
            {
                NameBox.Text = editingUser.FullName;
                EmailBox.Text = editingUser.Username;

                RoleBox.SelectedValue = editingUser.RoleId;
                DepartmentBox.SelectedValue = editingUser.DepartmentId;
            }
            else
            {
                RoleBox.SelectedIndex = -1;
                DepartmentBox.SelectedIndex = -1;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                MessageBox.Show("Имя и Email обязательны.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedRoleId = RoleBox.SelectedValue as int?;
            if (!selectedRoleId.HasValue)
            {
                MessageBox.Show("Выберите роль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedDepartmentId = DepartmentBox.SelectedValue as int?;

            if (editingUser == null)
            {
                ResultUser = new User
                {
                    FullName = NameBox.Text,
                    Username = EmailBox.Text,
                    PasswordHash = "",
                    RoleId = selectedRoleId.Value,
                    DepartmentId = selectedDepartmentId
                };
                _db.Users.Add(ResultUser);
            }
            else
            {
                editingUser.FullName = NameBox.Text;
                editingUser.Username = EmailBox.Text;
                editingUser.RoleId = selectedRoleId.Value;
                editingUser.DepartmentId = selectedDepartmentId;

                ResultUser = editingUser;
            }

            _db.SaveChanges();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
