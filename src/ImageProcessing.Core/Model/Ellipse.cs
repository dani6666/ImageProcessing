using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Core.Model
{
    internal class Ellipse
    {
        public System.Drawing.Rectangle BoundingRect;
        public float Angle;
        public System.Drawing.Point Center;
        public Ellipse(System.Drawing.Rectangle boundingRect, float angle, System.Drawing.Point center)
        {
            BoundingRect = boundingRect;
            Angle = angle;
            Center = center;
        }
    }
}
