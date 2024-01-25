using iTextSharp.text.pdf.parser;
using Dotnet = System.Drawing.Image;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using System;
using System.Text;
using ShopLabelGenerator.Models;
using System.Drawing;

namespace ShopLabelGenerator.Service
{
    public static class PDF
    {
        public static List<QRCode> ExtractImagesFromPDF(string path)
        {
            var pdf = new PdfReader(path);
            // Блок для извлечения описания
            List<QRCode> QRCodesStringCollection = new List<QRCode>();
            int counter = 0;

            try
            {
                for (int pageNumber = 1; pageNumber <= pdf.NumberOfPages; pageNumber++)
                {
                    PdfDictionary pg = pdf.GetPageN(pageNumber);
                    var imgs = GetImagesFromPdfDict(pg, pdf);

                    // Блок для извлечения описания
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string text = PdfTextExtractor.GetTextFromPage(pdf, pageNumber, strategy);
                    text = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));
                    string[] descArr = text.Split('\n');


                    for (int i = 0; i < imgs.Count; i++)
                    {
                        if (imgs[i] != null)
                        {
                            QRCode qRCodeDesc = new QRCode
                            {
                                Id = ++counter,
                                Model = System.Text.RegularExpressions.Regex.Replace(descArr[i * 5].Trim(), @"\s+", " "),
                                Art = descArr[i * 5 + 1].Substring(8),
                                Size = descArr[i * 5 + 2].Substring(8),
                                // Color = descArr[4 * i + 3],
                                QRTopPart = descArr[i * 5 + 3],
                                QRBottomPart = descArr[i * 5 + 4],
                                QRCodeImage = imgs[i]
                            };
                            QRCodesStringCollection.Add(qRCodeDesc);
                        }
                    }
                }
            }
            finally
            {
                pdf.Close();
            }

            return QRCodesStringCollection;
        }

        private static IList<Dotnet> GetImagesFromPdfDict(PdfDictionary dict, PdfReader doc)
        {
            List<Dotnet> images = new List<Dotnet>();
            PdfDictionary res = (PdfDictionary)PdfReader.GetPdfObject(dict.Get(PdfName.RESOURCES));
            PdfDictionary xobj = (PdfDictionary)PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT));

            if (xobj != null)
            {
                foreach (PdfName name in xobj.Keys)
                {
                    PdfObject obj = xobj.Get(name);
                    if (obj.IsIndirect())
                    {
                        PdfDictionary tg = (PdfDictionary)(PdfReader.GetPdfObject(obj));
                        PdfName subtype = (PdfName)(PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE)));
                        if (PdfName.IMAGE.Equals(subtype))
                        {
                            int xrefIdx = ((PRIndirectReference)obj).Number;
                            PdfObject pdfObj = doc.GetPdfObject(xrefIdx);
                            PdfStream str = (PdfStream)(pdfObj);

                            PdfImageObject pdfImage =
                                new PdfImageObject((PRStream)str);
                            Dotnet img = pdfImage.GetDrawingImage();

                            images.Add(img);
                        }
                        else if (PdfName.FORM.Equals(subtype) || PdfName.GROUP.Equals(subtype))
                        {
                            images.AddRange(GetImagesFromPdfDict(tg, doc));
                        }
                    }
                }
            }

            return images;
        }
    }
}