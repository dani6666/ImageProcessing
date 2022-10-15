using ImageProcessing.Core.Interfaces.Services;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageProcessing.Core.Services;

namespace ImageProcessing.Windows
{
    public partial class MainWindow : Window
    {
        private IImageProcessingService _imageProcessingService;
        private Bitmap? _image;

        public MainWindow(IImageProcessingService imageProcessingService)
        {
            _imageProcessingService = imageProcessingService;

            InitializeComponent();
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != true) return;

            var fileUri = new Uri(openFileDialog.FileName);
            _image = new Bitmap(fileUri.OriginalString);
            OriginalImage.Source = _image.ToImageBitmap();
        }

        private void ProcessImage_Click(object sender, RoutedEventArgs e)
        {
            if (_image is null) return;

            var ips = new ImageProcessingService();
            ips.ProcessPixels(_image);

            OriginalImage.Source = _image.ToImageBitmap();
            //var originalImage = OriginalImage.Source as BitmapImage;
            // int stride = (originalImage.PixelWidth * originalImage.Format.BitsPerPixel + 7) / 8;
            // byte[] pixels = new byte[originalImage.PixelHeight * stride];
            /// originalImage.CopyPixels(pixels, stride, 0);
        }
    }
}
