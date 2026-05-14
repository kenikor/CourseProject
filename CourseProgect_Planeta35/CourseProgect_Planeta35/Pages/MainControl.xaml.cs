using CourseProgect_Planeta35.Controls;
using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using CourseProgect_Planeta35.Pages;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace CourseProgect_Planeta35
{
    public partial class MainControl : UserControl
    {
        private readonly AppDbContext _db;
        public User CurrentUser { get; }
        public List<Asset> Items { get; set; }
        public List<AssetCategory> Categories { get; set; }
        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }
        public SolidColorPaint LegendPaint { get; set; }

        public event Action<string> NavigateRequested;

        public MainControl(User user)
        {
            InitializeComponent();
            CurrentUser = user;
            _db = new AppDbContext();
            Items = _db.Assets.ToList();

            Categories = _db.AssetCategories.ToList();

            LoadStats();
            LoadCategories();
            InitChart();
            DataContext = this;

            var timer = new System.Windows.Threading.DispatcherTimer();
            var rnd = new Random();

            timer.Interval = TimeSpan.FromSeconds(2);

            timer.Tick += (s, e) =>
            {
                var list = Series[0] as LineSeries<int>;
                if (list?.Values is IList<int> values)
                {
                    values.Add(rnd.Next(2, 12));
                    if (values.Count > 10)
                        values.RemoveAt(0);
                }
            };

            timer.Start();
        }

        private void InitChart()
        {
            var rnd = new Random();
            LegendPaint = new SolidColorPaint(new SKColor(230, 230, 230));

            var data = Enumerable.Range(1, 7)
                .Select(_ => rnd.Next(2, 12))
                .ToList();

            Series = new ISeries[]
            {
                new LineSeries<int>
                {
                    Name = "Активность системы",

                    Values = data,

                    Stroke = new SolidColorPaint(
                        new SKColor(139, 155, 76), 
                        4),

                    GeometryFill = new SolidColorPaint(
                        new SKColor(62, 92, 76)), 

                    GeometryStroke = new SolidColorPaint(SKColors.White, 2),

                    GeometrySize = 10,

                    Fill = null,

                    DataLabelsPaint =
                        new SolidColorPaint(SKColors.White)
                }
                    };

                    XAxes = new[]
                    {
                new Axis
                {
                    Labels = new[]
                    {
                        "Янв","Фев","Мар",
                        "Апр","Май","Июн","Июл"
                    },

                    LabelsPaint =
                        new SolidColorPaint(SKColors.LightGray),

                    TextSize = 12,

                    SeparatorsPaint =
                        new SolidColorPaint(
                            new SKColor(50, 60, 55))
                }
            };

            YAxes = new[]
            {
                new Axis
                {
                    LabelsPaint =
                        new SolidColorPaint(SKColors.LightGray),

                    TextSize = 12,

                    SeparatorsPaint =
                        new SolidColorPaint(
                            new SKColor(50, 60, 55))
                }
            };
        }

        private void LoadStats()
        {
            TotalObjectsText.Text = Items.Count.ToString();
            ActiveObjectsText.Text = Items.Count(i => i.Status == "В эксплуатации").ToString();
            MaintenanceObjectsText.Text = Items.Count(i => i.Status == "На обслуживании").ToString();
            TotalValueText.Text = $"{Items.Sum(i => i.Cost ?? 0):N0} ₽";
        }

        private void LoadCategories()
        {
            CategoriesPanel.Children.Clear();
            int total = Items.Count;

            foreach (var cat in Categories)
            {
                var catItems = Items.Where(i => i.CategoryId == cat.Id).ToList();
                if (catItems.Count == 0) continue;

                double percentage = total > 0 ? (double)catItems.Count / total * 100 : 0;

                StackPanel sp = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };

                StackPanel header = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch };
                header.Children.Add(new TextBlock { Text = cat.Name, Width = 160, Foreground = new SolidColorBrush(Color.FromRgb(230, 239, 230)), FontWeight = FontWeights.SemiBold });
                header.Children.Add(new TextBlock { Text = $"{catItems.Count} объектов", Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7A35")) });

                sp.Children.Add(header);

                sp.Children.Add(new ProgressBar
                {
                    Minimum = 0,
                    Maximum = 100,
                    Value = percentage,
                    Height = 10,
                    Background = new SolidColorBrush(Color.FromRgb(30, 35, 32)),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B9B4C")),
                    BorderThickness = new Thickness(0),
                    Margin = new Thickness(0, 4, 0, 0)
                });

                CategoriesPanel.Children.Add(sp);
            }
        }
    }
}
