using CourseProgect_Planeta35.Controls;
using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CourseProgect_Planeta35.Pages
{
    public partial class DepartmentsControl : UserControl
    {
        private readonly User CurrentUser;

        public DepartmentsControl(User user)
        {
            InitializeComponent();

            CurrentUser = user ??
                throw new ArgumentNullException(nameof(user));

            LoadDepartmentsFromDb();
        }

        private void LoadDepartmentsFromDb()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var list = db.Departments.ToList();

                    DepartmentsList.Items.Clear();

                    foreach (var dept in list)
                    {
                        AddDepartmentCard(
                            dept.Id,
                            dept.Name,
                            dept.Location,
                            dept.Color
                        );
                    }

                    UpdateCount();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки подразделений:\n" +
                    ex.Message);
            }
        }

        private void AddDepartment_Click(
            object sender,
            RoutedEventArgs e)
        {
            var win = new AddDepartmentWindow();

            if (win.ShowDialog() == true)
            {
                using (var db = new AppDbContext())
                {
                    var entity = new Department
                    {
                        Name = win.DepartmentName,
                        Location = win.DepartmentDescription,
                        Color = win.DepartmentColor
                    };

                    db.Departments.Add(entity);

                    db.SaveChanges();

                    AddDepartmentCard(
                        entity.Id,
                        entity.Name,
                        entity.Location,
                        entity.Color
                    );
                }

                UpdateCount();
            }
        }

        private void UpdateCount()
        {
            CountText.Text =
                $"{DepartmentsList.Items.Count} подразделений";
        }

        private void AddDepartmentCard(int id, string name, string description, string color)
        {
            Brush accent = (Brush)new BrushConverter().ConvertFromString(color);

            Border card = new Border
            {
                Style = (Style)FindResource("DepartmentCardStyle"),
                Tag = id
            };

            Grid root = new Grid();

            Border stripe = new Border
            {
                Width = 8,
                HorizontalAlignment = HorizontalAlignment.Left,
                CornerRadius = new CornerRadius(20, 0, 0, 20),
                Background = accent
            };
            root.Children.Add(stripe);

            Grid content = new Grid
            {
                Margin = new Thickness(22, 16, 18, 12)
            };

            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            StackPanel header = new StackPanel { Orientation = Orientation.Horizontal };
            Ellipse dot = new Ellipse
            {
                Width = 14,
                Height = 14,
                Fill = accent,
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock title = new TextBlock
            {
                Text = name,
                Margin = new Thickness(10, 0, 0, 0),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            header.Children.Add(dot);
            header.Children.Add(title);
            Grid.SetRow(header, 0); 

            TextBlock desc = new TextBlock
            {
                Text = description,
                Margin = new Thickness(0, 10, 0, 15),
                Foreground = (Brush)new BrushConverter().ConvertFromString("#C7D2CB"),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetRow(desc, 1);

            Border footer = new Border
            {
                Background = (Brush)new BrushConverter().ConvertFromString("#273029"),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 6, 10, 6),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            StackPanel footerContent = new StackPanel { Orientation = Orientation.Horizontal };
            Ellipse statusDot = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = accent,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock footerText = new TextBlock
            {
                Text = "Активно",
                Foreground = accent,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            footerContent.Children.Add(statusDot);
            footerContent.Children.Add(footerText);
            footer.Child = footerContent;

            Grid.SetRow(footer, 2);

            Button delete = new Button
            {
                Content = "🗑",
                Width = 32,
                Height = 32,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.White,
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 6, 6, 0) 
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
                DepartmentsList.Items.Remove(card);
                UpdateCount();
            };

            content.Children.Add(header);
            content.Children.Add(desc);
            content.Children.Add(footer);

            root.Children.Add(content);
            root.Children.Add(delete);

            card.Child = root;

            DepartmentsList.Items.Add(card);
        }
    }
}
