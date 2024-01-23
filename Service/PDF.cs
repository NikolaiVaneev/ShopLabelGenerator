using iTextSharp.text.pdf.parser;
using Dotnet = System.Drawing.Image;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using System;
using System.Text;
using ShopLabelGenerator.Models;

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
                    List<PdfObject> objs = FindImageInPDFDictionary(pg);

                    // Блок для извлечения описания
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string text = PdfTextExtractor.GetTextFromPage(pdf, pageNumber, strategy);
                    text = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));
                    string[] descArr = text.Split('\n');

                    for (int i = 0; i < objs.Count; i++)
                    //foreach (var obj in objs)
                    {
                        if (objs[i] != null)
                        {
                            QRCode qRCodeDesc = new QRCode
                            {
                                Id = ++counter,
                                Model = System.Text.RegularExpressions.Regex.Replace(descArr[i * 5].Trim(), @"\s+", " "),
                                Art = descArr[i * 5 + 1].Substring(8),
                                Size = descArr[i * 5 + 2].Substring(8),
                               // Color = descArr[4 * i + 3],
                                QRTopPart = descArr[i * 5 + 3],
                                QRBottomPart = descArr[i * 5 + 4]
                            };


                            int XrefIndex = Convert.ToInt32(((PRIndirectReference)objs[i]).Number.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            PdfObject pdfObj = pdf.GetPdfObject(XrefIndex);
                            PdfStream pdfStrem = (PdfStream)pdfObj;
                            var pdfImage = new PdfImageObject((PRStream)pdfStrem);
                            var img = pdfImage.GetDrawingImage();

                            //imgs.Add(img);
                            qRCodeDesc.QRCodeImage = img;
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
        private static List<PdfObject> FindImageInPDFDictionary(PdfDictionary pg)
        {
            var res = (PdfDictionary)PdfReader.GetPdfObject(pg.Get(PdfName.RESOURCES));
            var xobj = (PdfDictionary)PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT));
            var pdfObgs = new List<PdfObject>();

            if (xobj != null)
            {
                foreach (PdfName name in xobj.Keys)
                {
                    PdfObject obj = xobj.Get(name);
                    if (obj.IsIndirect())
                    {
                        var tg = (PdfDictionary)PdfReader.GetPdfObject(obj);
                        var type = (PdfName)PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE));

                        if (PdfName.IMAGE.Equals(type)) // image at the root of the pdf
                        {
                            pdfObgs.Add(obj);
                        }
                        else if (PdfName.FORM.Equals(type)) // image inside a form
                        {
                            FindImageInPDFDictionary(tg).ForEach(o => pdfObgs.Add(o));
                        }
                        else if (PdfName.GROUP.Equals(type)) // image inside a group
                        {
                            FindImageInPDFDictionary(tg).ForEach(o => pdfObgs.Add(o));
                        }
                    }
                }
            }

            return pdfObgs;
        }
  
        public static List<QRCode> ExtractQRCodeDescFromPDF(string path)
        {
            List<QRCode> QRCodesStringCollection = new List<QRCode>();

            using(PdfReader reader = new PdfReader(path))
            {
                int counter = 0;
                for (int pageNo = 1; pageNo < reader.NumberOfPages + 1; pageNo++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string text = PdfTextExtractor.GetTextFromPage(reader, pageNo, strategy);
                    text = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(text)));
                    string[] descArr = text.Split('\n');
                    // Получаем количество кодов на странице
                    int QRCodesFromPage = descArr.Length / 6;
                    
                    for (int i = 0; i < QRCodesFromPage; i++)
                    {
                        

                        QRCode qRCodeDesc = new QRCode
                        {
                            Id = ++counter,
                            Model = System.Text.RegularExpressions.Regex.Replace(descArr[i * 6].Trim(), @"\s+", " "),
                            Art = descArr[6 * i + 1],
                            Size = descArr[6 * i + 2].Substring(8),
                            Color = descArr[6 * i + 3],
                            QRTopPart = descArr[6 * i + 4],
                            QRBottomPart = descArr[6 * i + 5]
                        };
                        QRCodesStringCollection.Add(qRCodeDesc);
                    }
                }
            }

            return QRCodesStringCollection;
        }
    }
}