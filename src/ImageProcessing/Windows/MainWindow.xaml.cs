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
using Color = System.Drawing.Color;

namespace ImageProcessing.Windows
{
    public partial class MainWindow : Window
    {
        private IImageProcessingService _imageProcessingService;
        private Bitmap? _bitmap;

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
            _bitmap = new Bitmap(fileUri.OriginalString);

            var rand = new Random();
            using (var graphics = Graphics.FromImage(_bitmap))
            {
                using (var myBrush = new SolidBrush(Color.FromArgb(255, 0, 0)))
                {
                    graphics.FillRectangle(myBrush, new Rectangle(rand.Next(0, _bitmap.Width),
                        rand.Next(0, _bitmap.Height),
                        rand.Next(100, 400),
                        rand.Next(100, 500)));
                }
            }
            OriginalImage.Source = _bitmap.ToBitmapImage();
        }
        private void ProcessImage_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap is null) return;

            _imageProcessingService.ProcessPixels(_bitmap);

            ProcessedImage.Source = _bitmap.ToBitmapImage();
        }
    }
}
