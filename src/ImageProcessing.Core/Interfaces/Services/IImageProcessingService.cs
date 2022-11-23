using System.Drawing;

namespace ImageProcessing.Core.Interfaces.Services;

public interface IImageProcessingService
{
    //void DrawRectangle(Bitmap bitmap, Rectangle rectangle, Color color, bool fill);
    public void FindRectangles(Bitmap bitmap);
    public void ShowBoundingCircles(Bitmap bitmap);
    void RemoveNoise(Bitmap pixels);
}