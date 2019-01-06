using System;
using SimpleRESTApplication.Alumni;

namespace WorkWithPDFTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!(args.GetLength(0) < 1))
            {
                PdfInscriptor pdf = new PdfInscriptor(args[0]);
                pdf.MakeInscription("Копия верна" + Environment.NewLine + "Должность на фирме"
            + Environment.NewLine + Environment.NewLine + "Фамилия И.О.");
                pdf.document.Save(args[0]);
            }
        }
    }
}