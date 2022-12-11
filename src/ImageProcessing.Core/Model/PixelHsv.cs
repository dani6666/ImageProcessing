namespace ImageProcessing.Core.Model;

public class PixelHsv : IPixel
{
    /** Hue: 0 - 360 */
    public float H { get; set; }
    /** Saturation: 0 - 1 */
    public float S { get; set; }
    /** Value: 0 - 1 */
    public float V { get; set; }

    public bool IsMarked { get; set; }

    public PixelHsv(float hue, float saturation, float value)
    {
        H = hue;
        S = saturation;
        V = value;
    }

    public bool IsWithinBounds(PixelHsv lower, PixelHsv upper)
    {
        return lower.S <= S && S <= upper.S &&
               lower.H <= H && H <= upper.H &&
               lower.V <= V && V <= upper.V;
    }
    
    public PixelRgb AsRgb()
    {
        var hi = Convert.ToInt32(Math.Floor(H / 60)) % 6;
        var f = H / 60 - Math.Floor(H / 60);
        var value = V * 255;
        var v = Convert.ToByte(value);
        var p = Convert.ToByte(value * (1 - S));
        var q = Convert.ToByte(value * (1 - f * S));
        var t = Convert.ToByte(value * (1 - (1 - f) * S));

        return hi switch
        {
            0 => new PixelRgb(v, t, p),
            1 => new PixelRgb(q, v, p),
            2 => new PixelRgb(p, v, t),
            3 => new PixelRgb(p, q, v),
            4 => new PixelRgb(t, p, v),
            _ => new PixelRgb(v, p, q)
        };
    }
}