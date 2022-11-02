using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Core.Model
{
    public class Rectangle
    {
        public Point TopPoint;
        public Point BottomPoint;
        public Point RightPoint;
        public Point LeftPoint;

        public Rectangle(Point topPoint, Point bottomPoint, Point rightPoint, Point leftPoint)
        {
            TopPoint = topPoint;
            BottomPoint = bottomPoint;
            RightPoint = rightPoint;
            LeftPoint = leftPoint;
        }

        public Line TopRightLine =>
            Line.FromPoints(TopPoint.Column, TopPoint.Row, RightPoint.Column, RightPoint.Row);

        public Line TopLeftLine =>
            Line.FromPoints(TopPoint.Column, TopPoint.Row, LeftPoint.Column, LeftPoint.Row);

        public Line BottomRightLine =>
            Line.FromPoints(BottomPoint.Column, BottomPoint.Row, RightPoint.Column, RightPoint.Row);

        public Line BottomLeftLine =>
            Line.FromPoints(BottomPoint.Column, BottomPoint.Row, LeftPoint.Column, LeftPoint.Row);

        public bool IsRectangle => Math.Abs(TopRightLine.PerpendicularityMeter(TopLeftLine) - 1) < SharedSettings.FloatComparisonTolerance &&
                                   Math.Abs(BottomRightLine.PerpendicularityMeter(BottomLeftLine) - 1) < SharedSettings.FloatComparisonTolerance &&
                                   Math.Abs(TopRightLine.PerpendicularityMeter(BottomRightLine) - 1) < SharedSettings.FloatComparisonTolerance &&
                                   Math.Abs(BottomLeftLine.PerpendicularityMeter(TopLeftLine) - 1) < SharedSettings.FloatComparisonTolerance;

        public double Area => Math.Sqrt(Math.Pow(TopPoint.Column - RightPoint.Column, 2) + Math.Pow(TopPoint.Row - RightPoint.Row, 2)) * 
                              Math.Sqrt(Math.Pow(TopPoint.Column - LeftPoint.Column, 2) + Math.Pow(TopPoint.Row - LeftPoint.Row, 2));
    }
}
