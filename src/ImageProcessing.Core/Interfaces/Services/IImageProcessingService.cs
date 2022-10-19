using System.Drawing;

namespace ImageProcessing.Core.Interfaces.Services;

public interface IImageProcessingService
{
    void ProcessPixels(Bitmap pixels);
}