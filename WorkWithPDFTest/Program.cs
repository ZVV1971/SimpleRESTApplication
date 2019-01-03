using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;

namespace WorkWithPDFTest
{
    class Program
    {
        static string text = "Копия верна" + Environment.NewLine + "Должность на фирме" 
            + Environment.NewLine + Environment.NewLine + "Фамилия И.О.";
        static string filename = @"e:\1.test.pdf";
        static bool needToRotate = true;

        static void Main(string[] args)
        {
            PdfDocument document = PdfReader.Open(filename, PdfDocumentOpenMode.Modify);

            // Get an XGraphics object for drawing beneath the existing content.
            XGraphics gfx = XGraphics.FromPdfPage(document.Pages[0], XGraphicsPdfPageOptions.Prepend);

            XFont font = new XFont("Arial Cyr", 12, XFontStyle.Bold, new XPdfFontOptions(PdfFontEncoding.Unicode));

            // Get the size (in points) of the text.
            XSize textSize = gfx.MeasureString(text, font);
            //And this for one line -- we'll need a height of the line
            XSize lineHeight = gfx.MeasureString("ДолжностьЩЙ", font);

            int bleed = 5;
            XRect rct;

            //Split given string so as to draw each line separately
            string[] strings = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < strings.GetLength(0); i++)
            {
                gfx.Save();
                if (needToRotate)
                {
                    rct = new XRect(document.Pages[0].Width - textSize.Width + i * lineHeight.Height,
                        document.Pages[0].Height - textSize.Height, textSize.Width, textSize.Height);
                    //rotate around the top left corner of the rect
                    gfx.RotateAtTransform(-90, new XPoint(rct.Left, rct.Top));
                    //shift rotated image so as to bring it to the right bottom of the page
                    gfx.TranslateTransform(-textSize.Height + bleed, rct.Width - textSize.Height - bleed);
                }
                else
                {
                    rct = new XRect(bleed, document.Pages[0].Height - textSize.Height + i* lineHeight.Height,
                        textSize.Width, textSize.Height - bleed);
                }
                gfx.DrawString(strings[i], font, XBrushes.Black, rct, XStringFormats.TopLeft);
                gfx.Restore();
            }
            document.Save(filename);
        }
    }
}