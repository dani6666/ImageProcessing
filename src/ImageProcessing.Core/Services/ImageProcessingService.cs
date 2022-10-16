using System.Drawing;
using System.Drawing.Imaging;
using ImageProcessing.Core.Interfaces.Services;
namespace ImageProcessing.Core.Services;

public class ImageProcessingService : IImageProcessingService
{


    private Rectangle GetRectangle((byte R, byte G, byte B)[,] pixels)
    {
       //TODO: method for finding the red rectangle
        return new Rectangle();
    }

    public Bitmap ProcessPixels(Bitmap bitmap)
    {
        BitmapData data = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height), 
            ImageLockMode.ReadWrite, 
            bitmap.PixelFormat);
        /*the size of the image in bytes */
        int size = data.Stride * data.Height;
        var buffer = new byte[size];

        /*This overload copies data of /size/ into /data/ from location specified (/Scan0/)*/
        System.Runtime.InteropServices.Marshal.Copy(data.Scan0, buffer, 0, size);

        var matrix = ImageHelpers.ConvertTo2d(buffer, bitmap.Width);
        var rectangle = GetRectangle(matrix);
        buffer = ImageHelpers.ConvertTo1d(matrix);
        //TODO: draw the rectangle on image
        /* This override copies the data back into the location specified */
        System.Runtime.InteropServices.Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);

        bitmap.UnlockBits(data);
        return bitmap;
    }
}
