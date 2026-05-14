using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CourseProgect_Planeta35.Pages
{
    public partial class CategoriesControl : UserControl
    {
        private readonly User CurrentUser;

        public CategoriesControl(User user)
        {
            InitializeComponent();
            CurrentUser = user ?? throw new ArgumentNullException(nameof(user));

            LoadCategoriesFromDb();
        }

        private void LoadCategoriesFromDb()
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var list = db.AssetCategories.ToList();

                    CategoryPanel.Children.Clear();

                    foreach (var cat in list)
                    {
                        AddCategoryCard(
                            cat.Id,
                            cat.Name,
                            cat.Description,
                            cat.Color
                        );
                    }

                    UpdateCount();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий:\n" + ex.Message);
            }
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddCategoryWindow();

            if (win.ShowDialog() == true)
            {
                using (var db = new AppDbContext())
                {
                    var entity = new AssetCategory
                    {
                        Name = win.CategoryName,
                        Description = win.CategoryDescription,
                        Color = win.ColorHex
                    };

                    db.AssetCategories.Add(entity);
                    db.SaveChanges();

                    AddCategoryCard(entity.Id, entity.Name, entity.Description, entity.Color);
                }

                UpdateCount();
            }
        }

        private void UpdateCount()
        {
            CountText.Text = $"{CategoryPanel.Children.Count} категорий";
        }

        private void AddCategoryCard(int id, string name, string description, string color)
        {
            Brush accent;

            try
            {
                accent = (Brush)new BrushConverter().ConvertFromString(color);
            }
            catch
            {
                accent = new SolidColorBrush(Color.FromRgb(54, 107, 43));
            }

            Border card = new Border
            {
                Width = 320,
                Height = 120,
                Background = (Brush)new BrushConverter().ConvertFromString("#1E2320"),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(15),
                Margin = new Thickness(10),
                Tag = id
            };

            Grid root = new Grid();

            // LEFT STRIPE
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
                Margin = new Thickness(20, 15, 15, 15)
            };

            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition());
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // HEADER
            StackPanel header = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            Ellipse dot = new Ellipse
            {
                Width = 14,
                Height = 14,
                Fill = accent
            };

            TextBlock title = new TextBlock
            {
                Text = name,
                Margin = new Thickness(10, 0, 0, 0),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };

            header.Children.Add(dot);
            header.Children.Add(title);

            Grid.SetRow(header, 0);

            // DESCRIPTION
            TextBlock desc = new TextBlock
            {
                Text = description,
                Margin = new Thickness(0, 10, 0, 0),
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap
            };

            Grid.SetRow(desc, 1);

            // DELETE
            Button delete = new Button
            {
                Content = "🗑",
                Width = 30,
                Height = 30,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Tag = id
            };

            delete.Click += (s, e) =>
            {
                int catId = (int)delete.Tag;

                using (var db = new AppDbContext())
                {
                    var entity = db.AssetCategories.FirstOrDefault(x => x.Id == catId);
                    if (entity != null)
                    {
                        db.AssetCategories.Remove(entity);
                        db.SaveChanges();
                    }
                }

                CategoryPanel.Children.Remove(card);
                UpdateCount();
            };

            Grid.SetRow(delete, 2);

            content.Children.Add(header);
            content.Children.Add(desc);

            root.Children.Add(content);
            root.Children.Add(delete);

            card.Child = root;

            CategoryPanel.Children.Add(card);
        }
    }
}