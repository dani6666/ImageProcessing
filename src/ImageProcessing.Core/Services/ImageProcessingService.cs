using ImageProcessing.Core.Interfaces.Services;
using ImageProcessing.Core.Model;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Point = ImageProcessing.Core.Model.Point;
using Rectangle = ImageProcessing.Core.Model.Rectangle;

namespace ImageProcessing.Core.Services;

public class ImageProcessingService : IImageProcessingService
{
    
    private static List<List<Point>> DetectObjects(PixelHsv[,] pixels, Predicate<PixelHsv> condition)
    {
        var result = new List<List<Point>>();
        var rowsCount = pixels.GetLength(0);
        var colsCount = pixels.GetLength(1);
        for (int r = 0; r < rowsCount; r++)
        {
            for (int c = 0; c < colsCount; c++)
            {
                if (condition(pixels[r, c]) && !result.Any(o => o.BinarySearch(new Point(r, c)) >= 0))
                {
                    result.Add(DetectObject(pixels, condition, r, c));
                }
            }
        }

        return result;
    }

    private static List<Point> DetectObject(PixelHsv[,] pixels, Predicate<PixelHsv> condition, int row, int column) =>
        DetectObject(pixels, condition, row, column, new List<Point>());

    private static List<Point> DetectObject(PixelHsv[,] pixels, Predicate<PixelHsv> condition, int startingRow, int startingColumn, List<Point> result)
    {
        var rowsCount = pixels.GetLength(0);
        var columnsCount = pixels.GetLength(1);

        var coordinatesToCheck = new Queue<Point>();
        coordinatesToCheck.Enqueue(new Point(startingRow, startingColumn));

        do
        {
            var (row, column) = coordinatesToCheck.Dequeue();
            var index = result.BinarySearch(new Point(row, column));

            if (index >= 0 || !condition(pixels[row, column]))
                continue;

            result.Insert(~index, new Point(row, column));

            if (row + 1 < rowsCount - 1)
                coordinatesToCheck.Enqueue(new Point(row + 1, column));

            if (row - 1 >= 0)
                coordinatesToCheck.Enqueue(new Point(row - 1, column));

            if (column + 1 < columnsCount - 1)
                coordinatesToCheck.Enqueue(new Point(row, column + 1));

            if (column - 1 >= 0)
                coordinatesToCheck.Enqueue(new Point(row, column - 1));

        } while (coordinatesToCheck.Any());


        return result;
    }

    private static Rectangle? GetInsideRectangle(List<Point> obj)
    {
        var potentialRect = new Rectangle(obj.MaxBy(p => p.Row), obj.MinBy(p => p.Row), obj.MaxBy(p => p.Column), obj.MinBy(p => p.Column));

        while (!potentialRect.IsRectangle)
        {
            while (potentialRect.TopRightLine.PerpendicularityMeter(potentialRect.TopLeftLine) > 1 || obj.BinarySearch(potentialRect.TopPoint) < 0)
            {
                potentialRect.TopPoint.Row--;

                if (potentialRect.TopPoint.Row < potentialRect.BottomPoint.Row)
                    return null;
            }

            while (potentialRect.BottomRightLine.PerpendicularityMeter(potentialRect.BottomLeftLine) > 1 || obj.BinarySearch(potentialRect.BottomPoint) < 0)
            {
                potentialRect.BottomPoint.Row++;

                if (potentialRect.TopPoint.Row < potentialRect.BottomPoint.Row)
                    return null;
            }

            while (potentialRect.TopRightLine.PerpendicularityMeter(potentialRect.BottomRightLine) < 1 || obj.BinarySearch(potentialRect.RightPoint) < 0)
            {
                potentialRect.RightPoint.Column--;

                if (potentialRect.RightPoint.Column < potentialRect.LeftPoint.Column)
                    return null;
            }

            while (potentialRect.BottomLeftLine.PerpendicularityMeter(potentialRect.TopLeftLine) < 1 || obj.BinarySearch(potentialRect.LeftPoint) < 0)
            {
                potentialRect.LeftPoint.Column++;

                if (potentialRect.RightPoint.Column < potentialRect.LeftPoint.Column)
                    return null;
            }
        }

        return potentialRect;
    }

    private Ellipse GetBoundingEllipse(List<Point> obj)
    {
        var wrappingRect = new Rectangle(obj.MaxBy(p => p.Row), obj.MinBy(p => p.Row), obj.MaxBy(p => p.Column), obj.MinBy(p => p.Column));

        var horizontalDiagonalLength = wrappingRect.HorizontalDiagonalLength;
        var verticalDiagonalLength = wrappingRect.VerticalDiagonalLength;
        var horizontalDiagonal = wrappingRect.HorizontalDiagonal;
        var verticalDiagonal = wrappingRect.VerticalDiagonal;
        // src: https://www.topcoder.com/thrive/articles/Geometry%20Concepts%20part%202:%20%20Line%20Intersection%20and%20its%20Applications
        //var det = horizontalDiagonal.Gradient - verticalDiagonal.Gradient;
        //int centerX = (int)((int) (verticalDiagonal.Intercept - horizontalDiagonal.Intercept) / det);
        //int centerY = (int) ((int) -(-horizontalDiagonal.Gradient * verticalDiagonal.Intercept + verticalDiagonal.Gradient * horizontalDiagonal.Intercept) / det);

        Size size = new Size(horizontalDiagonalLength, ???);
        float angle = -(float)(Math.Atan2(wrappingRect.TopPoint.Row - wrappingRect.BottomPoint.Row, wrappingRect.BottomPoint.Column - wrappingRect.TopPoint.Column) * 180f / Math.PI);
        System.Drawing.Point center = new System.Drawing.Point((wrappingRect.TopPoint.Column + wrappingRect.BottomPoint.Column) / 2, (wrappingRect.TopPoint.Row + wrappingRect.BottomPoint.Row) / 2);
        //System.Drawing.Point center = new System.Drawing.Point(centerX, centerY);

        int h2 = size.Height / 2;
        int w2 = size.Width / 2;

        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(new System.Drawing.Point(center.X - w2, center.Y - h2), size);

        return new Ellipse(rect, angle, center);
    }

    private Ellipse GetBoundingCircle (List<Point> obj)
    {

        var points = new Point[] { obj.MaxBy(p => p.Row), obj.MinBy(p => p.Row), obj.MaxBy(p => p.Column), obj.MinBy(p => p.Column) };
        var diameter = 0;
        var p1 = points[0];
        var p2 = points[1];
        for(int i=0; i < points.Length; i++)
        {
            for(int j=i; j < points.Length; j++)
            {
                var dist = Point.CalculateDistance(points[i], points[j]);
                if (dist > diameter)
                {
                    diameter = dist;
                    p1 = points[i];
                    p2 = points[j];
                }
            }
        }
        Size size = new Size(diameter, diameter);
        System.Drawing.Point center = new System.Drawing.Point((p1.Column + p2.Column) / 2, (p1.Row + p2.Row) / 2);

        int h2 = size.Height / 2;
        int w2 = size.Width / 2;

        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(new System.Drawing.Point(center.X - w2, center.Y - h2), size);
        return new Ellipse(rect, 0f, center);
    }

    //private static IEnumerable<Rectangle> FindRectangles(PixelRgb[,] pixels)
    //{
    //    int rowsCount = pixels.GetLength(0);
    //    int colsCount = pixels.GetLength(1);
    //    for (int r = 0; r < rowsCount; r++)
    //    {
    //        var wasLastPixelRed = false;
    //        for (int c = 0; c < colsCount; c++)
    //        {
    //            if (pixels[r, c].IsRed())
    //            {
    //                if (!wasLastPixelRed)
    //                {
    //                    var rect = CheckForRectangle(pixels, r, c);
    //                    if (rect.HasValue)
    //                        yield return rect.Value;
    //                    wasLastPixelRed = true;
    //                }
    //            }
    //            else
    //                wasLastPixelRed = false;
    //        }
    //    }
    //}

    //private static Rectangle? CheckForRectangle(PixelRgb[,] pixels, int r, int c)
    //{
    //    var rLast = pixels.GetLength(0) - 1;
    //    var cLast = pixels.GetLength(1) - 1;

    //    // Searching for end of red lines
    //    // endR, endC mark last red pixel
    //    var endR = r;
    //    while (endR + 1 <= rLast && pixels[endR + 1, c].IsRed())
    //        endR++;

    //    if (endR == r)
    //        return null;

    //    var endC = c;
    //    while (endC + 1 <= cLast && pixels[r, endC + 1].IsRed())
    //        endC++;

    //    if (endC == c)
    //        return null;

    //    // Checking that there are on red pixels touching rectangle (then it is not a rectangle)
    //    // Math.Min/Max make sure not exceed array if rect touches image border
    //    for (int i = Math.Max(r - 1, 0); i <= Math.Min(endR + 1, rLast); i++)
    //    {
    //        if ((c > 0 && pixels[i, c - 1].IsRed()) || (endC < cLast && pixels[i, endC + 1].IsRed()))
    //            return null;
    //    }

    //    for (int i = Math.Max(c - 1, 0); i <= Math.Min(endC + 1, cLast); i++)
    //    {
    //        if ((r > 0 && pixels[r - 1, i].IsRed()) || (endR < rLast && pixels[endR + 1, i].IsRed()))
    //            return null;
    //    }


    //    // Checking rectangle fill
    //    for (int i = r; i <= endR; i++)
    //    {
    //        for (int j = c; j <= endC; j++)
    //        {
    //            if (!pixels[i, j].IsRed())
    //                return null;
    //        }
    //    }
    //    // 2d arrays are indexed [rowI, colI],
    //    // but geometrically we pass coordinates as (x,y),
    //    // where x - horizontal dimension (col), y - vertical dimension (x)
    //    return new Rectangle(c, r, endC - c, endR - r);
    //}

    // public void ProcessPixels(Bitmap bitmap)
    // {
    //     IEnumerable<Rectangle> rectangles;
    //     using (var image = new BitmapLockAdapter(bitmap))
    //     {
    //         var matrix = image.ReadPixels();
    //         rectangles = FindRectangles(matrix).ToList();
    //     }
    //     foreach (var rectangle in rectangles)
    //         DrawRectangle(bitmap, rectangle, Color.Black, false);
    // }

    public void ProcessPixels(Bitmap bitmap)
    {
        var ellipses = new List<Ellipse>();

        using (var image = new BitmapLockAdapter(bitmap))
        {
            var hsv = image.ReadPixels().AsHsv();

            var objects = DetectObjects(hsv,
                p => p.IsWithinBounds(new PixelHsv(330, 0.3f, 0.3f), new PixelHsv(360, 1, 1)) ||
                     p.IsWithinBounds(new PixelHsv(0, 0.3f, 0.3f), new PixelHsv(30, 1, 1)));

            foreach (var obj in objects)
            {
                var rect = GetInsideRectangle(obj);

                if (rect != null && rect.Area > obj.Count * 0.9f)
                {
                    foreach (var (r, c) in obj)
                    {
                        hsv[r, c].H = 110;
                    }
                }
                var ellipse = GetBoundingCircle(obj);
                ellipses.Add(ellipse);
            }
            image.WritePixels(hsv
                //.Cover(new PixelHsv(330, 0.3f, 0.3f), new PixelHsv(360, 1, 1), new PixelHsv(110, 0.95f, 0.95f))
                //.Cover(new PixelHsv(0, 0.3f, 0.3f), new PixelHsv(30, 1, 1), new PixelHsv(110, 0.95f, 0.95f))
                .AsRgb()
            );
        }

        foreach(var e in ellipses)
        {
            using var pen = new Pen(Color.Blue, 2);
            using var graphics = Graphics.FromImage(bitmap);
            //graphics.TranslateTransform(e.Center.X, e.Center.Y);
            //graphics.RotateTransform(e.Angle);
            //graphics.TranslateTransform(-e.Center.X, -e.Center.Y);
            //graphics.DrawRectangle(pen, e.BoundingRect);
            graphics.DrawEllipse(pen, e.BoundingRect);
            //graphics.ResetTransform();
        }
    }

    //public void DrawRectangle(Bitmap bitmap, Rectangle rectangle, Color color, bool fill)
    //{
    //    using var graphics = Graphics.FromImage(bitmap);
    //    if (fill)
    //    {
    //        using var brush = new SolidBrush(color);
    //        graphics.FillRectangle(brush, rectangle);
    //    }
    //    else
    //    {
    //        using var pen = new Pen(color, 4);
    //        graphics.DrawRectangle(pen, rectangle);
    //    }
    //}

    public void RemoveNoise(Bitmap bitmap)
    {
        using var image = new BitmapLockAdapter(bitmap);
        var matrix = image.ReadPixels();

        image.WritePixels(
            ImageHelpers.MorphologicalClosing(
                ImageHelpers.MorphologicalOpening(matrix)
            )
        );

    }
}
