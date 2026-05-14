using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Windows.Media.Imaging;

namespace CourseProgect_Planeta35.Models
{
    public class InventoryItem : INotifyPropertyChanged
    {
        private Asset _asset;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("inventory_id")]
        public int InventoryId { get; set; }
        [ForeignKey("InventoryId")]
        public Inventory Inventory { get; set; }

        [Required]
        [Column("asset_id")]
        public int AssetId { get; set; }
        [ForeignKey("AssetId")]
        public Asset Asset
        {
            get => _asset;
            set
            {
                _asset = value;
                _qrImage = null;
                OnPropertyChanged(nameof(Asset));
            }
        }

        private string _imagePathToShow;

        public string ImagePathToShow
        {
            get => _imagePathToShow;
            set
            {
                _imagePathToShow = value;
                OnPropertyChanged(nameof(ImagePathToShow));
            }
        }

        [Required, MaxLength(50)]
        [Column("status")]
        public string Status { get; set; }

        [MaxLength(255)]
        [Column("note")]
        public string Note { get; set; }
        public int ResponsiblePersonId { get; set; }

        private BitmapImage _qrImage;
        private bool _qrGenerated;

        [NotMapped]
        public BitmapImage QRImage
        {
            get => _qrImage;
            private set
            {
                _qrImage = value;
                OnPropertyChanged(nameof(QRImage));
            }
        }

        public async Task GenerateMultipleQrsAsync(int count)
        {
            for (int i = 1; i <= count; i++)
            {
                string url = $"http://localhost:8080/asset/{i}";

                var bitmap = await CreateQrBitmapAsync(url);

                this.QRImage = bitmap;
            }
        }

        private async Task<BitmapImage> CreateQrBitmapAsync(string payload)
        {
            return await Task.Run(() =>
            {
                var qrGenerator = new QRCoder.QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(payload, QRCoder.QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
                var bytes = qrCode.GetGraphic(20);

                using var ms = new MemoryStream(bytes);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            });
        }
    }
}
