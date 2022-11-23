using ImageProcessing.Core.Interfaces.Services;
using ImageProcessing.Core.Model;
using System.Drawing;
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

        while (points.Pairwise().Any(t => t.Item3 < 6))
        {
            var pairsToMerge = points.Pairwise().Where(t => t.Item1 != t.Item2 && t.Item3 < 6).ToHashSet();
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
        return new [] { p1, p2 };
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
        using var image = new BitmapLockAdapter(bitmap);
        var hsv = image.ReadPixels().AsHsv();

        var objects = DetectObjects(hsv,
            p => p.IsWithinBounds(new PixelHsv(330, 0.3f, 0.3f), new PixelHsv(360, 1, 1)) ||
                 p.IsWithinBounds(new PixelHsv(0, 0.3f, 0.3f), new PixelHsv(30, 1, 1)));

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
            if (triangle != null)
            {
                using var pen = new Pen(Color.Black, 20);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.DrawLine(pen, triangle.A.Row, triangle.A.Column, triangle.B.Row, triangle.B.Column);
                graphics.DrawLine(pen, triangle.A.Row, triangle.A.Column, triangle.C.Row, triangle.C.Column);
                graphics.DrawLine(pen, triangle.C.Row, triangle.C.Column, triangle.B.Row, triangle.B.Column);
            }
        }
        // image.WritePixels(hsv
        //     //.Cover(new PixelHsv(330, 0.3f, 0.3f), new PixelHsv(360, 1, 1), new PixelHsv(110, 0.95f, 0.95f))
        //     //.Cover(new PixelHsv(0, 0.3f, 0.3f), new PixelHsv(30, 1, 1), new PixelHsv(110, 0.95f, 0.95f))
        //     .AsRgb()
        // );
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
