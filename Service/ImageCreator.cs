using BarcodeLib;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using ShopLabelGenerator.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace ShopLabelGenerator.Service
{
    public static class ImageCreator
    {

        static float Percent { get; set; } = 1.0f;
        private static Image CodeGenerate(string message)
        {
            Barcode barcode = new Barcode
            {
                RotateFlipType = RotateFlipType.Rotate270FlipNone,
                IncludeLabel = true,
                Height = 150
            };
            Image image = barcode.Encode(TYPE.EAN13, message);

            return image;
        }
        public static Image GetSticker(Product product, int w, int h, string note, float FontSize, bool border = false)
        {
            Bitmap bmp = new Bitmap(w, h);  //размер стикера

            float FontFizeKoef = FontSize / 100;

            using (Graphics pic = Graphics.FromImage(bmp))
            {
                Rectangle rect;
                // Обводка прямоугольником.
                if (border)
                {
                    Pen pen = new Pen(Color.Black, 2);
                    pic.DrawRectangle(pen, 2, 2, w - 2, h - 2);
                }

                pic.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                // QR код
                pic.DrawImage(product.QRCode.QRCodeImage, 20, 40, GetRangeInPercent(25f, w), GetRangeInPercent(25f, w));

                // Описание кода
                StringFormat stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                Font font = new Font("Arial Regular", 15.0f * FontFizeKoef, FontStyle.Regular);
                rect = new Rectangle(20, (int)GetRangeInPercent(80f, h), (int)GetRangeInPercent(25f, w), (int)GetRangeInPercent(12f, h));
                // тест размещения прямоугльника
                //Pen p = new Pen(Color.Black, 2);
                //pic.DrawRectangle(p, rect);
                StringBuilder qrNote = new StringBuilder();
                qrNote.AppendLine(product.QRCode.QRTopPart);
                qrNote.AppendLine(product.QRCode.QRBottomPart);
                pic.DrawString(qrNote.ToString(), font, new SolidBrush(Color.Black), rect, stringFormat);

                // EAK логотип.
                pic.DrawImage(Properties.Resources.EAC, 350, 275, 100, 100);

                // Штрих-код
                if (!string.IsNullOrWhiteSpace(product.BARCode))
                {
                    Image barCode = CodeGenerate(product.BARCode);
                    //barCode.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    pic.DrawImage(barCode, GetRangeInPercent(87f, w), GetRangeInPercent(3f, h), GetRangeInPercent(13f, w), GetRangeInPercent(94f, h));
                }

                // Описание.
                // Настройка выравнивания текста.
                stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Near
                };
                rect = new Rectangle(-410, 890, 380, 150);
                pic.RotateTransform(-90);
                // тест размещения прямоугльника
                //Pen p = new Pen(Color.Black, 2);
                //pic.DrawRectangle(p, rect);
                font = new Font("Arial Regular", 9.0f * FontFizeKoef, FontStyle.Regular);

                pic.DrawString(note, font, new SolidBrush(Color.Black), rect, stringFormat);
                pic.RotateTransform(90);

                // Артикул
                rect = new Rectangle(350, (int)GetRangeInPercent(9f, h), (int)GetRangeInPercent(25f, w), (int)GetRangeInPercent(10f, h));
                // тест размещения прямоугльника
                //Pen p = new Pen(Color.Black, 2);
                //pic.DrawRectangle(p, rect);
                font = new Font("Arial Regular", 28.0f * FontFizeKoef, FontStyle.Regular);
                pic.DrawString("Артикул:", font, new SolidBrush(Color.Black), rect, stringFormat);
                rect = new Rectangle(580, (int)GetRangeInPercent(9f, h), (int)GetRangeInPercent(25f, w), (int)GetRangeInPercent(10f, h));
                // тест размещения прямоугльника
                //Pen p = new Pen(Color.Black, 2);
                //pic.DrawRectangle(p, rect);
                pic.DrawString(product.VendorCode, font, new SolidBrush(Color.Black), rect, stringFormat);

                // Данные модели
                // Тип
                rect = new Rectangle(350, (int)GetRangeInPercent(25f, h), (int)GetRangeInPercent(18f, w), (int)GetRangeInPercent(20f, h));
                // тест размещения прямоугльника
                //Pen p = new Pen(Color.Black, 2);
                //pic.DrawRectangle(p, rect);
                pic.DrawString(product.Type, font, new SolidBrush(Color.Black), rect, stringFormat);
                // Размер
                rect = new Rectangle(580, (int)GetRangeInPercent(35f, h), (int)GetRangeInPercent(18f, w), (int)GetRangeInPercent(10f, h));
                // тест размещения прямоугльника
                //p = new Pen(Color.Black, 2);
                //pic.DrawRectangle(p, rect);
                pic.DrawString($"Размер: {product.Size}", font, new SolidBrush(Color.Black), rect, stringFormat);

                // Описание
                rect = new Rectangle(580, 275, (int)GetRangeInPercent(25f, w), (int)GetRangeInPercent(40f, h));
                // тест размещения прямоугльника
                //Pen p = new Pen(Color.Black, 2);
                //pic.DrawRectangle(p, rect);

                StringBuilder description = new StringBuilder();
                description.AppendLine($"Цвет: {product.Color}");
                description.AppendLine($"Верх: {product.TopMaterial}");
                description.AppendLine($"Подкладка: {product.Insole}");
                description.AppendLine($"Подошва: {product.SoleMaterial}");
                description.AppendLine("");
                description.AppendLine($"Дата производства: {product.ProductionDate}");

                font = new Font("Arial Regular", 15.0f * FontFizeKoef, FontStyle.Regular);

                pic.DrawString(description.ToString(), font, new SolidBrush(Color.Black), rect, stringFormat);
            }
            return bmp;
        }
        public static PdfDocument CreateStickers(List<Product> CodeList, string note, int widthMm, int heightMm, ICollection<QRCode> qRCodes, float FontSize)
        {

            PdfDocument doc = new PdfDocument();

            //double convert = 11.81049;
            double convert = 11.81902;
            Bitmap bmp = new Bitmap(2480, 3508);
            // Размер одного стикера в пикселях.
            int w = (int)(widthMm * convert);
            int h = (int)(heightMm * convert);
            // Получаем количество стикеров на лист А4 по вертикали, горизонтали и всего.
            int maxColumns = bmp.Width / w;
            int maxRows = bmp.Height / h;
            int maxSticksOnA4 = maxRows * maxColumns;
            // Получаем начальную точку отрисовки готовых стикеров.
            Point startPoint = new Point((bmp.Width - maxColumns * w) / 2, (bmp.Height - maxRows * h) / 2);
            // Стартовые координаты для каждого стикера
            float horizPixel = startPoint.X; //стартовый Х
            float vertPixel = startPoint.Y;  //стартовый Y

            // Устанавливаем стартовые счетчики.
            int rowCount = 0; //количество стикеров в строке
            int A4Count = 1;  //листов А4 получилось всего
            int codeOnA4 = 0; //стикеров на листе в данный момент


            using (Graphics pic = Graphics.FromImage(bmp))
            {
                pic.Clear(Color.White);
                MemoryStream stream;
                PdfPage page;
                XGraphics gfx;
                XImage Ximage;
                XRect xRect;

                for (int i = 0; i < CodeList.Count; i++)
                {
                    if (codeOnA4 >= maxSticksOnA4) //если кодов нарисовано Max, то обнуляем счетчики, сохраняем файл и перересовываем
                    {
                        //bmp.Save(Path.Combine(pathToFolder, $"{A4Count}_{widthMm}x{heightMm}.jpg"), ImageFormat.Jpeg);
                        //создание отдельного потока и запихивание в него изображения
                        stream = new MemoryStream();
                        bmp.Save(stream, ImageFormat.Jpeg);
                        stream.Position = 0;
                        page = doc.AddPage();
                        page.Size = PdfSharp.PageSize.A4;
                        //page.Width = bmp.Width;
                        //page.Height = bmp.Height;
                        gfx = XGraphics.FromPdfPage(page);
                        Ximage = XImage.FromStream(stream);
                        xRect = new XRect(0, 0, page.Width, page.Height);
                        gfx.DrawImage(Ximage, xRect);

                        Ximage.Dispose();
                        gfx.Dispose();
                        stream.Dispose();

                        pic.Clear(Color.White);
                        codeOnA4 = 0;
                        horizPixel = startPoint.X; //стартовый Х
                        vertPixel = startPoint.Y;  //стартовый Y
                        A4Count++;
                    }

                    var qr = qRCodes.Where(u => u.Art == CodeList[i].VendorCode && u.Size == CodeList[i].Size && !u.IsUsing).FirstOrDefault();

                    if (qr != null)
                    {
                        CodeList[i].QRCode = qr;
                        qr.IsUsing = true;
                    }


                    Image image = GetSticker(CodeList[i], w, h, note, FontSize, true);
                    pic.DrawImage(image, horizPixel + (image.Width * (1 - Percent)) / 2, vertPixel + (image.Height * (1 - Percent)) / 2, image.Width * Percent, image.Height * Percent);
                    horizPixel += image.Width;

                    rowCount++;
                    if (rowCount >= maxColumns)
                    {
                        horizPixel = startPoint.X;
                        vertPixel += image.Height;
                        rowCount = 0;
                    }
                    codeOnA4++;
                }
                // bmp.Save(Path.Combine(pathToFolder, $"{A4Count}_{widthMm}x{heightMm}.jpg"), ImageFormat.Jpeg);

                //создание отдельного потока и запихивание в него изображения
                stream = new MemoryStream();
                bmp.Save(stream, ImageFormat.Jpeg);
                stream.Position = 0;
                page = doc.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                gfx = XGraphics.FromPdfPage(page);
                Ximage = XImage.FromStream(stream);
                xRect = new XRect(0, 0, page.Width, page.Height);
                gfx.DrawImage(Ximage, xRect);

                Ximage.Dispose();
                gfx.Dispose();
                stream.Dispose();
            }

            return doc;
        }
        static private float GetRangeInPercent(float percent, int x)
        {
            return (percent / (100 / (float)x));
        }
    }
}
