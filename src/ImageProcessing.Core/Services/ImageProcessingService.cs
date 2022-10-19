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
        for (int i = 0; i < pixels.GetLength(0); i++)
        {
            var wasLastPixelRed = false;
            for (int j = 0; j < pixels.GetLength(1); j++)
            {
                if (pixels[i, j].IsRed())
                {
                    if (!wasLastPixelRed)
                    {
                        var rect = CheckForRectangle(pixels, i, j);
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

    private static Rectangle? CheckForRectangle(Pixel[,] pixels, int x, int y)
    {
        var xLast = pixels.GetLength(0) - 1;
        var yLast = pixels.GetLength(1) - 1;

        // Searching for end of red lines
        // endX, endY mark last red pixel
        var endX = x;
        while (endX + 1 <= xLast && pixels[endX + 1, y].IsRed())
            endX++;

        if (endX == x)
            return null;

        var endY = y;
        while (endY + 1 <= yLast && pixels[x, endY + 1].IsRed())
            endY++;

        if (endY == y)
            return null;

        // Checking that there are on red pixels touching rectangle (then it is not a rectangle)
        // Math.Min/Max make sure not exceed array if rect touches image border
        for (int i = Math.Max(x - 1, 0); i <= Math.Min(endX + 1, xLast); i++)
        {
            if ((y > 0 && pixels[i, y - 1].IsRed()) || (endY < yLast && pixels[i, endY + 1].IsRed()))
                return null;
        }

        for (int i = Math.Max(y - 1, 0); i <= Math.Min(endY + 1, yLast); i++)
        {
            if ((x > 0 && pixels[x - 1, i].IsRed()) || (endX < xLast && pixels[endX + 1, i].IsRed()))
                return null;
        }


        // Checking rectangle fill
        for (int i = x; i <= endX; i++)
        {
            for (int j = y; j <= endY; j++)
            {
                if (!pixels[i, j].IsRed())
                    return null;
            }
        }

        return new Rectangle(x, y, endX - x, endY - y);
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

        foreach (var rectangle in rectangles)
            MarkRect(matrix, rectangle);

        buffer = ImageHelpers.ConvertTo1d(matrix);

        /* This override copies the data back into the location specified */
        Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);

        bitmap.UnlockBits(data);
    }

    private static void MarkRect(Pixel[,] matrix, Rectangle rectangle)
    {
        for (int i = rectangle.Top; i < rectangle.Bottom; i++)
        {
            matrix[rectangle.Left, i] = new Pixel(0, 0, 0);
            matrix[rectangle.Right, i] = new Pixel(0, 0, 0);
            matrix[rectangle.Left + 1, i] = new Pixel(0, 0, 0);
            matrix[rectangle.Right - 1, i] = new Pixel(0, 0, 0);
        }

        for (int i = rectangle.Left; i < rectangle.Right; i++)
        {
            matrix[i, rectangle.Bottom] = new Pixel(0, 0, 0);
            matrix[i, rectangle.Top] = new Pixel(0, 0, 0);
            matrix[i, rectangle.Bottom - 1] = new Pixel(0, 0, 0);
            matrix[i, rectangle.Top + 1] = new Pixel(0, 0, 0);
        }
    }
}
