﻿using ImageProcessing.Core.Model;
using System.Drawing;
using Rectangle = System.Drawing.Rectangle;

namespace ImageProcessing.Core.Interfaces.Services;

public interface IImageProcessingService
{
    //void DrawRectangle(Bitmap bitmap, Rectangle rectangle, Color color, bool fill);
    void FindRectangles(Bitmap bitmap);
    void ShowBoundingCircles(Bitmap bitmap);
    void FindTriangles(Bitmap bitmap);
    void RemoveNoise(Bitmap pixels);
    Dictionary<(PixelHsv, PixelHsv), int> GetColorStats(Bitmap bitmap, int numOfBuckets);
}