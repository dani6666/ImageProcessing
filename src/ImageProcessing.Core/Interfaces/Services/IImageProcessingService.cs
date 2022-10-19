using System.Drawing;

namespace ImageProcessing.Core.Interfaces.Services;

public interface IImageProcessingService
{
    void DrawRectangle(Bitmap bitmap, Rectangle rectangle, Color color, bool fill);
    void ProcessPixels(Bitmap pixels);
}