using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ImageProcessing.Core.Interfaces.Services;
using ImageProcessing.Core.Model;

namespace ImageProcessing.Core.Services;

public class ImageProcessingService : IImageProcessingService
{
    private static IEnumerable<Rectangle> FindRectangles(Pixel[,] pixels)
    {
        int rowsCount = pixels.GetLength(0);
        int colsCount = pixels.GetLength(1);
        for (int r = 0; r < rowsCount; r++)
        {
            var wasLastPixelRed = false;
            for (int c = 0; c < colsCount; c++)
            {
                if (pixels[r, c].IsRed())
                {
                    if (!wasLastPixelRed)
                    {
                        var rect = CheckForRectangle(pixels, r, c);
                        if (rect.HasValue)
                            yield return rect.Value;
                        wasLastPixelRed = true;
                    }
                }
                else
                    wasLastPixelRed = false;
            }
        }
    }

    private static Rectangle? CheckForRectangle(Pixel[,] pixels, int r, int c)
    {
        var rLast = pixels.GetLength(0) - 1;
        var cLast = pixels.GetLength(1) - 1;

        // Searching for end of red lines
        // endR, endC mark last red pixel
        var endR = r;
        while (endR + 1 <= rLast && pixels[endR + 1, c].IsRed())
            endR++;

        if (endR == r)
            return null;

        var endC = c;
        while (endC + 1 <= cLast && pixels[r, endC + 1].IsRed())
            endC++;

        if (endC == c)
            return null;

        // Checking that there are on red pixels touching rectangle (then it is not a rectangle)
        // Math.Min/Max make sure not exceed array if rect touches image border
        for (int i = Math.Max(r - 1, 0); i <= Math.Min(endR + 1, rLast); i++)
        {
            if ((c > 0 && pixels[i, c - 1].IsRed()) || (endC < cLast && pixels[i, endC + 1].IsRed()))
                return null;
        }

        for (int i = Math.Max(c - 1, 0); i <= Math.Min(endC + 1, cLast); i++)
        {
            if ((r > 0 && pixels[r - 1, i].IsRed()) || (endR < rLast && pixels[endR + 1, i].IsRed()))
                return null;
        }


        // Checking rectangle fill
        for (int i = r; i <= endR; i++)
        {
            for (int j = c; j <= endC; j++)
            {
                if (!pixels[i, j].IsRed())
                    return null;
            }
        }
        // 2d arrays are indexed [rowI, colI],
        // but geometrically we pass coordinates as (x,y),
        // where x - horizontal dimension (col), y - vertical dimension (x)
        return new Rectangle(c, r, endC - c, endR - r);
    }

    public void ProcessPixels(Bitmap bitmap)
    {
        var data = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite,
            bitmap.PixelFormat);
        /*the size of the image in bytes */
        int size = data.Stride * data.Height;
        var buffer = new byte[size];

        /*This overload copies data of /size/ into /data/ from location specified (/Scan0/)*/
        Marshal.Copy(data.Scan0, buffer, 0, size);

        var matrix = ImageHelpers.ConvertTo2d(buffer, bitmap.Width);

        var rectangles = FindRectangles(matrix).ToList();

        buffer = ImageHelpers.ConvertTo1d(matrix);

        /* This override copies the data back into the location specified */
        Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);

        bitmap.UnlockBits(data);
        foreach (var rectangle in rectangles)
            DrawRectangle(bitmap, rectangle, Color.Black, false);
    }

    public void DrawRectangle(Bitmap bitmap, Rectangle rectangle, Color color, bool fill)
    {
        using (var graphics = Graphics.FromImage(bitmap))
        {
            if (fill)
            {
                using (var brush = new SolidBrush(color))
                {
                    graphics.FillRectangle(brush, rectangle);
                }
            }

            else
            {
                using (Pen pen = new Pen(color, 4))
                {
                    graphics.DrawRectangle(pen, rectangle);
                }
            }

        }
    }
}
