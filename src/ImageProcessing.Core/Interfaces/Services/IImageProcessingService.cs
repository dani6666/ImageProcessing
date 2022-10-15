using System.Drawing;

namespace ImageProcessing.Core.Interfaces.Services;

public interface IImageProcessingService
{
    Bitmap ProcessPixels(Bitmap pixels);
}