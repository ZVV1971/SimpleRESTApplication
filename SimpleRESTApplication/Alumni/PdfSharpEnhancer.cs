using System;
using System.IO;

namespace PdfSharp.Pdf.IO
{
    //taken from https://stackoverflow.com/questions/12782295/does-pdf-file-contain-iref-stream
    static public class CompatiblePdfReader
    {
        /// <summary>
        /// uses itextsharp to convert any pdf to 1.4 compatible pdf, called instead of PdfReader.open
        /// </summary>
        static public PdfDocument Open(string pdfPath, PdfDocumentOpenMode openMode = PdfDocumentOpenMode.Modify)
        {
            using (var fileStream = new FileStream(pdfPath, FileMode.Open, FileAccess.Read))
            {
                var len = (int)fileStream.Length;
                var fileArray = new Byte[len];
                fileStream.Read(fileArray, 0, len);
                fileStream.Close();

                return Open(fileArray, openMode);
            }
        }

        /// <summary>
        /// uses itextsharp to convert any pdf to 1.4 compatible pdf, called instead of PdfReader.open
        /// </summary>
        static public PdfDocument Open(byte[] fileArray, PdfDocumentOpenMode openMode = PdfDocumentOpenMode.Modify)
        {
            return Open(new MemoryStream(fileArray), openMode);
        }

        /// <summary>
        /// uses itextsharp to convert any pdf to 1.4 compatible pdf, called instead of PdfReader.open
        /// </summary>
        static public PdfDocument Open(MemoryStream sourceStream, PdfDocumentOpenMode openMode = PdfDocumentOpenMode.Modify)
        {
            PdfDocument outDoc;
            sourceStream.Position = 0;

            try
            {
                outDoc = PdfReader.Open(sourceStream, openMode);
            }
            catch (PdfReaderException)
            {
                //workaround if pdfsharp doesn't support this pdf
                sourceStream.Position = 0;
                var outputStream = new MemoryStream();
                var reader = new iTextSharp.text.pdf.PdfReader(sourceStream);
                var pdfStamper = new iTextSharp.text.pdf.PdfStamper(reader, outputStream) { FormFlattening = true };
                pdfStamper.Writer.SetPdfVersion(iTextSharp.text.pdf.PdfWriter.PDF_VERSION_1_4);
                pdfStamper.Writer.CloseStream = false;
                pdfStamper.Close();

                outDoc = PdfReader.Open(outputStream, openMode);
            }

            return outDoc;
        }
    }
}
