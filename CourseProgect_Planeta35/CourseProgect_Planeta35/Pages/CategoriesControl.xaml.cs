using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        // ============================
        // ЗАГРУЗКА КАТЕГОРИЙ ИЗ БД
        // ============================
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
                        string color = string.IsNullOrWhiteSpace(cat.Color)
                            ? "#366B2B"
                            : cat.Color;

                        AddCategoryCard(cat.Id, cat.Name, color);
                    }

                    UpdateCount();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий:\n" + ex.Message);
            }
        }

        // =======================
        // ДОБАВЛЕНИЕ КАТЕГОРИИ
        // =======================
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
                        Description = "",
                        Color = win.ColorHex
                    };

                    db.AssetCategories.Add(entity);
                    db.SaveChanges();

                    // после сохранения появится ID
                    AddCategoryCard(entity.Id, entity.Name, entity.Color);
                }

                UpdateCount();
            }
        }

        private void UpdateCount()
        {
            CountText.Text = $"{CategoryPanel.Children.Count} категорий";
        }

        // ==========================================
        // СОЗДАНИЕ КАРТОЧКИ КАТЕГОРИИ (UI элемент)
        // ==========================================
        private void AddCategoryCard(int id, string name, string colorHex)
        {
            Color color = (Color)ColorConverter.ConvertFromString(colorHex);
            Color lightColor = Color.FromArgb(40, color.R, color.G, color.B);

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

            // LEFT
            StackPanel left = new StackPanel { Orientation = Orientation.Horizontal };

            Border colorOuter = new Border
            {
                Width = 40,
                Height = 40,
                Background = new SolidColorBrush(lightColor),
                CornerRadius = new CornerRadius(20)
            };

            Border colorInner = new Border
            {
                Width = 20,
                Height = 20,
                Background = new SolidColorBrush(color),
                CornerRadius = new CornerRadius(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            colorOuter.Child = colorInner;
            left.Children.Add(colorOuter);

            StackPanel text = new StackPanel { Margin = new Thickness(12, 0, 0, 0) };
            text.Children.Add(new TextBlock { Text = name, FontWeight = FontWeights.Bold });
            text.Children.Add(new TextBlock { Text = colorHex.ToUpper(), Foreground = Brushes.Gray });

            left.Children.Add(text);

            // RIGHT BUTTONS
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
                int categoryId = (int)card.Tag;

                using (var db = new AppDbContext())
                {
                    var entity = db.AssetCategories.FirstOrDefault(x => x.Id == categoryId);
                    if (entity != null)
                    {
                        db.AssetCategories.Remove(entity);
                        db.SaveChanges();
                    }
                }

                CategoryPanel.Children.Remove(card);
                UpdateCount();
            };

            right.Children.Add(edit);
            right.Children.Add(delete);

            grid.Children.Add(left);
            Grid.SetColumn(right, 1);
            grid.Children.Add(right);

            card.Child = grid;

            CategoryPanel.Children.Add(card);
        }
    }
}