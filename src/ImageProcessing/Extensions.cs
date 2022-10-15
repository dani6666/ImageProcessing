using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImageProcessing
{
    public static class Extensions
    {
        public static BitmapImage ToImageBitmap(this Bitmap bitmap)
        {
            var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Bmp);
            var image = new BitmapImage();
            image.BeginInit();
            stream.Seek(0, SeekOrigin.Begin);
            image.StreamSource = stream;
            image.EndInit();

            return image;
        }
    }
}
