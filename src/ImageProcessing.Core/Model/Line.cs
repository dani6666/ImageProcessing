using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Core.Model
{
    public class Line
    {
        public float Gradient { get; set; }
        public float Intercept { get; set; }

        public Line(float gradient, float intercept)
        {
            Gradient = gradient;
            Intercept = intercept;
        }

        public static Line FromPoints(int firstX, int firstY, int secondX, int secondY)
        {
            var gradient = firstX == secondX ?
                float.MaxValue :
                (firstY - secondY) / (firstX - secondX);

            return new Line(gradient, firstY - gradient * firstX);
        }

        public float PerpendicularityMeter(Line secondLine) =>
            -Gradient * secondLine.Gradient;
    }
}
