namespace ImageProcessing.Core.Model;

public class Triangle
{
    public Point A { get; set; }
    public Point B { get; set; }
    public Point C { get; set; }
    
    public Triangle(Point a, Point b, Point c)
    {
        A = a;
        B = b;
        C = c;
    }
}