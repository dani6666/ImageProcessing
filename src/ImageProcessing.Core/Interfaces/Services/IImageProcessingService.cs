namespace ImageProcessing.Core.Interfaces.Services;

public interface IImageProcessingService
{
    byte[,] ProcessPixels(byte[,] pixels);
}