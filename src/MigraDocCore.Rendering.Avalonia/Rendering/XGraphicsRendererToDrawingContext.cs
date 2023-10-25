using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PdfSharpCore.Drawing;

namespace MigraDocCore.Avalonia.Rendering;

public class XGraphicsRendererToDrawingContext : IXGraphicsRenderer
{
    private readonly DrawingContext ctx;
    private readonly Stack<DrawingContext.PushedState> stack = new();
    private readonly Dictionary<XGraphicsState, int> gstate = new();


    void IXGraphicsRenderer.Close()
    {
        while (this.stack.Count > 0)
        {
            this.stack.Pop().Dispose();
        }
    }

    void IXGraphicsRenderer.DrawLine(XPen? xpen, double x1, double y1, double x2, double y2)
    {
        if (xpen is null)
            return;

        var pen = this.GetPen(xpen);
        this.ctx.DrawLine(pen, new Point(x1, y1), new Point(x2, y2));
    }

    void IXGraphicsRenderer.DrawLines(XPen? xpen, XPoint[]? points)
    {
        if (points is null || points.Length < 1 || xpen is null)
            return;

        var geom = Geometries.MakePolyLine(points, false);
        var pen = this.GetPen(xpen);
        this.ctx.DrawGeometry(null, pen, geom);
    }

    void IXGraphicsRenderer.DrawBezier(XPen? xpen, double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
    {
        if (xpen is null)
            return;

        var geom = Geometries.MakeBezier(x1, y1, x2, y2, x3, y3, x4, y4);
        var pen = this.GetPen(xpen);
        this.ctx.DrawGeometry(null, pen, geom);
    }

    void IXGraphicsRenderer.DrawBeziers(XPen? xpen, XPoint[]? points)
    {
        if (points is null || points.Length < 4 || xpen is null)
            return;

        var geom = Geometries.MakeBeziers(points);
        var pen = this.GetPen(xpen);
        this.ctx.DrawGeometry(null, pen, geom);
    }

    void IXGraphicsRenderer.DrawCurve(XPen? xpen, XPoint[]? points, double tension)
    {
        if (points is null || points.Length < 2 || xpen is null)
            return;

        var geom = Geometries.MakeCurve(points, tension / 3.0, false, null);
        var pen = this.GetPen(xpen);
        this.ctx.DrawGeometry(null, pen, geom);
    }

    void IXGraphicsRenderer.DrawArc(XPen? xpen, double x, double y, double width, double height, double startAngle, double sweepAngle)
    {
        if (xpen is null)
            return;

        var geom = Geometries.MakeArc(x, y, width, height, startAngle, sweepAngle);
        var pen = this.GetPen(xpen);
        this.ctx.DrawGeometry(null, pen, geom);
    }

    void IXGraphicsRenderer.DrawRectangle(XPen? xpen, XBrush? xbrush, double x, double y, double width, double height)
    {
        if (xpen is null && xbrush is null)
            return;

        var brush = this.GetBrush(xbrush);
        var pen = this.GetPen(xpen);
        this.ctx.DrawRectangle(brush, pen, new Rect(x, y, width, height));
    }

    void IXGraphicsRenderer.DrawRectangles(XPen? xpen, XBrush? xbrush, XRect[] rects)
    {
        if (rects is null || rects.Length < 1 || (xpen is null && xbrush is null))
            return;

        var brush = this.GetBrush(xbrush);
        var pen = this.GetPen(xpen);
        foreach (var rect in rects)
            this.ctx.DrawRectangle(brush, pen, Geometries.MakeRect(rect));
    }

    void IXGraphicsRenderer.DrawRoundedRectangle(XPen? xpen, XBrush? xbrush, double x, double y, double width, double height, double ellipseWidth, double ellipseHeight)
    {
        if (xpen is null && xbrush is null)
            return;

        var rect = new Rect(x, y, width, height);
        var brush = this.GetBrush(xbrush);
        var pen = this.GetPen(xpen);
        this.ctx.DrawRectangle(brush, pen, rect, 0.5 * ellipseWidth, 0.5 * ellipseHeight);
    }

    void IXGraphicsRenderer.DrawEllipse(XPen? xpen, XBrush? xbrush, double x, double y, double width, double height)
    {
        if (xpen is null && xbrush is null)
            return;

        var geom = Geometries.MakeEllipse(x, y, width, height);
        var brush = this.GetBrush(xbrush);
        var pen = this.GetPen(xpen);
        this.ctx.DrawGeometry(brush, pen, geom);
    }

    void IXGraphicsRenderer.DrawPolygon(XPen? xpen, XBrush? xbrush, XPoint[]? points, XFillMode fillmode)
    {
        if (points is null || points.Length < 1 || (xpen is null && xbrush is null))
            return;

        var geom = Geometries.MakePolygon(points, fillmode);
        var brush = this.GetBrush(xbrush);
        var pen = this.GetPen(xpen);
        this.ctx.DrawGeometry(brush, pen, geom);
    }

    void IXGraphicsRenderer.DrawPie(XPen? xpen, XBrush? xbrush, double x, double y, double width, double height, double startAngle, double sweepAngle)
    {
        if (xpen is null && xbrush is null)
            return;

        // Not implemented
    }

    void IXGraphicsRenderer.DrawClosedCurve(XPen? xpen, XBrush? xbrush, XPoint[]? points, double tension, XFillMode fillmode)
    {
        if (points is null || points.Length < 2 || (xpen is null && xbrush is null))
            return;

        var geom = Geometries.MakeCurve(points, tension / 3.0, true, fillmode);
        var brush = this.GetBrush(xbrush);
        var pen = this.GetPen(xpen);
        this.ctx.DrawGeometry(brush, pen, geom);
    }

    void IXGraphicsRenderer.DrawPath(XPen? xpen, XBrush? xbrush, XGraphicsPath path)
    {
        if (path is null || (xpen is null && xbrush is null))
            return;

        // Not implemented
    }

    void IXGraphicsRenderer.DrawString(string text, XFont? xfont, XBrush xbrush, XRect layoutRectangle, XStringFormat format)
    {
        if (text is null || xfont is null)
            return;

        var font = this.GetFont(xfont);

        var (textAlignment, offsetX) =
            format.Alignment switch
            {
                XStringAlignment.Near => (TextAlignment.Left, 0.0),
                XStringAlignment.Center => (TextAlignment.Center, 0.5 * layoutRectangle.Width),
                XStringAlignment.Far => (TextAlignment.Right, layoutRectangle.Width),
                _ => (TextAlignment.Left, 0.0)
            };

        var offsetY =
            format.LineAlignment switch
            {
                XLineAlignment.Near => 0.0,
                XLineAlignment.Center => (2.0 / 3.0) * font.Ascent + 0.5 * layoutRectangle.Height,
                XLineAlignment.Far => font.Ascent - font.Descent + layoutRectangle.Height,
                XLineAlignment.BaseLine => font.Ascent,
                _ => 0.0
            };

        var ft = font.Format(text, textAlignment);

        var brush = this.GetBrush(xbrush);
        var origin = new Point(layoutRectangle.X + offsetX, layoutRectangle.Y + offsetY);
        this.ctx.DrawText(brush, origin, ft);

        if (xfont.Strikeout)
        {
            var l = new Point(ft.Bounds.Left, -font.Ascent + font.StrikethroughPosition);
            var r = new Point(ft.Bounds.Right, -font.Ascent + font.StrikethroughPosition);
            this.ctx.DrawLine(new Pen(brush, font.StrikethroughThickness), origin + l, origin + r);
        }

        if (xfont.Underline)
        {
            var l = new Point(ft.Bounds.Left, -font.Ascent + font.UnderlinePosition);
            var r = new Point(ft.Bounds.Right, -font.Ascent + font.UnderlinePosition);
            this.ctx.DrawLine(new Pen(brush, font.UnderlineThickness), origin + l, origin + r);
        }
    }

    void IXGraphicsRenderer.DrawImage(XImage? ximage, double x, double y, double width, double height)
    {
        if (ximage is null || width <= 0.0 || height <= 0.0)
            return;

        using var image = this.GetImage(ximage);
        this.ctx.DrawImage(image, new Rect(x, y, width, height));
    }

    void IXGraphicsRenderer.DrawImage(XImage? ximage, XRect destRect, XRect srcRect, XGraphicsUnit srcUnit)
    {
        if (ximage is null)
            return;

        using var image = this.GetImage(ximage);
        this.ctx.DrawImage(image, Geometries.MakeRect(srcRect), Geometries.MakeRect(destRect), global::Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality);
    }

    void IXGraphicsRenderer.Save(XGraphicsState state)
    {
        this.gstate[state] = this.stack.Count;
    }

    void IXGraphicsRenderer.Restore(XGraphicsState state)
    {
        if (this.gstate.TryGetValue(state, out var target))
        {
            this.gstate.Remove(state);
            while (this.stack.Count > target)
                this.stack.Pop().Dispose();
        }
    }

    void IXGraphicsRenderer.BeginContainer(XGraphicsContainer container, XRect dstrect, XRect srcrect, XGraphicsUnit unit)
    {
        // Not implemented
    }

    void IXGraphicsRenderer.EndContainer(XGraphicsContainer container)
    {
        // Not implemented
    }

    void IXGraphicsRenderer.AddTransform(XMatrix xtransform, XMatrixOrder matrixOrder)
    {
        var matrix = new Matrix(xtransform.M11, xtransform.M12, xtransform.M21, xtransform.M22, xtransform.OffsetX, xtransform.OffsetY);
        switch (matrixOrder)
        {
            case XMatrixOrder.Prepend:
                this.stack.Push(this.ctx.PushPreTransform(matrix));
                break;

            case XMatrixOrder.Append:
                this.stack.Push(this.ctx.PushPostTransform(matrix));
                break;
        }
    }

    void IXGraphicsRenderer.SetClip(XGraphicsPath path, XCombineMode combineMode)
    {
        // Not implemented
    }

    void IXGraphicsRenderer.ResetClip()
    {
        // Not implemented
    }

    void IXGraphicsRenderer.WriteComment(string comment)
    {
        // Not implemented
    }


    public XGraphicsRendererToDrawingContext(DrawingContext ctx)
    {
        this.ctx = ctx;
    }



    private IBrush? GetBrush(XBrush? xbrush)
    {
        if (xbrush is null)
            return null;

        return Media.MakeBrush(xbrush);
    }

    private IPen? GetPen(XPen? xpen)
    {
        if (xpen is null)
            return null;

        return Media.MakePen(xpen);
    }

    private Bitmap GetImage(XImage ximage)
    {
        return Media.MakeImage(ximage);
    }

    private FontInfo GetFont(XFont xfont)
    {
        return Fonts.MakeFont(xfont);
    }


    public static double PageScaleFactor(XGraphicsUnit pageUnit)
    {
        switch(pageUnit)
        {
            case XGraphicsUnit.Presentation: return 1.0;
            case XGraphicsUnit.Point: return 96.0 / 72.0;
            case XGraphicsUnit.Inch: return 96.0;
            case XGraphicsUnit.Millimeter: return 96.0 / 25.4;
            case XGraphicsUnit.Centimeter: return 96.0 / 2.54;
            default: return 1.0;
        }
    }
}


public static class XGraphicsExtensions
{
    public static XGraphics FromDrawingContext(DrawingContext context, XSize size)
    {
        var adapter = new XGraphicsRendererToDrawingContext(context);
        return XGraphics.FromRenderer(adapter, size, XGraphicsUnit.Point, XPageDirection.Downwards);
    }

    public static IDisposable PushPageTransform(DrawingContext context, XGraphics gfx)
    {
        var scale = XGraphicsRendererToDrawingContext.PageScaleFactor(gfx.PageUnit);
        return context.PushPreTransform(Matrix.CreateScale(scale, scale));
    }
}
