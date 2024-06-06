using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PdfSharpCore.Drawing.Avalonia;

internal static class Media
{
    public static IBrush MakeSolidBrush(XColor xcolor)
    {
        var color = Color.FromRgb(xcolor.R, xcolor.G, xcolor.B);
        return new SolidColorBrush(color, xcolor.A).ToImmutable();
    }

    public static IBrush MakeBrush(XBrush xbrush)
    {
        switch (xbrush)
        {
            case XSolidBrush solid:
                return MakeSolidBrush(solid.Color);

            case XLinearGradientBrush grad:
                var gb = new LinearGradientBrush();
                gb.StartPoint = new RelativePoint(new Point(0.0, 0.0), RelativeUnit.Absolute); // TODO: getter for _useRect
                gb.EndPoint = new RelativePoint(new Point(1.0, 1.0), RelativeUnit.Absolute); // TODO: getter for _useRect
                gb.GradientStops.Add(new GradientStop(Colors.Red, 0.0)); // TODO: getter for _color1
                gb.GradientStops.Add(new GradientStop(Colors.Blue, 1.0)); // TODO: getter for _color2
                return gb.ToImmutable();

            case XRadialGradientBrush grad:
                var rb = new RadialGradientBrush();
                rb.Center = new RelativePoint(new Point(0.0, 0.0), RelativeUnit.Absolute); // TODO: getter for _center
                rb.GradientStops.Add(new GradientStop(Colors.Red, 0.0)); // TODO: getter for _color1
                rb.GradientStops.Add(new GradientStop(Colors.Blue, 1.0)); // TODO: getter for _color2
                return rb.ToImmutable();

            default:
                return Brushes.Black;
        }
    }

    public static IPen MakePen(XPen xpen)
    {
        var dashStyle =
            xpen.DashStyle switch
            {
                XDashStyle.Solid => null,
                XDashStyle.Dash => DashStyle.Dash,
                XDashStyle.Dot => DashStyle.Dot,
                XDashStyle.DashDot => DashStyle.DashDot,
                XDashStyle.DashDotDot => DashStyle.DashDotDot,
                XDashStyle.Custom => new DashStyle(xpen.DashPattern, xpen.DashOffset),
                _ => null
            };

        var lineCap =
            xpen.LineCap switch
            {
                XLineCap.Flat => PenLineCap.Flat,
                XLineCap.Round => PenLineCap.Round,
                XLineCap.Square => PenLineCap.Square,
                _ => PenLineCap.Flat
            };

        var lineJoin =
            xpen.LineJoin switch
            {
                XLineJoin.Miter => PenLineJoin.Miter,
                XLineJoin.Round => PenLineJoin.Round,
                XLineJoin.Bevel => PenLineJoin.Bevel,
                _ => PenLineJoin.Miter
            };

        var brush =
            (xpen.Brush is null)
                ? MakeSolidBrush(xpen.Color)
                : MakeBrush(xpen.Brush);

        var thickness = (96.0 / 72.0) * xpen.Width;
        return new Pen(brush, thickness, dashStyle, lineCap, lineJoin).ToImmutable();
    }

    public static Bitmap MakeImage(XImage ximage)
    {
        using var stream = ximage.AsBitmap();
        return new Bitmap(stream);
    }
}
