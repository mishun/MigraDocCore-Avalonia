using Avalonia;
using Avalonia.Media;

namespace PdfSharpCore.Drawing.Avalonia;

internal static class Geometries
{
    private static FillRule GetFillRule(XFillMode fr)
    {
        switch (fr)
        {
            case XFillMode.Alternate: return FillRule.EvenOdd;
            case XFillMode.Winding: return FillRule.NonZero;
            default: return FillRule.EvenOdd;
        }
    }

    private static void BeginFigureAt(StreamGeometryContext ctx, XPoint p, XFillMode? fillMode)
    {
        if(fillMode is XFillMode fm)
        {
            ctx.SetFillRule(GetFillRule(fm));
            ctx.BeginFigure(new Point(p.X, p.Y), true);
        }
        else
            ctx.BeginFigure(new Point(p.X, p.Y), false);
    }


    public static Rect MakeRect(XRect rect) =>
        new Rect(new Point(rect.TopLeft.X, rect.TopLeft.Y), new Point(rect.BottomRight.X, rect.BottomRight.Y));


    public static Geometry MakeEllipse(double x, double y, double width, double height)
    {
        return new EllipseGeometry(new Rect(x, y, width, height));
    }

    public static Geometry MakePolyLine(XPoint[] points, bool isClosed)
    {
        var g = new StreamGeometry();
        using var ctx = g.Open();
        BeginFigureAt(ctx, points[0], null);
        for(int i = 1; i < points.Length; i++)
        {
            var p = points[i];
            ctx.LineTo(new Point(p.X, p.Y));
        }
        ctx.EndFigure(isClosed);
        return g;
    }

    public static Geometry MakePolygon(XPoint[] points, XFillMode fillMode)
    {
        var g = new StreamGeometry();
        using var ctx = g.Open();
        BeginFigureAt(ctx, points[0], fillMode);
        for(int i = 1; i < points.Length; i++)
        {
            var p = points[i];
            ctx.LineTo(new Point(p.X, p.Y));
        }
        ctx.EndFigure(true);
        return g;
    }

    public static Geometry MakeBezier(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
    {
        var g = new StreamGeometry();
        using var ctx = g.Open();
        ctx.BeginFigure(new Point(x1, y1), false);
        ctx.CubicBezierTo(new Point(x2, y2), new Point(x3, y3), new Point(x4, y4));
        ctx.EndFigure(false);
        return g;
    }

    public static Geometry MakeBeziers(XPoint[] points)
    {
        var g = new StreamGeometry();
        using var ctx = g.Open();
        BeginFigureAt(ctx, points[0], null);
        for(int i = 1; i + 2 < points.Length; i += 3)
        {
            var p0 = points[i];
            var p1 = points[i + 1];
            var p2 = points[i + 2];
            ctx.CubicBezierTo(new Point(p0.X, p0.Y), new Point(p1.X, p1.Y), new Point(p2.X, p2.Y));
        }
        ctx.EndFigure(false);
        return g;
    }


    private static void MakeCurveSegment(StreamGeometryContext ctx, XPoint pt0, XPoint pt1, XPoint pt2, XPoint pt3, double tension3)
    {
        ctx.CubicBezierTo (
            new Point(pt1.X + tension3 * (pt2.X - pt0.X), pt1.Y + tension3 * (pt2.Y - pt0.Y)),
            new Point(pt2.X - tension3 * (pt3.X - pt1.X), pt2.Y - tension3 * (pt3.Y - pt1.Y)),
            new Point(pt2.X, pt2.Y)
        );
    }

    public static Geometry MakeCurve(XPoint[] points, double tension3, bool isClosed, XFillMode? fillMode)
    {
        var g = new StreamGeometry();
        using var ctx = g.Open();

        BeginFigureAt(ctx, points[0], fillMode);
        var count = points.Length;
        if (count == 2)
            MakeCurveSegment(ctx, points[0], points[0], points[1], points[1], tension3);
        else if (count > 2)
        {
            MakeCurveSegment(ctx, (isClosed ? points[count - 1] : points[0]), points[0], points[1], points[2], tension3);
            for(int idx = 1; idx + 2 < count; idx++)
                MakeCurveSegment(ctx, points[idx - 1], points[idx], points[idx + 1], points[idx + 2], tension3);
            MakeCurveSegment(ctx, points[count - 3], points[count - 2], points[count - 1], (isClosed ? points[0] : points[count - 1]), tension3);
            if (isClosed)
                MakeCurveSegment(ctx, points[count - 2], points[count - 1], points[0], points[1], tension3);
        }

        ctx.EndFigure(isClosed);
        return g;
    }


    public static Geometry MakeArc(double x, double y, double width, double height, double startAngle, double sweepAngle)
    {
        var α = startAngle;
        if(α < 0.0)
            α = α + (1.0 + Math.Floor((Math.Abs(α) / 360.0))) * 360.0;
        else if(α > 360.0)
            α = α - Math.Floor(α / 360.0) * 360.0;

        if(Math.Abs(sweepAngle) >= 360.0)
           sweepAngle = Math.Sign(sweepAngle) * 360.0;

        var β = startAngle + sweepAngle;
        if(β < 0.0)
            β = β + (1.0 + Math.Floor((Math.Abs(β) / 360.0))) * 360.0;
        else if(β > 360.0)
            β = β - Math.Floor(β / 360.0) * 360.0;

        if (α == 0.0 && β < 0.0)
            α = 360.0;
        else if (α == 360.0 && β > 0.0)
            α = 0.0;

        // Scanling factor.
        var δx = 0.5 * width;
        var δy = 0.5 * height;

        // Center of ellipse.
        var x0 = x + δx;
        var y0 = y + δy;


    /*    if (width == height)
        {
            // Circular arc needs no correction.
            ? = ? * Calc.Deg2Rad;
            ? = ? * Calc.Deg2Rad;
        }
        else
        {
            // Elliptic arc needs the angles to be adjusted such that the scaling transformation is compensated.
            ? = ? * Calc.Deg2Rad;
            sin? = Math.Sin(?);
            if (Math.Abs(sin?) > 1E-10)
            {
                if (? < Math.PI)
                    ? = Math.PI / 2 - Math.Atan(?y * Math.Cos(?) / (?x * sin?));
                else
                    ? = 3 * Math.PI / 2 - Math.Atan(?y * Math.Cos(?) / (?x * sin?));
            }
            //? = Calc.?Half - Math.Atan(?y * Math.Cos(?) / (?x * sin?));
            ? = ? * Calc.Deg2Rad;
            sin? = Math.Sin(?);
            if (Math.Abs(sin?) > 1E-10)
            {
                if (? < Math.PI)
                    ? = Math.PI / 2 - Math.Atan(?y * Math.Cos(?) / (?x * sin?));
                else
                    ? = 3 * Math.PI / 2 - Math.Atan(?y * Math.Cos(?) / (?x * sin?));
            }
            //? = Calc.?Half - Math.Atan(?y * Math.Cos(?) / (?x * sin?));
        }*/

        var sinα = Math.Sin(α);
        var cosα = Math.Cos(α);
        var sinβ = Math.Sin(β);
        var cosβ = Math.Cos(β);

        var startPoint = new Point(x0 + δx * cosα, y0 + δy * sinα);
        var destPoint = new Point(x0 + δx * cosβ, y0 + δy * sinβ);
        var size = new Size(δx, δy);
        var isLargeArc = Math.Abs(sweepAngle) >= 180.0;
        var sweepDirection = (sweepAngle > 0.0) ? SweepDirection.Clockwise : SweepDirection.CounterClockwise;

        var g = new StreamGeometry();
        using var ctx = g.Open ();
        ctx.BeginFigure(startPoint, false);
        ctx.ArcTo(destPoint, size, sweepAngle / 180.0 * Math.PI, isLargeArc, sweepDirection);
        ctx.EndFigure(false);
        return g;
    }
}
