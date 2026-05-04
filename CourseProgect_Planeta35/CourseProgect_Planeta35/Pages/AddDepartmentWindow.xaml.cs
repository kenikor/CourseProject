using System.Windows;
using System.Windows.Input;

namespace CourseProgect_Planeta35.Controls
{
    public partial class AddDepartmentWindow : Window
    {
        public string DepartmentName { get; private set; }
        public string DepartmentDescription { get; private set; }

        public AddDepartmentWindow()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Название обязательно");
                return;
            }

            DepartmentName = NameBox.Text.Trim();
            DepartmentDescription = DescriptionBox.Text.Trim();

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
