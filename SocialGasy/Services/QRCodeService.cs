using QRCoder;

namespace SocialGasy.Services
{
    public class QRCodeService
    {
        public byte[] GenerateQRCode(string text)
        {
            if (string.IsNullOrEmpty(text)) return Array.Empty<byte>();

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(20);
            }
        }
    }
}