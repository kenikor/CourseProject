using CourseProgect_Planeta35.Data;
using CourseProgect_Planeta35.Models;
using CourseProgect_Planeta35.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
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
            if (string.IsNullOrWhiteSpace(value))
                value = "-";

            var p = new Paragraph
            {
                SpacingAfter = 4
            };

            p.Add(new Chunk(
                label + ": ",
                new iTextSharp.text.Font(
                    font.BaseFont,
                    11,
                    iTextSharp.text.Font.BOLD,
                    BaseColor.BLACK)));

            p.Add(new Chunk(value, font));

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
                var assets = DataGridPreview.ItemsSource.Cast<AssetViewModel>().ToList();

                if (assets == null || !assets.Any())
                {
                    MessageBox.Show("Нет данных для экспорта");
                    return;
                }

                using var fs = new FileStream(dlg.FileName, FileMode.Create);

                var document = new Document(PageSize.A4, 25, 25, 30, 30);

                var writer = PdfWriter.GetInstance(document, fs);

                writer.PageEvent = new PdfFooter();

                writer.ViewerPreferences = PdfWriter.PageLayoutSinglePage;

                document.Open();

                string fontPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                    "arial.ttf");

                var baseFont = BaseFont.CreateFont(
                    fontPath,
                    BaseFont.IDENTITY_H,
                    BaseFont.EMBEDDED);

                var titleFont = new iTextSharp.text.Font(
                    baseFont,
                    18,
                    iTextSharp.text.Font.BOLD,
                    new BaseColor(25, 25, 25));

                var sectionFont = new iTextSharp.text.Font(
                    baseFont,
                    13,
                    iTextSharp.text.Font.BOLD,
                    new BaseColor(40, 40, 40));

                var textFont = new iTextSharp.text.Font(
                    baseFont,
                    10,
                    iTextSharp.text.Font.NORMAL,
                    new BaseColor(70, 70, 70));

                var smallFont = new iTextSharp.text.Font(
                    baseFont,
                    8,
                    iTextSharp.text.Font.ITALIC,
                    BaseColor.GRAY);

                Paragraph title = new Paragraph
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 4
                };

                title.Add(new Chunk("PLANETA 35\n", titleFont));

                title.Add(new Chunk(
                    "Отчет по имуществу",
                    new iTextSharp.text.Font(
                        baseFont,
                        11,
                        iTextSharp.text.Font.NORMAL,
                        BaseColor.GRAY)));

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

                    PdfPTable card = new PdfPTable(2)
                    {
                        WidthPercentage = 100,
                        SpacingAfter = 10
                    };

                    card.SetWidths(new float[] { 85, 15 });

                    PdfPCell container = new PdfPCell
                    {
                        BorderWidth = 0.7f,
                        BorderColor = new BaseColor(210, 210, 210),
                        Padding = 10,
                        BackgroundColor = BaseColor.WHITE
                    };

                    Paragraph assetTitle = new Paragraph(
                        asset.Name ?? "Без названия",
                        sectionFont
                    );

                    assetTitle.SpacingAfter = 8;

                    container.AddElement(assetTitle);

                    BaseColor statusColor = BaseColor.WHITE;

                    iTextSharp.text.Font statusFont = new iTextSharp.text.Font(smallFont);

                    switch (asset.Status)
                    {
                        case "В наличии":
                            statusColor = new BaseColor(46, 204, 113);
                            statusFont.Color = BaseColor.WHITE;
                            break;

                        case "На обслуживании":
                            statusColor = new BaseColor(241, 196, 15);
                            statusFont.Color = BaseColor.WHITE;
                            break;

                        case "Списано":
                            statusColor = new BaseColor(231, 76, 60);
                            statusFont.Color = BaseColor.WHITE;
                            break;

                        case "Отсутствует":
                            statusColor = new BaseColor(230, 126, 34);
                            statusFont.Color = BaseColor.WHITE;
                            break;

                        case "Все":
                        default:
                            statusColor = BaseColor.GRAY;
                            statusFont.Color = BaseColor.WHITE;
                            break;
                    }

                    PdfPTable statusTable = new PdfPTable(1);

                    BaseFont statusBaseFont = smallFont.BaseFont;
                    float fontSize = smallFont.Size;

                    iTextSharp.text.Font whiteFont = new iTextSharp.text.Font(baseFont, fontSize, iTextSharp.text.Font.BOLD, BaseColor.WHITE);

                    PdfPCell statusCell = new PdfPCell(
                        new Phrase(asset.Status ?? "Неизвестно", whiteFont))
                    {
                        BackgroundColor = statusColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        PaddingTop = 4,
                        PaddingBottom = 4,
                        Border = iTextSharp.text.Rectangle.NO_BORDER
                    };

                    statusTable.WidthPercentage = 100;
                    statusTable.SpacingBefore = 5;
                    statusTable.SpacingAfter = 5;

                    statusTable.AddCell(statusCell);

                    statusTable.WidthPercentage = 18;

                    container.AddElement(statusTable);

                    AddInfo(container, "Описание", asset.Description, textFont);
                    AddInfo(container, "Категория", asset.Category?.Name, textFont);
                    AddInfo(container, "Подразделение", asset.Department?.Name, textFont);
                    AddInfo(container, "Ответственный", asset.Responsible?.FullName, textFont);
                    AddInfo(container, "Инвентарный номер", asset.InventoryNumber, textFont);

                    card.AddCell(container);

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

                        qrImage.ScaleAbsolute(55, 55);

                        qrImage.Alignment = Element.ALIGN_RIGHT;

                        PdfPCell qrCell = new PdfPCell
                        {
                            Border = iTextSharp.text.Rectangle.NO_BORDER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_CENTER
                        };

                        qrCell.AddElement(qrImage);

                        card.AddCell(qrCell);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("QR error: " + ex.Message);
                    }
                    
                    document.Add(card);

                    LineSeparator line = new LineSeparator
                    {
                        LineColor = new BaseColor(230, 230, 230),
                        Percentage = 100
                    };

                    document.Add(new Chunk(line));
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