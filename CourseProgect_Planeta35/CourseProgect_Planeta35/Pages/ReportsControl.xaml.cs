using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using CourseProgect_Planeta35.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
            UpdatePreview();
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
            CbStatus.SelectedIndex = 0;
            CbReportType.SelectedIndex = 0;
            UpdatePreview();
        }

        private void CbReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void AddInfo(
        PdfPCell container,
        string label,
        string value,
        iTextSharp.text.Font font)
        {
            Paragraph p = new Paragraph
            {
                SpacingAfter = 6
            };

            p.Add(new Chunk(label + ": ", new iTextSharp.text.Font(font.BaseFont, 12, iTextSharp.text.Font.BOLD)));
            p.Add(new Chunk(value ?? "-", font));

            container.AddElement(p);
        }

        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = "Inventory_Report.pdf"
            };

            if (dlg.ShowDialog() != true)
                return;

            try
            {
                var assets = DataGridPreview.ItemsSource as List<AssetViewModel>;

                using var fs = new FileStream(dlg.FileName, FileMode.Create);

                var document = new Document(PageSize.A4, 40, 40, 50, 50);

                var writer = PdfWriter.GetInstance(document, fs);

                writer.PageEvent = new PdfFooter();

                document.Open();

                string fontPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                    "arial.ttf");

                var baseFont = BaseFont.CreateFont(
                    fontPath,
                    BaseFont.IDENTITY_H,
                    BaseFont.EMBEDDED);

                var titleFont = new iTextSharp.text.Font(baseFont, 26, iTextSharp.text.Font.BOLD, new BaseColor(33, 37, 41));
                var sectionFont = new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                var textFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.NORMAL, new BaseColor(60, 60, 60));
                var smallFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.ITALIC, BaseColor.GRAY);

                Paragraph title = new Paragraph("PLANETA 35\nОТЧЕТ ПО ИМУЩЕСТВУ", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };

                document.Add(title);

                Paragraph date = new Paragraph(
                    $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}",
                    smallFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 30
                };

                document.Add(date);

                foreach (var vm in assets)
                {
                    var asset = vm.Asset;

                    PdfPTable card = new PdfPTable(1)
                    {
                        WidthPercentage = 100,
                        SpacingAfter = 25
                    };

                    PdfPCell container = new PdfPCell
                    {
                        BorderWidth = 1,
                        BorderColor = new BaseColor(220, 220, 220),
                        Padding = 20,
                        BackgroundColor = BaseColor.WHITE
                    };

                    Paragraph assetTitle = new Paragraph(
                        asset.Name ?? "Без названия",
                        sectionFont);

                    assetTitle.SpacingAfter = 10;

                    container.AddElement(assetTitle);

                    BaseColor statusColor = BaseColor.GRAY;

                    switch (asset.Status)
                    {
                        case "Активен":
                            statusColor = new BaseColor(46, 204, 113);
                            break;

                        case "На ремонте":
                            statusColor = new BaseColor(241, 196, 15);
                            break;

                        case "Списан":
                            statusColor = new BaseColor(231, 76, 60);
                            break;
                    }

                    PdfPTable statusTable = new PdfPTable(1);

                    PdfPCell statusCell = new PdfPCell(new Phrase(asset.Status ?? "Неизвестно", textFont))
                    {
                        BackgroundColor = statusColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 8,
                        Border = iTextSharp.text.Rectangle.NO_BORDER
                    };

                    statusTable.AddCell(statusCell);

                    statusTable.WidthPercentage = 30;

                    container.AddElement(statusTable);

                    container.AddElement(new Paragraph("\n"));

                    AddInfo(container, "Описание", asset.Description, textFont);
                    AddInfo(container, "Категория", asset.Category?.Name, textFont);
                    AddInfo(container, "Подразделение", asset.Department?.Name, textFont);
                    AddInfo(container, "Ответственный", asset.Responsible?.FullName, textFont);
                    AddInfo(container, "Инвентарный номер", asset.InventoryNumber, textFont);

                    try
                    {
                        string qrText = $"https://localhost:5001/asset/{asset.Id}";

                        using var qrGenerator = new QRCoder.QRCodeGenerator();

                        var qrData = qrGenerator.CreateQrCode(
                            qrText,
                            QRCoder.QRCodeGenerator.ECCLevel.Q);

                        var qrCode = new QRCoder.PngByteQRCode(qrData);

                        byte[] qrBytes = qrCode.GetGraphic(20);

                        var qrImage = iTextSharp.text.Image.GetInstance(qrBytes);

                        qrImage.ScaleAbsolute(90, 90);

                        qrImage.Alignment = Element.ALIGN_RIGHT;

                        container.AddElement(new Paragraph("\nQR-код объекта:", smallFont));
                        container.AddElement(qrImage);
                    }
                    catch
                    {

                    }

                    card.AddCell(container);

                    document.Add(card);
                }

                Paragraph footer = new Paragraph(
                    $"Документ сформирован пользователем: {CurrentUser.FullName}",
                    smallFont)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 20
                };

                document.Add(footer);

                document.Close();

                MessageBox.Show("PDF успешно сохранён");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка PDF: " + ex.Message);
            }
        }

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
        public class AssetViewModel
        {
            public Asset Asset { get; set; }
        }
    }
}