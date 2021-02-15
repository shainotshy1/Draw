using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace iDraw.Models
{
    public class Constants
    {
        public static SKColor colorConstant = new SKColor(79, 217, 56, 0xFF);
        public static double size = 10;
        public static SKPaintStyle style = SKPaintStyle.Stroke;
        public static List<SKPaintStyle> styles = new List<SKPaintStyle>{SKPaintStyle.Stroke,SKPaintStyle.Fill,SKPaintStyle.StrokeAndFill};
        public static List<SKColor> colors = new List<SKColor> { new SKColor(79, 217, 56, 0xFF), new SKColor(56, 217, 208, 0xFF), new SKColor(56, 113, 217, 0xFF), new SKColor(211, 217, 56, 0xFF), new SKColor(217, 56, 56, 0xFF), new SKColor(217, 101, 56, 0xFF), new SKColor(255, 255, 255, 0xFF) };
        public static List<SKStrokeCap> strokecaps = new List<SKStrokeCap> {SKStrokeCap.Round,SKStrokeCap.Butt,SKStrokeCap.Square};
        public static List<SKStrokeJoin> strokejoins = new List<SKStrokeJoin> { SKStrokeJoin.Round,SKStrokeJoin.Bevel,SKStrokeJoin.Miter };
    }
}
