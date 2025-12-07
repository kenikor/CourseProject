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
                FileName = "Отчет.pdf"
            };

            if (dlg.ShowDialog() != true) return;

            try
            {
                var assets = _allAssets.Select(a => a.Asset).ToList();

                using var fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write, FileShare.None);
                var doc = new Document(PageSize.A4.Rotate(), 25, 25, 25, 25);
                PdfWriter.GetInstance(doc, fs);
                doc.Open();

                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                var font = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

                PdfPTable table = new PdfPTable(7) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 3f, 2f, 2f });

                string[] headers = { "Название", "Описание", "Категория", "Подразделение", "Ответственный", "Серийный номер", "Статус" };
                foreach (var h in headers)
                    table.AddCell(new PdfPCell(new Phrase(h, font)) { BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_CENTER });

                foreach (var asset in assets)
                {
                    table.AddCell(Cell(asset.Name, font));
                    table.AddCell(Cell(asset.Description, font));
                    table.AddCell(Cell(asset.Category?.Name, font));
                    table.AddCell(Cell(asset.Department?.Name, font));
                    table.AddCell(Cell(asset.Responsible?.FullName, font));
                    table.AddCell(Cell(asset.InventoryNumber, font));
                    table.AddCell(Cell(asset.Status, font));
                }

                doc.Add(table);
                doc.Close();

                MessageBox.Show("PDF сохранён!");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте PDF: " + ex.Message);
            }
        }

        private PdfPCell Cell(string text, iTextSharp.text.Font font) =>
            new PdfPCell(new Phrase(text ?? "", font))
            {
                NoWrap = false,
                MinimumHeight = 20,
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };

        private void BtnExportXlsx_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "Report.xlsx"
            };

            if (dlg.ShowDialog() != true) return;

            try
            {
                using var package = new ExcelPackage();
                var sheet = package.Workbook.Worksheets.Add("Report");

                string[] headers = { "Название", "Описание", "Категория", "Подразделение", "Ответственный", "Серийный номер", "Статус" };
                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.Cells[1, i + 1].Value = headers[i];
                    sheet.Cells[1, i + 1].Style.Font.Bold = true;
                    sheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                int row = 2;
                foreach (var asset in _allAssets.Select(a => a.Asset))
                {
                    sheet.Cells[row, 1].Value = asset.Name;
                    sheet.Cells[row, 2].Value = asset.Description;
                    sheet.Cells[row, 3].Value = asset.Category?.Name ?? "";
                    sheet.Cells[row, 4].Value = asset.Department?.Name ?? "";
                    sheet.Cells[row, 5].Value = asset.Responsible?.FullName ?? "";
                    sheet.Cells[row, 6].Value = asset.InventoryNumber ?? "";
                    sheet.Cells[row, 7].Value = asset.Status ?? "";
                    row++;
                }

                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
                package.SaveAs(new FileInfo(dlg.FileName));

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