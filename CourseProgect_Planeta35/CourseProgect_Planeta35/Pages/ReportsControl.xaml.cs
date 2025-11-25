using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using OfficeOpenXml;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace CourseProgect_Planeta35.Controls
{
    public partial class ReportsControl : Window
    {
        public ReportsControl()
        {
            InitializeComponent();
        }

        private void ExportToExcel(List<Dictionary<string, string>> reportData, string fileName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Отчёт");

            if (reportData.Count == 0) return;

            // Заголовки
            int colIndex = 1;
            foreach (var header in reportData[0].Keys)
            {
                sheet.Cells[1, colIndex].Value = header;
                colIndex++;
            }

            // Данные
            for (int row = 0; row < reportData.Count; row++)
            {
                colIndex = 1;
                foreach (var value in reportData[row].Values)
                {
                    sheet.Cells[row + 2, colIndex].Value = value;
                    colIndex++;
                }
            }

            // Сохраняем
            var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            File.WriteAllBytes(savePath, package.GetAsByteArray());
            MessageBox.Show($"Отчёт сохранён: {savePath}");
        }

        private void ExportToPdf(List<Dictionary<string, string>> reportData, string fileName)
        {
            if (reportData.Count == 0) return;

            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = "Отчёт";

            PdfPage page = pdf.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Arial", 12);

            double y = 20;
            double x = 20;

            // Заголовки
            foreach (var header in reportData[0].Keys)
            {
                gfx.DrawString(header, font, XBrushes.Black, x, y);
                x += 100; // ширина колонки
            }

            y += 20;

            // Данные
            foreach (var row in reportData)
            {
                x = 20;
                foreach (var value in row.Values)
                {
                    gfx.DrawString(value, font, XBrushes.Black, x, y);
                    x += 100;
                }
                y += 20;

                // Если страница заполнена, добавляем новую
                if (y > page.Height - 50)
                {
                    page = pdf.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 20;
                }
            }

            var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            pdf.Save(savePath);
            MessageBox.Show($"PDF сохранён: {savePath}");
        }

        // Пример вызова
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var reportData = GetReportData(); // Получаем данные отчёта как List<Dictionary<string,string>>

            ExportToExcel(reportData, "report.xlsx");
            ExportToPdf(reportData, "report.pdf");
        }

        private List<Dictionary<string, string>> GetReportData()
        {
            return new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string>
                {
                    {"Название","Ноутбук"},
                    {"Категория","Электроника"},
                    {"Статус","В эксплуатации"},
                    {"Примечания","Без замечаний"}
                },
                new Dictionary<string, string>
                {
                    {"Название","Проектор"},
                    {"Категория","Оборудование"},
                    {"Статус","На обслуживании"},
                    {"Примечания","Не работает лампа"}
                }
            };
        }
    }
}
