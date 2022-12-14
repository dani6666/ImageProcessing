using ImageProcessing.Core.Model;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;

namespace ImageProcessing.Core.Services
{
    internal static class ImageHelpers
    {
        public static PixelRgb[,] ConvertTo2d(byte[] inputArr, int width)
        {
            int height = inputArr.Length / (3 * width);

            PixelRgb[,] arr2d = new PixelRgb[height, width];
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width * 3; c += 3)
                {
                    int baseIndex = r * width * 3 + c;
                    arr2d[r, c / 3] = new PixelRgb(inputArr[baseIndex + 2], inputArr[baseIndex + 1], inputArr[baseIndex]);
                }
            }
            return arr2d;
        }

        public static byte[] ConvertTo1d(PixelRgb[,] inputArr)
        {

            byte[] arr1d = new byte[inputArr.Length * 3];
            var i = 0;
            foreach (var pixel in inputArr)
            {
                arr1d[i++] = pixel.B;
                arr1d[i++] = pixel.G;
                arr1d[i++] = pixel.R;
            }
            return arr1d;
        }
        /** 
         * increases small elements to kernel size
        */
        // pixel.IsMarked
        public static PixelHsv[,] Dilation(PixelHsv[,] pixels, int kernelSize=4)
        {
            int h = pixels.GetLength(0);
            int w = pixels.GetLength(1);

            PixelHsv[,] result = new PixelHsv[h, w];

            int padding = (kernelSize - 1) / 2;
            for (int r = padding; r < h - padding; r++)
            {
                for (int c = padding; c < w - padding; c++)
                {
                    for (int kernelR = -padding; kernelR <= padding; kernelR++)
                    {
                        for (int kernelC = -padding; kernelC <= padding; kernelC++)
                        {
                            var marked =  pixels[r,c].IsMarked;

                            result[r + kernelR, c + kernelC] = new PixelHsv(pixels[r + kernelR, c + kernelC].H, pixels[r + kernelR, c + kernelC].S, pixels[r + kernelR, c + kernelC].V);
                            if (marked)
                                result[r + kernelR, c + kernelC].IsMarked = marked;
                            
                            else
                                result[r + kernelR, c + kernelC].IsMarked = pixels[r + kernelR, c + kernelC].IsMarked;

                        }
                    }
                }
            }
            return result;
        }

        /** 
         * removes elements smaller than kernel
        */

        public static PixelHsv[,] Erosion(PixelHsv[,] pixels, int kernelSize=4)
        {
            int h = pixels.GetLength(0);
            int w = pixels.GetLength(1);

            PixelHsv[,] result = new PixelHsv[h, w];
            int padding = (kernelSize - 1) / 2;
            for (int r = 0; r < h; r++)
            {
                for (int c = 0; c < w; c++)
                {
                    result[r, c] = new PixelHsv(0, 0, 0);
                }
            }

            for (int r = padding; r < h - padding; r++)
            {
                for (int c = padding; c < w - padding; c++)
                {
                    var marked = true;
                    for (int kernelR = -padding; kernelR <= padding; kernelR++)
                    {
                        for (int kernelC = -padding; kernelC <= padding; kernelC++)
                        {
                            marked = marked && pixels[r + kernelR, c + kernelC].IsMarked;
                            if (!marked) break;

                        }
                        if (!marked) break;
                    }
                    result[r, c] = new PixelHsv(pixels[r, c].H, pixels[r, c].S, pixels[r, c].V);
                    result[r, c].IsMarked = marked;

                }
            }
            return result;
        }

        /** 
         * smoothing contours, breaks elements smaller than kernel
        */

        public static PixelHsv[,] MorphologicalOpening(PixelHsv[,] pixels, int kernelSize = 4)
        {
            return Dilation(Erosion(pixels, kernelSize), kernelSize);
        }
        /** 
         * fuses openings smaller than kernel
        */
        public static PixelHsv[,] MorphologicalClosing(PixelHsv[,] pixels, int kernelSize = 4)
        {
            return Erosion(Dilation(pixels, kernelSize), kernelSize);
        }

        public static bool[,] FindMask(PixelHsv[,] pixels, PixelHsv lower, PixelHsv upper)
        {
            return pixels.Transform(hsv => hsv.IsWithinBounds(lower, upper));
        }

        public static PixelHsv[,] Cover(this PixelHsv[,] pixels, PixelHsv lower, PixelHsv upper, PixelHsv coverColor)
        {
            var rows = pixels.GetLength(0);
            var cols = pixels.GetLength(1);
            for (var r = 0; r < rows; r++)
                for (var c = 0; c < cols; c++)
                    if (pixels[r, c].IsWithinBounds(lower, upper))
                        pixels[r, c].H = coverColor.H;
            return pixels;
        }

        public static Dictionary<(PixelHsv, PixelHsv), int> CreateHistogram(this PixelHsv[,] pixels, List<(PixelHsv, PixelHsv)> buckets)
        {
            var rows = pixels.GetLength(0);
            var cols = pixels.GetLength(1);
            var histogram = new Dictionary<(PixelHsv, PixelHsv), int>();
            foreach(var bucket in buckets)
                histogram[bucket] = 0;
            
            for (var r = 0; r < rows; r++)
                for (var c = 0; c < cols; c++)
                {
                    foreach (var range in buckets)
                    {
                        var pixel = pixels[r, c];
                        if (pixels[r, c].IsWithinBounds(range.Item1, range.Item2))
                        {
                            histogram[range] +=1;
                            break;
                        }
                    }
                }
            return histogram;
        }
    }
}
