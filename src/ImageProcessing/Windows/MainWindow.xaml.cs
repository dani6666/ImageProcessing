using ImageProcessing.Core.Interfaces.Services;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using LinearGradientBrush = System.Drawing.Drawing2D.LinearGradientBrush;
using Point = System.Drawing.Point;

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

            OriginalImage.Source = _bitmap.ToBitmapImage();
        }
        private void FindRectangles_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap is null) return;

            // var rand = new Random();
            // _imageProcessingService.DrawRectangle(_bitmap,
            //     new Rectangle(rand.Next(0, _bitmap.Width),
            //             rand.Next(0, _bitmap.Height),
            //             rand.Next(100, 400),
            //             rand.Next(100, 500)),
            //     Color.FromArgb(255, 0, 0),
            //     true);
            // OriginalImage.Source = _bitmap.ToBitmapImage();

            var bitmap = _bitmap.Clone(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), _bitmap.PixelFormat);
            _imageProcessingService.FindRectangles(bitmap);
            ProcessedImage.Source = bitmap.ToBitmapImage();
        }

        private void FindOvals_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap is null) return;

            // var rand = new Random();
            // _imageProcessingService.DrawRectangle(_bitmap,
            //     new Rectangle(rand.Next(0, _bitmap.Width),
            //             rand.Next(0, _bitmap.Height),
            //             rand.Next(100, 400),
            //             rand.Next(100, 500)),
            //     Color.FromArgb(255, 0, 0),
            //     true);
            // OriginalImage.Source = _bitmap.ToBitmapImage();

            var bitmap = _bitmap.Clone(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), _bitmap.PixelFormat);
            _imageProcessingService.ShowBoundingCircles(bitmap);
            ProcessedImage.Source = bitmap.ToBitmapImage();
        }


        private void FindTriangles_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap is null) return;

            // var rand = new Random();
            // _imageProcessingService.DrawRectangle(_bitmap,
            //     new Rectangle(rand.Next(0, _bitmap.Width),
            //             rand.Next(0, _bitmap.Height),
            //             rand.Next(100, 400),
            //             rand.Next(100, 500)),
            //     Color.FromArgb(255, 0, 0),
            //     true);
            // OriginalImage.Source = _bitmap.ToBitmapImage();

            var bitmap = _bitmap.Clone(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), _bitmap.PixelFormat);
            _imageProcessingService.FindTriangles(bitmap);
            ProcessedImage.Source = bitmap.ToBitmapImage();
        }

        private void FindRocks_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap is null) return;

            // var rand = new Random();
            // _imageProcessingService.DrawRectangle(_bitmap,
            //     new Rectangle(rand.Next(0, _bitmap.Width),
            //             rand.Next(0, _bitmap.Height),
            //             rand.Next(100, 400),
            //             rand.Next(100, 500)),
            //     Color.FromArgb(255, 0, 0),
            //     true);
            // OriginalImage.Source = _bitmap.ToBitmapImage();

            var bitmap = _bitmap.Clone(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), _bitmap.PixelFormat);
            _imageProcessingService.FindRocks(bitmap);
            ProcessedImage.Source = bitmap.ToBitmapImage();
        }

        private void ShowStats_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap is null) return;

            var histogram = _imageProcessingService.GetColorStats(_bitmap, 9);
            var emptyBitmap = new Bitmap(1000, 1000);
            using var graphics = Graphics.FromImage(emptyBitmap);
            using var whiteBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
            graphics.FillRectangle(whiteBrush, new Rectangle(0, 0,_bitmap.Width,_bitmap.Width));
            var i = 0;
            foreach(var entry in histogram)
            {
                var lower = entry.Key.Item1.AsRgb();
                var upper = entry.Key.Item2.AsRgb();
                var brush = new LinearGradientBrush(
                    new Point(0, 10),
                    new Point(200, 10),
                    Color.FromArgb(lower.R, lower.G, lower.B),
                    Color.FromArgb(upper.R, upper.G, upper.B));
                graphics.FillRectangle(brush, 0, 100 * i, 200, 50);
                RectangleF rectf = new RectangleF(250, 100 * i, 500, 50);
                graphics.DrawString("Hue range " + entry.Key.Item1.H + " - " + entry.Key.Item2.H + ": " + entry.Value.ToString(), 
                    new Font("Tahoma", 18), Brushes.Black, rectf);
                i++;
            }

            ProcessedImage.Source = emptyBitmap.ToBitmapImage();
        }

        private void ShowHue_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap is null) return;
            var bitmap = _bitmap.Clone(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), _bitmap.PixelFormat);
            _imageProcessingService.ShowHue(bitmap);
            ProcessedImage.Source = bitmap.ToBitmapImage();

        }


        private void RemoveNoise_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap is null) return;

            var bitmap = _bitmap.Clone(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), _bitmap.PixelFormat);
            _imageProcessingService.RemoveNoise(bitmap);
            ProcessedImage.Source = bitmap.ToBitmapImage();
        }
    }
}
