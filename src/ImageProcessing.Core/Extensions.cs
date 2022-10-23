using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ImageProcessing.Core.Model;
using ImageProcessing.Core.Services;

namespace ImageProcessing.Core;

public static class Extensions
{
    public static PixelRgb[,] ToPixels(this Bitmap bitmap)
    {
        var data = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);
        
        var size = data.Stride * data.Height;
        var buffer = new byte[size];
        Marshal.Copy(data.Scan0, buffer, 0, size);
        
        bitmap.UnlockBits(data);
        return ImageHelpers.ConvertTo2d(buffer, bitmap.Width);
    }

    public static PixelHsv[,] AsHsv(this PixelRgb[,] pixels)
    {
        var rows = pixels.GetLength(0);
        var cols = pixels.GetLength(1);
        var result = new PixelHsv[rows, cols];
        for (var r = 0; r < rows; r++)
            for (var c = 0; c < cols; c++)
                result[r, c] = pixels[r, c].AsHsv();
        return result;
    }
    
    public static PixelRgb[,] AsRgb(this PixelHsv[,] pixels)
    {
        var rows = pixels.GetLength(0);
        var cols = pixels.GetLength(1);
        var result = new PixelRgb[rows, cols];
        for (var r = 0; r < rows; r++)
            for (var c = 0; c < cols; c++)
                result[r, c] = pixels[r, c].AsRgb();
        return result;
    }

    public static PixelRgb[,] AsRgb(this bool[,] mask)
    {
        var rows = mask.GetLength(0);
        var cols = mask.GetLength(1);
        var result = new PixelRgb[rows, cols];
        for (var r = 0; r < rows; r++)
        for (var c = 0; c < cols; c++)
            result[r, c] = mask[r, c] ? new PixelRgb(255, 255, 255) : new PixelRgb(0, 0, 0);
        return result;
    }

    public static bool[,] Or(this bool[,] mask, bool[,] other)
    {
        var rows = mask.GetLength(0);
        var cols = mask.GetLength(1);
        var result = new bool[rows, cols];
        for (var r = 0; r < rows; r++)
        for (var c = 0; c < cols; c++)
            result[r, c] = mask[r, c] || other[r, c];
        return result;
    }
}