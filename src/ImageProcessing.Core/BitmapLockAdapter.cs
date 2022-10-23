using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ImageProcessing.Core.Model;
using ImageProcessing.Core.Services;

namespace ImageProcessing.Core;

internal class BitmapLockAdapter : IDisposable
{
    private readonly Bitmap _bitmap;
    private readonly BitmapData _data;
    private byte[] _buffer;

    public BitmapLockAdapter(Bitmap bitmap)
    {
        _bitmap = bitmap;
        _data = bitmap.LockBits(
            new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
            ImageLockMode.ReadWrite,
            PixelFormat.Format24bppRgb);
        
        var size = _data.Stride * _data.Height;
        _buffer = new byte[size];
        Marshal.Copy(_data.Scan0, _buffer, 0, size);
    }

    public void Dispose()
    {
        Marshal.Copy(_buffer, 0, _data.Scan0, _buffer.Length);
        _bitmap.UnlockBits(_data);
    }

    public PixelRgb[,] ReadPixels()
    {
        return ImageHelpers.ConvertTo2d(_buffer, _bitmap.Width);
    }

    public void WritePixels(PixelRgb[,] pixels)
    {
        var buffer = ImageHelpers.ConvertTo1d(pixels);
        _buffer = buffer;
    }
}
