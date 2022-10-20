using ImageProcessing.Core.Interfaces.Services;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Windows;
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
            _imageProcessingService.DrawRectangle(_bitmap,
                new Rectangle(rand.Next(0, _bitmap.Width),
                        rand.Next(0, _bitmap.Height),
                        rand.Next(100, 400),
                        rand.Next(100, 500)),
                Color.FromArgb(255, 0, 0),
                true);
            
            OriginalImage.Source = _bitmap.ToBitmapImage();
        }
        private void ProcessImage_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap is null) return;

            var bitmap = _bitmap.Clone(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), _bitmap.PixelFormat);
            _imageProcessingService.ProcessPixels(bitmap);

            ProcessedImage.Source = bitmap.ToBitmapImage();
        }
    }
}
