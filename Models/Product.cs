namespace ShopLabelGenerator.Models
{
    public class Product
    {
        /// <summary>
        /// Артикул
        /// </summary>
        public string VendorCode { get; set; }
        /// <summary>
        /// Штрихкод
        /// </summary>
        public string BARCode { get; set; }
        /// <summary>
        /// Размер
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Цвет
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// Тип
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Материал верха
        /// </summary>
        public string TopMaterial { get; set; }
        /// <summary>
        /// Cтелькa (материал подкладки)
        /// </summary>
        public string Insole { get; set; }
        /// <summary>
        /// Материал подошвы
        /// </summary>
        public string SoleMaterial { get; set; }
        /// <summary>
        /// Дата производства
        /// </summary>
        public string ProductionDate { get; set; } = string.Empty;
        /// <summary>
        /// QR-код
        /// </summary>
        public QRCode QRCode { get; set; }
    }
}
