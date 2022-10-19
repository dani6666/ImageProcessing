using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessing.Core.Model;

namespace ImageProcessing.Core.Services
{
    internal static class ImageHelpers
    {
        public static Pixel[,] ConvertTo2d(byte[] inputArr, int width)
        {
            int height = inputArr.Length / (3 * width);

            Pixel[,] arr2d = new Pixel[height, width];
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width*3; c += 3)
                {
                    int baseIndex = r * width * 3 + c;
                    arr2d[r, c/3] = new Pixel(inputArr[baseIndex + 2], inputArr[baseIndex + 1], inputArr[baseIndex]);
                }
            }
            return arr2d;
        }

        public static byte[] ConvertTo1d(Pixel[,] inputArr)
        {
            int rowsCount = inputArr.GetLength(0);
            int colsCount = inputArr.GetLength(1);
            byte[] arr1d = new byte[rowsCount*colsCount*3];
            var i = 0;
            for (int r = 0; r < rowsCount; r++)
            {
                for (int c = 0; c < colsCount; c++)
                {
                    arr1d[i++] = inputArr[r, c].Blue;
                    arr1d[i++] = inputArr[r, c].Green;
                    arr1d[i++] = inputArr[r, c].Red;

                }
            }
            return arr1d;
        }
    }
}
