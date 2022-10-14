using ImageProcessing.Core.Interfaces.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageProcessing.Windows
{
    public partial class MainWindow : Window
    {
        private IImageProcessingService _imageProcessingService;

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
            OriginalImage.Source = new BitmapImage(fileUri);
        }

        private void ProcessImage_Click(object sender, RoutedEventArgs e)
        {
            var originalImage = OriginalImage.Source as BitmapImage;

            int stride = (originalImage.PixelWidth * originalImage.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[originalImage.PixelHeight * stride];
            originalImage.CopyPixels(pixels, stride, 0);
        }
    }
}
