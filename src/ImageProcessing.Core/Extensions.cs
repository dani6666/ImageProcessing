using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ImageProcessing.Core.Model;
using ImageProcessing.Core.Services;

namespace ImageProcessing.Core;

public static class Extensions
{
    public static Pixel[,] ToPixels(this Bitmap bitmap)
    {
        var data = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            bitmap.PixelFormat);
        
        var size = data.Stride * data.Height;
        var buffer = new byte[size];
        Marshal.Copy(data.Scan0, buffer, 0, size);
        
        bitmap.UnlockBits(data);
        return ImageHelpers.ConvertTo2d(buffer, bitmap.Width);
    }
}