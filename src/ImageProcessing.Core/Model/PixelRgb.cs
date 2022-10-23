
using System.Drawing;

namespace ImageProcessing.Core.Model;

public class PixelRgb : IPixel
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    public PixelRgb(byte red, byte green, byte blue)
    {
        R = red;
        G = green;
        B = blue;
    }

    public bool IsRed() => R == 255 && G == 0 && B == 0;

    public PixelHsv AsHsv()
    { 
        var max = Math.Max(R, Math.Max(G, B));
        var min = Math.Min(R, Math.Min(G, B));
        var color = Color.FromArgb(R, G, B);
        return new PixelHsv(color.GetHue(), max == 0 ? 0 : 1d - (1d * min / max), max / 255d);
    } 
}
