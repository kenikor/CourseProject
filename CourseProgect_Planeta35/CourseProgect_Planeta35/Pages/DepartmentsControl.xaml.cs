using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using CourseProgect_Planeta35.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CourseProgect_Planeta35.Pages
{
    public partial class DepartmentsControl : UserControl
    {
        private readonly User CurrentUser;
        public DepartmentsControl(User user)
        {
            InitializeComponent();
            CurrentUser = user ?? throw new ArgumentNullException(nameof(user));
            LoadDepartmentsFromDb();
        }

        private void LoadDepartmentsFromDb()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var list = db.Departments.ToList();
                    DepartmentsPanel.Children.Clear();

                    foreach (var dept in list)
                        AddDepartmentCard(dept.Id, dept.Name, dept.Location);

                    UpdateCount();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки подразделений:\n" + ex.Message);
            }
        }

        private void AddDepartment_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddDepartmentWindow();

            if (win.ShowDialog() == true)
            {
                using (var db = new AppDbContext())
                {
                    var entity = new Department
                    {
                        Name = win.DepartmentName,
                        Location = win.DepartmentDescription
                    };

                    db.Departments.Add(entity);
                    db.SaveChanges();

                    AddDepartmentCard(entity.Id, entity.Name, entity.Location);
                }

                UpdateCount();
            }
        }

        private void UpdateCount()
        {
            CountText.Text = $"{DepartmentsPanel.Children.Count} подразделений";
        }

        private void AddDepartmentCard(int id, string name, string description)
        {
            Border card = new Border
            {
                Width = 320,
                Height = 110,
                Background = Brushes.White,
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(15),
                Margin = new Thickness(10),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Opacity = 0.15,
                    BlurRadius = 10
                },
                Tag = id
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel left = new StackPanel { Orientation = Orientation.Horizontal };

            Border iconOuter = new Border
            {
                Width = 40,
                Height = 40,
                Background = new SolidColorBrush(Color.FromArgb(40, 55, 107, 43)),
                CornerRadius = new CornerRadius(20)
            };

            Border iconInner = new Border
            {
                Width = 20,
                Height = 20,
                Background = new SolidColorBrush(Color.FromRgb(55, 107, 43)),
                CornerRadius = new CornerRadius(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            iconOuter.Child = iconInner;
            left.Children.Add(iconOuter);

            StackPanel text = new StackPanel { Margin = new Thickness(12, 0, 0, 0) };
            text.Children.Add(new TextBlock { Text = name, FontWeight = FontWeights.Bold, FontSize = 16 });
            text.Children.Add(new TextBlock { Text = description, Foreground = Brushes.Gray, FontSize = 12 });

            left.Children.Add(text);

            StackPanel right = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button edit = new Button
            {
                Content = "✎",
                Width = 32,
                Height = 32,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.Gray,
                Margin = new Thickness(4)
            };

            Button delete = new Button
            {
                Content = "🗑",
                Width = 32,
                Height = 32,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.DarkRed,
                Margin = new Thickness(4)
            };

            delete.Click += (s, e) =>
            {
                int deptId = (int)card.Tag;
                using (var db = new AppDbContext())
                {
                    var entity = db.Departments.FirstOrDefault(d => d.Id == deptId);
                    if (entity != null)
                    {
                        db.Departments.Remove(entity);
                        db.SaveChanges();
                    }
                }
                DepartmentsPanel.Children.Remove(card);
                UpdateCount();
            };

            right.Children.Add(edit);
            right.Children.Add(delete);

            grid.Children.Add(left);
            Grid.SetColumn(right, 1);
            grid.Children.Add(right);

            card.Child = grid;

            DepartmentsPanel.Children.Add(card);
        }
    }
}
