using Microsoft.Win32;
using ShopLabelGenerator.Infrastructure.Commands;
using ShopLabelGenerator.Models;
using ShopLabelGenerator.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TelegramBotNikBitNode.ViewModels.Base;

namespace ShopLabelGenerator.ViewModels.Windows
{
    internal class MainWindowViewModel : ViewModel
    {
        public MainWindowViewModel()
        {
            #region Команды

            OpenQRCodesFileCommand = new RelayCommand(OnOpenQRCodesFileCommandExecuted, CanOpenQRCodesFileCommandExcecut);
            OpenBARCodeFileCommand = new RelayCommand(OnOpenBARCodeFileCommandExecuted, CanOpenBARCodeFileCommandExcecut);
            OpenProductFileCommand = new RelayCommand(OnOpenProductFileCommandExecuted, CanOpenProductFileCommandExcecut);
            CreateCommand = new RelayCommand(OnCreateCommandExecuted, CanCreateCommandExcecut);
            SaveDescriptionCommand = new RelayCommand(OnSaveDescriptionCommandExecuted, CanSaveDescriptionCommandExcecut);

            #endregion

            Description = Properties.Settings.Default.Description;
        }

        #region Свойства

        #region Путь до файла с QR кодами
        private string _QRCodesPath;
        /// <summary>Путь до файла с QR кодам</summary>
        public string QRCodesPath
        {
            get => _QRCodesPath;
            set => SetProperty(ref _QRCodesPath, value);
        }
        #endregion

        #region Путь до файла с BAR кодами
        private string _BARCodesPath;
        /// <summary>Путь до файла с BAR кодам</summary>
        public string BARCodesPath
        {
            get => _BARCodesPath;
            set => SetProperty(ref _BARCodesPath, value);
        }
        #endregion

        private int _fontSize = 100;
        public int FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        #region Путь до файла с товарами
        private string _productTablePath;
        /// <summary>Путь до файла с товарами</summary>
        public string ProductTablePath
        {
            get => _productTablePath;
            set => SetProperty(ref _productTablePath, value);
        }
        #endregion

        #region Описание
        private string _description;
        /// <summary>Описание</summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        #endregion

        #region Статус программы
        private string _status = "Выберите файлы с данными";
        /// <summary>Статус программы</summary>
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        #endregion

        #region Работа
        private bool _isNotWorking = true;
        /// <summary>Работа</summary>
        public bool IsNotWorking
        {
            get => _isNotWorking;
            set => SetProperty(ref _isNotWorking, value);
        }
        #endregion

        #endregion

        #region Команды

        #region Указать файл QR
        public ICommand OpenQRCodesFileCommand { get; }
        private bool CanOpenQRCodesFileCommandExcecut(object p) => true;
        private void OnOpenQRCodesFileCommandExecuted(object p)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
            {
                //FileName = "Document", // Default file name
                //DefaultExt = ".pdf", // Default file extension
                Filter = "Файл PDF (.pdf)|*.pdf" // Filter files by extension
            };

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                QRCodesPath = dlg.FileName;
            }
        }
        #endregion

        #region Указать файл со штрих кодами
        public ICommand OpenBARCodeFileCommand { get; }
        private bool CanOpenBARCodeFileCommandExcecut(object p) => true;
        private void OnOpenBARCodeFileCommandExecuted(object p)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
            {
                //FileName = "Document", // Default file name
                //DefaultExt = ".pdf", // Default file extension
                Filter = "Файл электронных таблиц |*.xls; *.xlsx" // Filter files by extension
            };

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                BARCodesPath = dlg.FileName;
            }
        }
        #endregion

        #region Указать файл с продукцией
        public ICommand OpenProductFileCommand { get; }
        private bool CanOpenProductFileCommandExcecut(object p) => true;
        private void OnOpenProductFileCommandExecuted(object p)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
            {
                //FileName = "Document", // Default file name
                //DefaultExt = ".pdf", // Default file extension
                Filter = "Файл электронных таблиц |*.xls; *.xlsx" // Filter files by extension
            };

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                ProductTablePath = dlg.FileName;
            }
        }
        #endregion

        #region Сохранить описание
        public ICommand SaveDescriptionCommand { get; }
        private bool CanSaveDescriptionCommandExcecut(object p) => true;
        private void OnSaveDescriptionCommandExecuted(object p)
        {
            Properties.Settings.Default.Description = Description;
            Properties.Settings.Default.Save();
            MessageBox.Show("Описание успешно сохранено", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region Запуск
        public ICommand CreateCommand { get; }
        private bool CanCreateCommandExcecut(object p) => true;
        private async void OnCreateCommandExecuted(object p)
        {
            await Task.Run(() =>
            {
                IsNotWorking = false;

                // Считываение данных штрих-кодов
                Status = "Считываение данных штрих-кодов";
               // ICollection<BarCode> barcodes = ExcelWorker.GetBarCodesData(BARCodesPath);
                ICollection<BarCode> barcodes = ExcelWorker.GetBarCodesData(ProductTablePath);
                // Считывание данных по товарам
                Status = "Считывание данных по товарам";
                List<Product> products = ExcelWorker.GetProductsData(ProductTablePath, barcodes).ToList();
                List<Product> prod = new List<Product>();

                // Извлекаем QR-коды и текстовое описание
                Status = "Извлечение QR-кодов и текстовых описаний";
                var QRCodes = PDF.ExtractImagesFromPDF(QRCodesPath);

                if (QRCodes.Count < products.Count)
                {
                    MessageBox.Show($"Количество QR-кодов ({QRCodes.Count}) не соответствует количеству товаров ({products.Count})");
                    Status = "Ошибка";
                    return;
                }


                // Присвоение QR кодов
                foreach (var product in products)
                {
                    var qr = QRCodes.Where(u => u.Art.Contains(product.VendorCode) && u.Size == product.Size && !u.IsUsing).FirstOrDefault();

                    if (qr != null)
                    {
                        product.QRCode = new QRCode()
                        {
                            QRCodeImage = qr.QRCodeImage,
                            QRBottomPart = qr.QRBottomPart,
                            QRTopPart = qr.QRTopPart
                        };
                        qr.IsUsing = true;
                    }
                    else
                    {
                        Status = "Ошибка сопоставления данных";
                        return;
                    }
                    // Присвоение бар-кодов
                    var bar = barcodes.Where(x => x.Size == product.Size && x.VendorCode == product.VendorCode).FirstOrDefault();
                    if (bar != null)
                    {
                        product.BARCode = bar.BARCode;
                    }
                    else
                    {
                        Status = "Ошибка сопоставления данных (штрих-код)";
                        return;
                    }

                    prod.Add(new Product()
                    {
                        Type = product.Type,
                        Size = product.Size,
                        VendorCode = product.VendorCode,
                        Color = product.Color,
                        Insole = product.Insole,
                        ProductionDate = product.ProductionDate,
                        SoleMaterial = product.SoleMaterial,
                        Name = product.Name,
                        TopMaterial = product.TopMaterial,
                        QRCode = qr,
                        BARCode = bar.BARCode
                    });
                }

                Status = "Формирование этикеток";

                var pdfDocument = ImageCreator.CreateStickers(prod, Description, 105, 37, QRCodes, FontSize);

                SaveFileDialog sd = new SaveFileDialog
                {
                    Title = "Выберите место для сохранения этикеток",
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"Этикетки_{DateTime.Now:dd.MM.yyyy}"
                };
                if (sd.ShowDialog() == true)
                {
                    pdfDocument.Save(sd.FileName);
                    Process.Start(sd.FileName);
                }

                pdfDocument.Dispose();
                Status = "Готово";
                IsNotWorking = true;
            });

        }
        #endregion

        #endregion


    }
}
