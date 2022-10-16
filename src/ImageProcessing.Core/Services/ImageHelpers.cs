using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Core.Services
{
    internal static class ImageHelpers
    {
        public static (byte R,byte G,byte B)[,] ConvertTo2d(byte[] inputArr, int width)
        {
            int height = inputArr.Length / (3 * width);

            (byte, byte, byte)[,] arr2d = new (byte,byte,byte)[height, width];
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width*3; c += 3)
                {
                    int baseIndex = r * width * 3 + c;
                    arr2d[r, c/3] = (inputArr[baseIndex + 2], inputArr[baseIndex + 1], inputArr[baseIndex]);
                }
            }
            return arr2d;
        }

        public static byte[] ConvertTo1d((byte R, byte G, byte B)[,] inputArr)
        {
            int rowsCount = inputArr.GetLength(0);
            int colsCount = inputArr.GetLength(1);
            byte[] arr1d = new byte[rowsCount*colsCount*3];
            var i = 0;
            for (int r = 0; r < rowsCount; r++)
            {
                for (int c = 0; c < colsCount; c++)
                {
                    arr1d[i++] = inputArr[r, c].B;
                    arr1d[i++] = inputArr[r, c].G;
                    arr1d[i++] = inputArr[r, c].R;

                }
            }
            return arr1d;
        }
    }
}
