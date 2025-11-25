using CourseProgect_Planeta35.Models;
using CourseProgect_Planeta35.Services;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace CourseProgect_Planeta35.Controls
{
    public partial class ReportsControl : UserControl
    {
        private readonly User _currentUser;

        private List<InventoryItem> _allItems;
        private List<AssetCategory> _categories;
        private List<Department> _departments;
        private List<User> _users;
        private List<InventoryCheck> _checks;
        private List<Role> _roles;

        public string PreviewInfo { get; set; }

        public ReportsControl(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;

            LoadData();
            DataContext = this;

            CbReportType.SelectedIndex = 0;
            CbStatus.SelectedIndex = 0;

            RefreshPreview();
        }

        private void LoadData()
        {
            _allItems = StorageService.GetInventoryItems();
            _categories = StorageService.GetCategories();
            _departments = StorageService.GetDepartments();
            _users = StorageService.GetUsers();
            _checks = StorageService.GetInventoryChecks();
            _roles = StorageService.GetRoles();

            CbCategory.ItemsSource = new[] { new { Id = 0, Name = "Все категории" } }
                .Concat(_categories.Select(c => new { Id = c.Id, Name = c.Name }))
                .ToList();

            CbCategory.SelectedValuePath = "Id";
            CbCategory.DisplayMemberPath = "Name";
            CbCategory.SelectedValue = 0;

            CbDepartment.ItemsSource = new[] { new { Id = 0, Name = "Все подразделения" } }
                .Concat(_departments.Select(d => new { Id = d.Id, Name = d.Name }))
                .ToList();

            CbDepartment.SelectedValuePath = "Id";
            CbDepartment.DisplayMemberPath = "Name";
            CbDepartment.SelectedValue = 0;
        }

        private IEnumerable<Dictionary<string, string>> PrepareReportData()
        {
            var reportType = ((ComboBoxItem)CbReportType.SelectedItem)?.Content?.ToString() ?? "inventory";
            var categoryFilter = (CbCategory.SelectedValue as int?) ?? 0;
            var departmentFilter = (CbDepartment.SelectedValue as int?) ?? 0;
            var statusFilter = ((ComboBoxItem)CbStatus.SelectedItem)?.Content?.ToString() ?? "all";

            bool isAdmin = false;

            if (_currentUser.Role != null)
                isAdmin = _currentUser.Role.Name.Equals("admin", StringComparison.OrdinalIgnoreCase);
            else
            {
                var role = _roles.FirstOrDefault(r => r.Id == _currentUser.RoleId);
                isAdmin = role != null && role.Name.Equals("admin", StringComparison.OrdinalIgnoreCase);
            }

            var baseItems = isAdmin
                ? _allItems
                : _allItems.Where(i => i.ResponsiblePersonId == _currentUser.Id).ToList();

            var filtered = baseItems.Where(item =>
            {
                bool matchCategory = categoryFilter == 0 || item.Asset?.CategoryId == categoryFilter;
                bool matchDepartment = departmentFilter == 0 || item.Asset?.DepartmentId == departmentFilter;
                bool matchStatus = statusFilter == "all" || item.Status == statusFilter;

                return matchCategory && matchDepartment && matchStatus;
            }).ToList();

            if (reportType == "inventory")
            {
                return filtered.Select(item =>
                {
                    var category = _categories.FirstOrDefault(c => c.Id == item.Asset?.CategoryId)?.Name ?? "-";
                    var department = _departments.FirstOrDefault(d => d.Id == item.Asset?.DepartmentId)?.Name ?? "-";
                    var responsible = _users.FirstOrDefault(u => u.Id == item.ResponsiblePersonId)?.FullName ?? "-";

                    return new Dictionary<string, string>
                    {
                        ["Название"] = item.Asset?.Name ?? "-",
                        ["Категория"] = category,
                        ["Подразделение"] = department,
                        ["Статус"] = item.Status ?? "-",
                        ["Ответственный"] = responsible,
                        ["Описание"] = item.Note ?? "-"
                    };
                }).ToList();
            }
            else
            {
                return filtered.Select(item =>
                {
                    var checks = _checks.Where(c => c.ItemId == item.Id)
                        .OrderByDescending(c => c.CheckDate)
                        .ToList();

                    var last = checks.FirstOrDefault();
                    var checker = last != null
                        ? _users.FirstOrDefault(u => u.Id == last.CheckedById)?.FullName ?? "-"
                        : "-";

                    return new Dictionary<string, string>
                    {
                        ["Название"] = item.Asset?.Name ?? "-",
                        ["Последняя проверка"] = last?.CheckDate.ToString("g") ?? "Не проверялось",
                        ["Результат"] = last?.Status switch
                        {
                            "present" => "Присутствует",
                            "absent" => "Отсутствует",
                            "damaged" => "Повреждено",
                            _ => "-"
                        },
                        ["Проверял"] = checker,
                        ["Примечания"] = last?.Notes ?? "-"
                    };
                }).ToList();
            }
        }

        private void RefreshPreview()
        {
            var data = PrepareReportData().ToList();

            if (data.Count == 0)
            {
                DataGridPreview.ItemsSource = null;
                PreviewInfo = "Нет данных для отчёта";
            }
            else
            {
                var headers = data.SelectMany(d => d.Keys).Distinct().ToList();

                var rows = data.Select(d =>
                {
                    var row = new Dictionary<string, object>();
                    foreach (var h in headers)
                        row[h] = d.ContainsKey(h) ? d[h] : "";
                    return row;
                }).ToList();

                DataGridPreview.ItemsSource = rows;
                PreviewInfo = $"{rows.Count} записей будет включено в отчёт";
            }

            DataContext = this;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => RefreshPreview();

        private void BtnExportJson_Click(object sender, RoutedEventArgs e)
        {
            var data = PrepareReportData();
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var dlg = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                FileName = $"report-{DateTime.Now:yyyyMMddHHmmss}.json"
            };

            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, json);
                MessageBox.Show("JSON отчёт сохранён!", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void CbReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshPreview();
        }

        private void Filters_Changed(object sender, SelectionChangedEventArgs e)
        {
            RefreshPreview();
        }

        private void ExportPdf_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PDF экспорт пока не прикручен 😎");
        }
    }
}
