using ImageProcessing.Core.Interfaces.Services;
using ImageProcessing.Core.Model;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using Point = ImageProcessing.Core.Model.Point;
using Rectangle = ImageProcessing.Core.Model.Rectangle;

namespace ImageProcessing.Core.Services;

public class ImageProcessingService : IImageProcessingService
{

    private static List<List<Point>> DetectObjects(PixelHsv[,] pixels)
    {
        var result = new List<List<Point>>();
        var rowsCount = pixels.GetLength(0);
        var colsCount = pixels.GetLength(1);
        for (int r = 0; r < rowsCount; r++)
        {
            for (int c = 0; c < colsCount; c++)
            {
                if (pixels[r, c].IsMarked && !result.Any(o => o.BinarySearch(new Point(r, c)) >= 0))
                {
                    result.Add(DetectObject(pixels, r, c));
                }
            }
        }

        return result;
    }

    private static List<Point> DetectObject(PixelHsv[,] pixels, int row, int column) =>
        DetectObject(pixels, row, column, new List<Point>());

    private (Point extremeP1, Point extremeP2, int distance) GetExtremePoints(List<Point> obj, PixelHsv[,] pixels, Predicate<PixelHsv> condition)
    {
        Point extremeP1 = new Point();
        Point extremeP2 = new Point();
        var longestDistance = 0;
        var boundingPoints = new List<Point>();
        for (int i = 0; i < obj.Count; i++)
        {
            var p = obj[i];
            if (p.Row != 0 && p.Row != pixels.GetLength(0) - 1 && p.Column != 0 && p.Column != pixels.GetLength(1) - 1 &&
                condition(pixels[p.Row, p.Column - 1]) &&
                condition(pixels[p.Row, p.Column + 1]) &&
                condition(pixels[p.Row - 1, p.Column]) &&
                condition(pixels[p.Row + 1, p.Column]))
                continue;
            boundingPoints.Add(p);
        }

        for (int i = 0; i < boundingPoints.Count; i++)
        {
            var p1 = boundingPoints[i];


            for (int j = i; j < boundingPoints.Count; j++)
            {
                var p2 = boundingPoints[j];
                var dist = Point.CalculateDistance(p1, p2);
                if (dist > longestDistance)
                {
                    extremeP1 = p1;
                    extremeP2 = p2;
                    longestDistance = dist;
                }
            }
        }

        return (extremeP1, extremeP2, longestDistance);
    }
    private static List<Point> DetectObject(PixelHsv[,] pixels, int startingRow, int startingColumn, List<Point> result)
    {
        var rowsCount = pixels.GetLength(0);
        var columnsCount = pixels.GetLength(1);

        var coordinatesToCheck = new Queue<Point>();
        coordinatesToCheck.Enqueue(new Point(startingRow, startingColumn));

        var mergingRange = Math.Max(2, pixels.GetLength(0) / 200);

        do
        {
            var point = coordinatesToCheck.Dequeue();
            var index = result.BinarySearch(point);

            if (index >= 0)
                continue;

            result.Insert(~index, point);

            var (row, column) = point;

            for (int i = -mergingRange; i <= mergingRange; i++)
            {
                for (int j = -mergingRange; j <= mergingRange; j++)
                {
                    if (i == 0 && j == 0) continue;

                    var checkedRow = row + i;
                    var checkedColumn = column + j;

                    if (checkedRow < rowsCount - 1 && checkedRow >= 0 &&
                        checkedColumn < columnsCount - 1 && checkedColumn >= 0 &&
                        pixels[checkedRow, checkedColumn].IsMarked)
                    {
                        var tempI = i;
                        var tempJ = j;
                        while (tempI != 0 || tempJ != 0)
                        {
                            if (tempI > 0)
                                tempI--;
                            else if (tempI < 0)
                                tempI++;

                            if (tempJ > 0)
                                tempJ--;
                            else if (tempJ < 0)
                                tempJ++;

                            pixels[row + tempI, column + tempJ].IsMarked = true;
                            var p = new Point(row + tempI, column + tempJ);

                            var pIndex = result.BinarySearch(p);
                            if (pIndex >= 0)
                                continue;

                            result.Insert(~pIndex, p);
                        }
                        coordinatesToCheck.Enqueue(new Point(checkedRow, checkedColumn));
                    }
                }
            }

            //if (row + 1 < rowsCount - 1)
            //    coordinatesToCheck.Enqueue(new Point(row + 1, column));

            //if (row - 1 >= 0)
            //    coordinatesToCheck.Enqueue(new Point(row - 1, column));

            //if (column + 1 < columnsCount - 1)
            //    coordinatesToCheck.Enqueue(new Point(row, column + 1));

            //if (column - 1 >= 0)
            //    coordinatesToCheck.Enqueue(new Point(row, column - 1));

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

    private static Triangle? FindTriangle(List<Point> obj)
    {
        var vertices = SimpleVertices(obj);
        return vertices.Count == 3
            ? new Triangle(vertices[0], vertices[1], vertices[2])
            : null;
    }

    /**
     *  Search for vertices based on list of points
     */
    private static List<Point> SimpleVertices(List<Point> obj)
    {
        var boundRMin = obj.MinBy(p => p.Row).Row;
        var boundRMax = obj.MaxBy(p => p.Row).Row;
        var boundCMin = obj.MinBy(p => p.Column).Column;
        var boundCMax = obj.MaxBy(p => p.Column).Column;

        var points = Array.Empty<Point>()
            .Concat(FindMostDifferentPoints(obj.FindAll(p => p.Row == boundRMin)))
            .Concat(FindMostDifferentPoints(obj.FindAll(p => p.Row == boundRMax)))
            .Concat(FindMostDifferentPoints(obj.FindAll(p => p.Column == boundCMin)))
            .Concat(FindMostDifferentPoints(obj.FindAll(p => p.Column == boundCMax)))
            .Distinct()
            .ToList();

        var mergeDistance = 30;
        while (points.Pairwise().Any(t => t.Item3 < mergeDistance))
        {
            var pairsToMerge = points.Pairwise().Where(t => t.Item3 < mergeDistance).ToHashSet();
            var merged = pairsToMerge
                .Select(t => new Point((t.Item1.Row + t.Item2.Row) / 2, (t.Item1.Column + t.Item2.Column) / 2))
                .ToHashSet();
            var removed = pairsToMerge.Select(t => t.Item1)
                .Concat(pairsToMerge.Select(t => t.Item2))
                .ToHashSet();

            points = points.Where(p => !removed.Contains(p)).Concat(merged).ToList();
        }

        return points;
    }

    /** Find the most different two points from a list of points */
    private static IEnumerable<Point> FindMostDifferentPoints(List<Point> points)
    {
        var (p1, p2, _) = points.Pairwise().MaxBy(p => p.Item3);
        return new[] { p1, p2 };
    }
    //private Ellipse GetBoundingEllipse(List<Point> obj)
    //{
    //    var wrappingRect = new Rectangle(obj.MaxBy(p => p.Row), obj.MinBy(p => p.Row), obj.MaxBy(p => p.Column), obj.MinBy(p => p.Column));

    //    var horizontalDiagonalLength = wrappingRect.HorizontalDiagonalLength;
    //    var verticalDiagonalLength = wrappingRect.VerticalDiagonalLength;
    //    var horizontalDiagonal = wrappingRect.HorizontalDiagonal;
    //    var verticalDiagonal = wrappingRect.VerticalDiagonal;
    //    // src: https://www.topcoder.com/thrive/articles/Geometry%20Concepts%20part%202:%20%20Line%20Intersection%20and%20its%20Applications
    //    //var det = horizontalDiagonal.Gradient - verticalDiagonal.Gradient;
    //    //int centerX = (int)((int) (verticalDiagonal.Intercept - horizontalDiagonal.Intercept) / det);
    //    //int centerY = (int) ((int) -(-horizontalDiagonal.Gradient * verticalDiagonal.Intercept + verticalDiagonal.Gradient * horizontalDiagonal.Intercept) / det);

    //    Size size = new Size(horizontalDiagonalLength, ???);
    //    float angle = -(float)(Math.Atan2(wrappingRect.TopPoint.Row - wrappingRect.BottomPoint.Row, wrappingRect.BottomPoint.Column - wrappingRect.TopPoint.Column) * 180f / Math.PI);
    //    System.Drawing.Point center = new System.Drawing.Point((wrappingRect.TopPoint.Column + wrappingRect.BottomPoint.Column) / 2, (wrappingRect.TopPoint.Row + wrappingRect.BottomPoint.Row) / 2);
    //    //System.Drawing.Point center = new System.Drawing.Point(centerX, centerY);

    //    int h2 = size.Height / 2;
    //    int w2 = size.Width / 2;

    //    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(new System.Drawing.Point(center.X - w2, center.Y - h2), size);

    //    return new Ellipse(rect, angle, center);
    //}

    private bool GetBoundingCircle(List<Point> obj, PixelHsv[,] pixels, Predicate<PixelHsv> condition)
    {
        var (center, radius) = FindBoundingCircle(obj, pixels, condition);
        var (p1, p2, diameter) = GetExtremePoints(obj, pixels, condition);
        DrawPoint(pixels, center, 150);

        var line = Line.FromPoints(p1.Row, p1.Column, p2.Row, p2.Column);

        var smallerRadius = radius;
        while (radius * smallerRadius * Math.PI * 0.90 > obj.Count)
        {
            smallerRadius--;
        }

        if (smallerRadius == 0) return false;

        if (p2.Row > p1.Row)
            (p1, p2) = (p2, p1);

        float angle = (float)(/*Math.Abs(*/Math.Atan2(p1.Row - p2.Row, p1.Column - p2.Column))/*)*/;

        if (angle > Math.PI / 2)
            angle -= (float)Math.PI / 2;
        else
            angle += (float)Math.PI / 2;

        var isElipse = true;
        var count = obj.Count(p => IsInElipse(p, center, radius, smallerRadius, angle));
        if (count < obj.Count * 0.90)
            return false;

        var found = obj.Where(p => IsInElipse(p, center, radius, smallerRadius, angle));

        foreach (var (r, c) in found)
        {
            pixels[r, c].H = 110;
        }
        Size size = new Size(radius * 2, smallerRadius * 2);
        System.Drawing.Point drawingCenter = new System.Drawing.Point(center.Column, center.Row);
        //new System.Drawing.Point((p1.Column + p2.Column) / 2, (p1.Row + p2.Row) / 2);

        //int h2 = size.Height;
        //int w2 = size.Width;

        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(new System.Drawing.Point(drawingCenter.X - size.Width / 2, drawingCenter.Y - size.Height / 2), size);
        return true;
    }

    private double EllipseRate(List<Point> obj, PixelHsv[,] pixels, Predicate<PixelHsv> condition)
    {
        var (center, radius) = FindBoundingCircle(obj, pixels, condition);
        var (p1, p2, _) = GetExtremePoints(obj, pixels, condition);

        var smallerRadius = radius;
        while (radius * smallerRadius * Math.PI * 0.90 > obj.Count)
            smallerRadius--;

        if (smallerRadius == 0)
            return 0;

        if (p2.Row > p1.Row)
            (p1, p2) = (p2, p1);

        var angle = (float)(/*Math.Abs(*/Math.Atan2(p1.Row - p2.Row, p1.Column - p2.Column))/*)*/;

        if (angle > Math.PI / 2)
            angle -= (float)Math.PI / 2;
        else
            angle += (float)Math.PI / 2;

        var count = obj.Count(p => IsInElipse(p, center, radius, smallerRadius, angle));
        return (double)count / obj.Count;
    }

    private bool IsInElipse(Point givenPoint, Point elipseCenter, int biggerRadius, int smallerRadius, float angle)
    {
        //angle +=90;
        if (Math.Abs(angle) < 0.2f)
            return Math.Pow(givenPoint.Row - elipseCenter.Row, 2) / (biggerRadius * biggerRadius) +
                Math.Pow(givenPoint.Column - elipseCenter.Column, 2) / (smallerRadius * smallerRadius) <= 1;

        return Math.Pow(Math.Cos(angle) * (givenPoint.Column - elipseCenter.Column) + Math.Sin(angle) * (givenPoint.Row - elipseCenter.Row), 2) / (smallerRadius * smallerRadius)
            + Math.Pow(Math.Sin(angle) * (givenPoint.Column - elipseCenter.Column) - Math.Cos(angle) * (givenPoint.Row - elipseCenter.Row), 2) / (biggerRadius * biggerRadius) <= 1;
    }

    private (Point center, int diameter) FindBoundingCircle(List<Point> obj, PixelHsv[,] pixels, Predicate<PixelHsv> condition)
    {
        var avgColumn = (int)obj.Average(p => p.Column);
        var avgRow = (int)obj.Average(p => p.Row);

        var center = new Point(avgRow, avgColumn);

        var maxDistance = -1;

        foreach (var p in obj)
        {
            if (p.Row != 0 && p.Row != pixels.GetLength(0) - 1 && p.Column != 0 && p.Column != pixels.GetLength(1) - 1 &&
                condition(pixels[p.Row, p.Column - 1]) &&
                condition(pixels[p.Row, p.Column + 1]) &&
                condition(pixels[p.Row - 1, p.Column]) &&
                condition(pixels[p.Row + 1, p.Column]))
                continue;

            var dist = Point.CalculateDistance(center, p);
            if (dist > maxDistance)
            {
                maxDistance = dist;
            }
        }

        return (center, maxDistance);
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
    public Dictionary<(PixelHsv, PixelHsv), int> GetColorStats(Bitmap bitmap, int numOfBuckets)
    {
        using var image = new BitmapLockAdapter(bitmap);
        var hsv = image.ReadPixels().AsHsv();
        List<(PixelHsv, PixelHsv)> buckets = new List<(PixelHsv, PixelHsv)>(numOfBuckets);
        var bucketRange = 360 / numOfBuckets;
        for (int i = 0; i < numOfBuckets; i++)
        {
            var lowerBound = i * bucketRange;
            var upperBound = i == numOfBuckets - 1
                ? (i + 1) * bucketRange
                : (i + 1) * bucketRange - 1;

            buckets.Add((new PixelHsv(lowerBound, 0, 0), new PixelHsv(upperBound, 1, 1)));
        }
        return ImageHelpers.CreateHistogram(hsv, buckets);
    }

    public void FindRectangles(Bitmap bitmap)
    {
        using var image = new BitmapLockAdapter(bitmap);

        var hsv = image.ReadPixels().AsHsv();
        Predicate<PixelHsv> predicate = p => p.IsWithinBounds(new PixelHsv(330, 0.3f, 0.3f), new PixelHsv(360, 1, 1)) ||
                                            p.IsWithinBounds(new PixelHsv(0, 0.3f, 0.3f), new PixelHsv(30, 1, 1));
        MarkPixels(hsv, predicate);

        var objects = DetectObjects(hsv);

        foreach (var obj in objects)
        {
            // var rect = GetInsideRectangle(obj);
            // if (rect != null && rect.Area > obj.Count * 0.85f)
            //     foreach (var (r, c) in obj)
            //     {
            //         hsv[r, c].H = 110;
            //     }

            // Triangles
            var rect = GetInsideRectangle(obj);
            if (rect != null && rect.Area > obj.Count * 0.9f)
            {
                foreach (var (r, c) in obj)
                {
                    hsv[r, c].H = 110;
                }
            }
        }
        image.WritePixels(hsv
            //     //.Cover(new PixelHsv(330, 0.3f, 0.3f), new PixelHsv(360, 1, 1), new PixelHsv(110, 0.95f, 0.95f))
            //     //.Cover(new PixelHsv(0, 0.3f, 0.3f), new PixelHsv(30, 1, 1), new PixelHsv(110, 0.95f, 0.95f))
            .AsRgb()
         );
    }

    public void FindTriangles(Bitmap bitmap)
    {
        using var image = new BitmapLockAdapter(bitmap);

        var hsv = image.ReadPixels().AsHsv();
        Predicate<PixelHsv> predicate = p => p.IsWithinBounds(new PixelHsv(330, 0.3f, 0.3f), new PixelHsv(360, 1, 1)) ||
                                             p.IsWithinBounds(new PixelHsv(0, 0.3f, 0.3f), new PixelHsv(30, 1, 1));
        MarkPixels(hsv, predicate);

        var objects = DetectObjects(hsv);

        foreach (var obj in objects)
        {
            // var rect = GetInsideRectangle(obj);
            // if (rect != null && rect.Area > obj.Count * 0.85f)
            //     foreach (var (r, c) in obj)
            //     {
            //         hsv[r, c].H = 110;
            //     }

            // Triangles
            var triangle = FindTriangle(obj);
            if (triangle == null)
                continue;
            DrawPoint(hsv, new Point(triangle.A.Row, triangle.A.Column));
            DrawPoint(hsv, new Point(triangle.B.Row, triangle.B.Column));
            DrawPoint(hsv, new Point(triangle.C.Row, triangle.C.Column));
            foreach (var (r, c) in obj)
            {
                hsv[r, c].H = 110;
            }
            //if (triangle != null)
            //{
            //    using var pen = new Pen(Color.Black, 20);
            //    using var graphics = Graphics.FromImage(bitmap);
            //    graphics.DrawLine(pen, triangle.A.Row, triangle.A.Column, triangle.B.Row, triangle.B.Column);
            //    graphics.DrawLine(pen, triangle.A.Row, triangle.A.Column, triangle.C.Row, triangle.C.Column);
            //    graphics.DrawLine(pen, triangle.C.Row, triangle.C.Column, triangle.B.Row, triangle.B.Column);
            //}
        }
        image.WritePixels(hsv
            //     //.Cover(new PixelHsv(330, 0.3f, 0.3f), new PixelHsv(360, 1, 1), new PixelHsv(110, 0.95f, 0.95f))
            //     //.Cover(new PixelHsv(0, 0.3f, 0.3f), new PixelHsv(30, 1, 1), new PixelHsv(110, 0.95f, 0.95f))
            .AsRgb()
         );
    }

    void DrawPoint(PixelHsv[,] pixels, Point point, int hue = 150)
    {
        var black = new PixelHsv(hue, 0, 0);
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                pixels[Math.Min(point.Row + i, pixels.GetLength(0)-1), Math.Min(point.Column + j, pixels.GetLength(1)-1)] = black;
            }
        }
    }

    public void ShowBoundingCircles(Bitmap bitmap)
    {
        var ellipses = new List<Ellipse>();

        using (var image = new BitmapLockAdapter(bitmap))
        {
            var hsv = image.ReadPixels().AsHsv();
            Predicate<PixelHsv> predicate = p => p.IsWithinBounds(new PixelHsv(330, 0.3f, 0.3f), new PixelHsv(360, 1, 1)) ||
                     p.IsWithinBounds(new PixelHsv(0, 0.3f, 0.3f), new PixelHsv(30, 1, 1));
            MarkPixels(hsv, predicate);

            var objects = DetectObjects(hsv);

            foreach (var obj in objects)
            {
                var isElipse = GetBoundingCircle(obj, hsv, predicate);
                //if(ellipse != null)
                //ellipses.Add(ellipse);

                if (isElipse)
                    foreach (var (r, c) in obj)
                    {
                        hsv[r, c].H = 110;
                        hsv[r, c].S = 1;
                        hsv[r, c].V = 1;
                }
            }
            image.WritePixels(hsv
                //.Cover(new PixelHsv(330, 0.3f, 0.3f), new PixelHsv(360, 1, 1), new PixelHsv(110, 0.95f, 0.95f))
                //.Cover(new PixelHsv(0, 0.3f, 0.3f), new PixelHsv(30, 1, 1), new PixelHsv(110, 0.95f, 0.95f))
                .AsRgb()
            );
        }


        foreach (var e in ellipses)
        {
            using var pen = new Pen(Color.Blue, 2);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.TranslateTransform(e.Center.X, e.Center.Y);
            graphics.RotateTransform(e.Angle);
            graphics.TranslateTransform(-e.Center.X, -e.Center.Y);
            //graphics.DrawRectangle(pen, e.BoundingRect);
            graphics.DrawEllipse(pen, e.BoundingRect);
            graphics.ResetTransform();
        }
    }

    public void HideRocks(Bitmap bitmap)
    {
        using var image = new BitmapLockAdapter(bitmap);
        var rgb = image.ReadPixels();
        var hsv = rgb.AsHsv();
        Predicate<PixelHsv> predicate = p => p.IsWithinBounds(new PixelHsv(65, 0f, 0), new PixelHsv(195, 1f, 1f));
        var backgroundPixels = ImageHelpers.CollectPixels(hsv, predicate);

        var hueMean = backgroundPixels.Average(p => p.H);
        var hueStd = Math.Sqrt(backgroundPixels.Average(p => Math.Pow(p.H - hueMean, 2)));
        var hueDist = new NormalDistribution(hueMean, hueStd);

        var satMean = backgroundPixels.Average(p => p.S);
        var satStd = Math.Sqrt(backgroundPixels.Average(p => Math.Pow(p.S - satMean, 2)));
        var satDist = new NormalDistribution(satMean, satStd);

        var valueMean = backgroundPixels.Average(p => p.V);
        var valueStd = Math.Sqrt(backgroundPixels.Average(p => Math.Pow(p.V - valueMean, 2)));
        var valueDist = new NormalDistribution(valueMean, valueStd);


        predicate = p => p.IsWithinBounds(new PixelHsv(0, 0f, 0), new PixelHsv(60, 0.5f, 1)) ||
                                     p.IsWithinBounds(new PixelHsv(200, 0f, 0), new PixelHsv(360, 0.5f, 1));

        MarkPixels(hsv, predicate);

        hsv = ImageHelpers.Dilation(
            ImageHelpers.MorphologicalOpening(hsv, 5, 10), 15);

        var threshold = (image.Width / 100) * (image.Height / 100);
        var obj = DetectObjects(hsv)
            .Where(x => x.Count >= threshold)
            .MaxBy(x => x.Count);

        if (obj != null)
        {
            var ellipseRate = EllipseRate(obj, hsv, predicate);

            if (ellipseRate > 0.8)
                foreach (var (r, c) in obj)
                {
                    var h = Math.Min(Math.Max((int)hueDist.Sample(),0), 360);
                    var s = Math.Min(Math.Max((float)satDist.Sample(), 0), 1);
                    var v = Math.Min(Math.Max((float)valueDist.Sample(), 0), 1);
                    var pRGB = new PixelHsv(h, s, v).AsRgb();

                    var binary = Convert.ToString(pRGB.R, 2).PadLeft(3, '0');
                    var sb = new StringBuilder(binary);
                    sb[binary.Length -1] = '1';
                    sb[binary.Length - 2] = '1';
                    sb[binary.Length - 3] = '1';
                    pRGB.R = Convert.ToByte(sb.ToString(), 2);

                    binary = Convert.ToString(pRGB.G, 2).PadLeft(3, '0');
                    sb = new StringBuilder(binary);
                    sb[binary.Length - 1] = '1';
                    sb[binary.Length - 2] = '1';
                    sb[binary.Length - 3] = '1';
                    pRGB.G = Convert.ToByte(sb.ToString(), 2);

                    binary = Convert.ToString(pRGB.B, 2).PadLeft(3, '0');
                    sb = new StringBuilder(binary);
                    sb[binary.Length - 1] = '1';
                    sb[binary.Length - 2] = '1';
                    sb[binary.Length - 3] = '1';
                    pRGB.B = Convert.ToByte(sb.ToString(), 2);

                    rgb[r, c] = pRGB;
                }
        }
        image.WritePixels(rgb);
    }

    public void FindRocks(Bitmap bitmap)
    {
        using var image = new BitmapLockAdapter(bitmap);

        var hsv = image.ReadPixels().AsHsv();
        Predicate<PixelHsv> predicate = p => p.IsWithinBounds(new PixelHsv(0, 0f, 0), new PixelHsv(60, 0.5f, 1)) ||
                                             p.IsWithinBounds(new PixelHsv(200, 0f, 0), new PixelHsv(360, 0.5f, 1));

        MarkPixels(hsv, predicate);
        hsv = ImageHelpers.Dilation(
            ImageHelpers.MorphologicalOpening(hsv, 5, 10), 15);
        
        var threshold = (image.Width / 100) * (image.Height / 100);
        var obj = DetectObjects(hsv)
            .Where(x => x.Count >= threshold)
            .MaxBy(x => x.Count);
        
        var coverColor = 10;
        if (obj != null)
        {
            var ellipseRate = EllipseRate(obj, hsv, predicate);
            
            if (ellipseRate > 0.8)
                foreach (var (r, c) in obj)
                {
                    hsv[r, c].H = coverColor;
                    hsv[r, c].S = 1;
                    hsv[r, c].V = 1;
                }

            coverColor = (coverColor + 30) % 360;
        }
        image.WritePixels(hsv
            //.Cover(new PixelHsv(330, 0.3f, 0.3f), new PixelHsv(360, 1, 1), new PixelHsv(110, 0.95f, 0.95f))
            //.Cover(new PixelHsv(0, 0.3f, 0.3f), new PixelHsv(30, 1, 1), new PixelHsv(110, 0.95f, 0.95f))
            .AsRgb()
        );
    }

    public void FindHiddenRocks(Bitmap bitmap)
    {
        using var image = new BitmapLockAdapter(bitmap);

        var rgb = image.ReadPixels();
        Predicate<PixelRgb> predicate = p => 
            Convert.ToString(p.R, 2).EndsWith("111")
            && Convert.ToString(p.G, 2).EndsWith("111")
            && Convert.ToString(p.B, 2).EndsWith("111");

        var rowsCount = rgb.GetLength(0);
        var colsCount = rgb.GetLength(1);
        for (var r = 0; r < rowsCount; r++)
        {
            for (var c = 0; c < colsCount; c++)
            {
                if (predicate(rgb[r, c]))
                {
                    rgb[r, c].R = 255;
                    rgb[r, c].G = 0;
                    rgb[r, c].B = 0;
                }
            }
        }
        image.WritePixels(rgb);
    }

    public void MarkPixels(PixelHsv[,] pixels, Predicate<PixelHsv> condition)
    {
        var rowsCount = pixels.GetLength(0);
        var colsCount = pixels.GetLength(1);
        for (var r = 0; r < rowsCount; r++)
        {
            for (var c = 0; c < colsCount; c++)
            {
                if (condition(pixels[r, c]))
                    pixels[r, c].IsMarked = true;
            }
        }
    }

    public void RemoveNoise(Bitmap bitmap)
    {
        using var image = new BitmapLockAdapter(bitmap);
        var matrix = image.ReadPixels().AsHsv();
        Predicate<PixelHsv> predicate = p => p.IsWithinBounds(new PixelHsv(0, 0f, 0), new PixelHsv(60, 0.5f, 1)) ||
                                     p.IsWithinBounds(new PixelHsv(200, 0f, 0), new PixelHsv(360, 0.5f, 1));

        MarkPixels(matrix, predicate);
        var result = ImageHelpers.MorphologicalClosing(
                ImageHelpers.MorphologicalOpening(matrix));
        for (int i = 0; i < result.GetLength(0); i++)
            for (int j = 0; j < result.GetLength(1); j++)
                if (result[i, j].IsMarked)
                {
                    result[i, j].S = 1;
                    result[i, j].V = 1;

                }

        image.WritePixels(result.AsRgb());
    }

    public void ShowHue(Bitmap bitmap)
    {
        using var image = new BitmapLockAdapter(bitmap);

        var hsv = image.ReadPixels().AsHsv();
        for(int i =0; i < hsv.GetLength(0); i++)
        {
            for(int j=0; j< hsv.GetLength(1); j++)
            {
                //hsv[i, j].H = 360;
                hsv[i, j].S = 1;
                hsv[i, j].V = 1;
            }
        }
        image.WritePixels(hsv
            .AsRgb()
         );
    }
}
