using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Services
{
    public class PdfFooter : PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            BaseFont bf = BaseFont.CreateFont(
                BaseFont.HELVETICA,
                BaseFont.CP1252,
                false);

            PdfContentByte cb = writer.DirectContent;

            cb.BeginText();

            cb.SetFontAndSize(bf, 9);

            cb.ShowTextAligned(
                Element.ALIGN_CENTER,
                $"Страница {writer.PageNumber}",
                document.PageSize.Width / 2,
                20,
                0);

            cb.EndText();
        }
    }
}
