using System.Drawing;

namespace ImageProcessing.Core.Interfaces.Services;

public interface IImageProcessingService
{
    //void DrawRectangle(Bitmap bitmap, Rectangle rectangle, Color color, bool fill);
    void FindRectangles(Bitmap bitmap);
    void ShowBoundingCircles(Bitmap bitmap);
    void FindTriangles(Bitmap bitmap);
    void RemoveNoise(Bitmap pixels);
}