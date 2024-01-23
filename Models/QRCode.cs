using Dotnet = System.Drawing.Image;

namespace ShopLabelGenerator.Models
{
    public class QRCode
    {
        public int Id { get; set; }
        public string Size { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string Art { get; set; }
        public string QRTopPart { get; set; }
        public string QRBottomPart { get; set; }
        public Dotnet QRCodeImage { get; set; }
        public bool IsUsing { get; set; }
    }
}
