using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.IO;

namespace SimpleRESTApplication.Alumni
{
    public class PdfInscriptor: IDisposable
    {
        public PdfDocument document { get; set; }

        public PdfInscriptor(string filename, PdfDocumentOpenMode openMode = PdfDocumentOpenMode.Modify)
        {
            document = CompatiblePdfReader.Open(filename, openMode);
        }

        public PdfInscriptor(MemoryStream stream, PdfDocumentOpenMode openMode = PdfDocumentOpenMode.Modify)
        {
            document = CompatiblePdfReader.Open(stream, openMode);
        }

        public void MakeInscription(string text, bool needToRotate = true)
        {
            for (int j = 0; j < document.PageCount; j++)
            {
                // Get an XGraphics object for drawing beneath the existing content.
                XGraphics gfx = XGraphics.FromPdfPage(document.Pages[j], XGraphicsPdfPageOptions.Append);

                //All pages can be processed only in portrait orientation
                //thus if landscape then rotate 90 degrees counterclockwise
                if (document.Pages[j].Orientation == PdfSharp.PageOrientation.Landscape)
                {
                    document.Pages[j].Rotate += -90;
                    document.Pages[j].Orientation = PdfSharp.PageOrientation.Portrait;
                }

                XFont font = new XFont("Arial Cyr", 12, XFontStyle.Bold, new XPdfFontOptions(PdfFontEncoding.Unicode));

                // Get the size (in points) of the text.
                XSize textSize = gfx.MeasureString(text, font);
                //And this for one line -- we'll need a height of the line
                XSize lineHeight = gfx.MeasureString("ДолжностьЩЙ", font);

                //
                //if (gfx.PageSize.Width > gfx.PageSize.Height) needToRotate = false;

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
                        rct = new XRect(bleed, document.Pages[0].Height - textSize.Height + i * lineHeight.Height,
                            textSize.Width, textSize.Height - bleed);
                    }
                    gfx.DrawString(strings[i], font, XBrushes.Black, rct, XStringFormats.TopLeft);
                    gfx.Restore();
                }
            }
        }

        public void Dispose()
        {
            document.Dispose();
        }
    }
}