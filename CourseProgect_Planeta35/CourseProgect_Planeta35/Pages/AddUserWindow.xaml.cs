using System.Windows;
using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;

namespace CourseProgect_Planeta35.Pages
{
    public partial class AddUserWindow : Window
    {
        public User EditingUser { get; set; }

        public AddUserWindow()
        {
            InitializeComponent();
            LoadData();
        }

        public AddUserWindow(User userToEdit)
        {
            InitializeComponent();
            EditingUser = userToEdit;
            LoadData();

            // Заполняем поля
            TbFullName.Text = userToEdit.FullName;
            TbUsername.Text = userToEdit.Username;
            CbRole.SelectedValue = userToEdit.RoleId;
            CbDepartment.SelectedValue = userToEdit.DepartmentId;
            FormTitle.Text = "Редактирование пользователя";
        }

        private void LoadData()
        {
            using var db = new AppDbContext();
            CbRole.ItemsSource = db.Roles.ToList();
            CbDepartment.ItemsSource = db.Departments.ToList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();

            if (EditingUser == null)
            {
                // СОЗДАЁМ
                var newUser = new User
                {
                    FullName = TbFullName.Text,
                    Username = TbUsername.Text,
                    PasswordHash = PbPassword.Password,
                    RoleId = (int)CbRole.SelectedValue,
                    DepartmentId = (int?)CbDepartment.SelectedValue
                };

                db.Users.Add(newUser);
            }
            else
            {
                // РЕДАКТИРУЕМ
                var user = db.Users.Find(EditingUser.Id);

                user.FullName = TbFullName.Text;
                user.Username = TbUsername.Text;

                if (!string.IsNullOrEmpty(PbPassword.Password))
                    user.PasswordHash = PbPassword.Password;

                user.RoleId = (int)CbRole.SelectedValue;
                user.DepartmentId = (int?)CbDepartment.SelectedValue;
            }

            db.SaveChanges();
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
