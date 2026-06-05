using System.IO;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using QRCoder;

namespace warsztat3000.Views
{
    public partial class PokazKodQrDialog : Window
    {
        public PokazKodQrDialog()
        {
            InitializeComponent();
        }

        public PokazKodQrDialog(string token, string statusUrl) : this()
        {
            var tokenTextControl = this.FindControl<TextBlock>("TokenText");
            if (tokenTextControl != null)
                tokenTextControl.Text = token;

            var linkTextControl = this.FindControl<TextBox>("LinkText");
            if (linkTextControl != null)
                linkTextControl.Text = statusUrl;

            var qrImage = this.FindControl<Image>("QrImage");
            if (qrImage != null)
                qrImage.Source = GenerateQrBitmap(statusUrl);
        }

        private static Bitmap GenerateQrBitmap(string text)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(data);
            var bytes = qrCode.GetGraphic(12);
            return new Bitmap(new MemoryStream(bytes));
        }

        private void Zamknij_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void KopiujLink_Click(object sender, RoutedEventArgs e)
        {
            var linkTextControl = this.FindControl<TextBox>("LinkText");
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (linkTextControl == null || clipboard == null)
                return;

            await clipboard.SetTextAsync(linkTextControl.Text ?? string.Empty);
        }
    }
}
