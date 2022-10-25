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
        return pixels.Transform(rgb => rgb.AsHsv());
    }
    
    public static PixelRgb[,] AsRgb(this PixelHsv[,] pixels)
    {
        return pixels.Transform(hsv => hsv.AsRgb());
    }

    public static PixelRgb[,] AsRgb(this bool[,] mask)
    {
        return mask.Transform(b => b ? new PixelRgb(255, 255, 255) : new PixelRgb(0, 0, 0));
    }

    public static bool[,] Or(this bool[,] mask, bool[,] other)
    {
        return mask.Transform(other, (b1, b2) => b1 | b2);
    }

    public static bool[,] And(this bool[,] mask, bool[,] other)
    {
        return mask.Transform(other, (b1, b2) => b1 && b2);
    }

    public static T2[,] Transform<T1, T2>(this T1[,] input, Func<T1, T2> func)
    {
        var rows = input.GetLength(0);
        var cols = input.GetLength(1);
        var result = new T2[rows, cols];
        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                result[r, c] = func(input[r, c]);
            }
        }
        return result;
    }
    
    public static T3[,] Transform<T1, T2, T3>(this T1[,] input, T2[,] other, Func<T1, T2, T3> func)
    {
        var rows = input.GetLength(0);
        var cols = input.GetLength(1);
        var result = new T3[rows, cols];
        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                result[r, c] = func(input[r, c], other[r, c]);
            }
        }
        return result;
    }
}
