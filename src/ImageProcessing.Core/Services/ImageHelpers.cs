﻿using ImageProcessing.Core.Model;

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
        public static PixelRgb[,] Dilation(PixelRgb[,] pixels, int kernelSize=4)
        {
            int h = pixels.GetLength(0);
            int w = pixels.GetLength(1);

            PixelRgb[,] result = new PixelRgb[h, w];

            int padding = (kernelSize - 1) / 2;
            for (int r = padding; r < h - padding; r++)
            {
                for (int c = padding; c < w - padding; c++)
                {
                    for (int kernelR = -padding; kernelR <= padding; kernelR++)
                    {
                        for (int kernelC = -padding; kernelC <= padding; kernelC++)
                        {
                            var R = Math.Max((result[r + kernelR, c + kernelC]?.R ?? 0), pixels[r,c].R);
                            //var G = Math.Max((result[r + kernelR, c + kernelC]?.Green ?? 0), pixels[r, c].Green);
                            //var B = Math.Max((result[r + kernelR, c + kernelC]?.Blue ?? 0), pixels[r, c].Blue);

                            result[r + kernelR, c + kernelC] = new PixelRgb(R, R, R);
                        }
                    }
                }
            }
            return result;
        }

        /** 
         * removes elements smaller than kernel
        */

        public static PixelRgb[,] Erosion(PixelRgb[,] pixels, int kernelSize=4)
        {
            int h = pixels.GetLength(0);
            int w = pixels.GetLength(1);

            PixelRgb[,] result = new PixelRgb[h, w];
            int padding = (kernelSize - 1) / 2;
            for (int r = 0; r < h; r++)
            {
                for (int c = 0; c < w; c++)
                {
                    result[r, c] = new PixelRgb(0, 0, 0);
                }
            }

            for (int r = padding; r < h - padding; r++)
            {
                for (int c = padding; c < w - padding; c++)
                {
                    byte R = 255;
                    //byte G = 255;
                    //byte B = 255;
                    for (int kernelR = -padding; kernelR <= padding; kernelR++)
                    {
                        for (int kernelC = -padding; kernelC <= padding; kernelC++)
                        {
                            R = Math.Min(R, pixels[r + kernelR, c + kernelC].R) ;
                            //G = Math.Min(G, pixels[r + kernelR, c + kernelC].Green);
                            //B = Math.Min(B, pixels[r + kernelR, c + kernelC].Blue);

                        }
                    }
                    result[r, c].R = R;
                    result[r, c].G = R;
                    result[r, c].B = R;

                }
            }
            return result;
        }

        /** 
         * smoothing contours, breaks elements smaller than kernel
        */

        public static PixelRgb[,] MorphologicalOpening(PixelRgb[,] pixels, int kernelSize = 4)
        {
            return Dilation(Erosion(pixels, kernelSize), kernelSize);
        }
        /** 
         * fuses openings smaller than kernel
        */
        public static PixelRgb[,] MorphologicalClosing(PixelRgb[,] pixels, int kernelSize = 4)
        {
            return Erosion(Dilation(pixels, kernelSize), kernelSize);
        }

        public static bool[,] FindMask(PixelHsv[,] pixels, PixelHsv lower, PixelHsv upper)
        {
            var rows = pixels.GetLength(0);
            var cols = pixels.GetLength(1);
            var mask = new bool[rows, cols];
            for (var r = 0; r < rows; r++)
                for (var c = 0; c < cols; c++)
                    mask[r, c] = pixels[r, c].IsWithinBounds(lower, upper);
            return mask;
        }
    }
}