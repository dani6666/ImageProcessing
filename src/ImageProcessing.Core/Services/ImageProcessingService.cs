using System.Drawing;
using System.Drawing.Imaging;
using ImageProcessing.Core.Interfaces.Services;

namespace ImageProcessing.Core.Services;

public class ImageProcessingService : IImageProcessingService
{
    // public byte[,] ProcessPixels(byte[,] pixels)
    // {
    //     return pixels;
    // }

    public Bitmap ProcessPixels(Bitmap bitmap)
    {
        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        int bitsPerPixel = Image.GetPixelFormatSize(data.PixelFormat);

        /*the size of the image in bytes */
        int size = data.Stride * data.Height;
        var buffer = new byte[size];

        /*This overload copies data of /size/ into /data/ from location specified (/Scan0/)*/
        System.Runtime.InteropServices.Marshal.Copy(data.Scan0, buffer, 0, size);

        for (int i = 0; i < size; i += bitsPerPixel / 8 )
        {
            // B
            buffer[i] = 255;
            // G
            // buffer[i + 1] = 0;
            // R
            // buffer[i + 2] = 0;
            
            //double magnitude = 1/3d * (buffer[i] + buffer[i + 1] + buffer[i + 2]);
            //data[i] is the first of 3 bytes of color

        }

        /* This override copies the data back into the location specified */
        System.Runtime.InteropServices.Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);

        bitmap.UnlockBits(data);
        return bitmap;
    }
}
