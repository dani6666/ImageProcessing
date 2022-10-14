using ImageProcessing.Core.Interfaces.Services;

namespace ImageProcessing.Core.Services;

public class ImageProcessingService : IImageProcessingService
{
    public byte[,] ProcessImage(byte[,] pixels)
    {
        return pixels;
    }
}
