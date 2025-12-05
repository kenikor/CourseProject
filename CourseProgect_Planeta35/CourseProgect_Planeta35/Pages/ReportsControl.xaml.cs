using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace CourseProgect_Planeta35.Controls
{
    public partial class ReportsControl : UserControl
    {
        private readonly User CurrentUser;
        private readonly AppDbContext _db;
        private List<AssetViewModel> _allAssets;

        public string PreviewInfo
        {
            get { return (string)GetValue(PreviewInfoProperty); }
            set { SetValue(PreviewInfoProperty, value); }
        }

        public static readonly DependencyProperty PreviewInfoProperty =
            DependencyProperty.Register("PreviewInfo", typeof(string), typeof(ReportsControl), new PropertyMetadata(""));

        public ReportsControl(User currentUser, AppDbContext db)
        {
            InitializeComponent();
            CurrentUser = currentUser;
            _db = db;
            LoadData();
        }

        private void LoadData()
        {
            CbCategory.ItemsSource = _db.AssetCategories.ToList();
            CbDepartment.ItemsSource = _db.Departments.ToList();

            _allAssets = _db.Assets
                .Include(a => a.Category)
                .Include(a => a.Department)
                .Include(a => a.Responsible)
                .Select(a => new AssetViewModel { Asset = a })
                .ToList();

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var filtered = _allAssets.AsEnumerable();

            if (CbCategory.SelectedItem is AssetCategory cat)
                filtered = filtered.Where(a => a.Asset.Category.Id == cat.Id);

            if (CbDepartment.SelectedItem is Department dep)
                filtered = filtered.Where(a => a.Asset.Department.Id == dep.Id);

            if (CbStatus.SelectedItem is ComboBoxItem statusItem && statusItem.Content.ToString() != "Все")
                filtered = filtered.Where(a => a.Asset.Status == statusItem.Content.ToString());

            DataGridPreview.ItemsSource = filtered.ToList();
            PreviewInfo = $"Найдено объектов: {filtered.Count()}";
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            CbCategory.SelectedItem = null;
            CbDepartment.SelectedItem = null;
            CbStatus.SelectedIndex = 0; // Сбрасываем на "Все"
            CbReportType.SelectedIndex = 0;
            UpdatePreview();
        }

        private void CbReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = "Отчёт.pdf"
            };

            if (dlg.ShowDialog() != true)
                return;

            try
            {
                var assetsForPdf = _db.Assets
                    .Include(a => a.Category)
                    .Include(a => a.Department)
                    .Include(a => a.Responsible)
                    .ToList();

                using (var fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var doc = new Document(PageSize.A4, 25, 25, 25, 25);
                    PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    var font = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

                    PdfPTable table = new PdfPTable(7); // 7 колонок
                    table.WidthPercentage = 100;

                    string[] headers = { "Название", "Описание", "Категория", "Подразделение", "Ответственный", "Серийный номер", "Статус" };
                    foreach (var h in headers)
                        table.AddCell(new PdfPCell(new Phrase(h, font)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                    foreach (var item in assetsForPdf)
                    {
                        table.AddCell(new PdfPCell(new Phrase(item.Name ?? "", font)));
                        table.AddCell(new PdfPCell(new Phrase(item.Description ?? "", font)));
                        table.AddCell(new PdfPCell(new Phrase(item.Category?.Name ?? "", font)));
                        table.AddCell(new PdfPCell(new Phrase(item.Department?.Name ?? "", font)));
                        table.AddCell(new PdfPCell(new Phrase(item.Responsible?.FullName ?? "", font)));
                        table.AddCell(new PdfPCell(new Phrase(item.InventoryNumber ?? "", font)));
                        table.AddCell(new PdfPCell(new Phrase(item.Status ?? "", font)));
                    }

                    doc.Add(table);
                    doc.Close();
                }

                MessageBox.Show("PDF сохранён!");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте PDF: " + ex.Message);
            }
        }

        private void BtnExportXlsx_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "Report.xlsx"
            };

            if (dlg.ShowDialog() != true)
                return;

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Report");

                    string[] headers = { "Название", "Описание", "Категория", "Подразделение", "Ответственный", "Серийный номер", "Статус" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    int row = 2;
                    foreach (var item in _allAssets)
                    {
                        worksheet.Cells[row, 1].Value = item.Asset.Name;
                        worksheet.Cells[row, 2].Value = item.Asset.Description;
                        worksheet.Cells[row, 3].Value = item.Asset.Category?.Name ?? "";
                        worksheet.Cells[row, 4].Value = item.Asset.Department?.Name ?? "";
                        worksheet.Cells[row, 5].Value = item.Asset.Responsible?.FullName ?? "";
                        worksheet.Cells[row, 6].Value = item.Asset.InventoryNumber ?? "";
                        worksheet.Cells[row, 7].Value = item.Asset.Status ?? "";
                        row++;
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    using (var stream = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write))
                        package.SaveAs(stream);
                }

                MessageBox.Show("Excel файл сохранён!");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте Excel: " + ex.Message);
            }
        }

        // ViewModel для DataGrid
        public class AssetViewModel
        {
            public Asset Asset { get; set; }
        }
    }
}