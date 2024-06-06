using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace PdfSharpCore.Drawing.Avalonia;

internal class XGraphicsRendererToDrawingContext : IXGraphicsRenderer
{
    private readonly DrawingContext ctx;
    private readonly CultureInfo textCulture;
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

        var typeface = this.GetTypeface(xfont);

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
                XLineAlignment.Center => (2.0 / 3.0) * typeface.GetAscent(xfont.Size) + 0.5 * layoutRectangle.Height,
                XLineAlignment.Far => typeface.GetAscent(xfont.Size) - typeface.GetDescent(xfont.Size) + layoutRectangle.Height,
                XLineAlignment.BaseLine => typeface.GetAscent(xfont.Size),
                _ => 0.0
            };

        var formattedText = new FormattedText(text, this.textCulture, FlowDirection.LeftToRight, typeface.Typeface, xfont.Size, this.GetBrush(xbrush))
        {
            TextAlignment = textAlignment
        };

        if (this.GetTextDecorations(xfont) is TextDecorationCollection textDecorations)
            formattedText.SetTextDecorations(textDecorations);

        this.ctx.DrawText(formattedText, new Point(layoutRectangle.X + offsetX, layoutRectangle.Y + offsetY));
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
        this.ctx.DrawImage(image, Geometries.MakeRect(srcRect), Geometries.MakeRect(destRect));
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
                this.stack.Push(this.ctx.PushTransform(matrix));
                break;

            case XMatrixOrder.Append:
                this.stack.Push(this.ctx.PushTransform(matrix));
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


    internal XGraphicsRendererToDrawingContext(DrawingContext ctx, CultureInfo textCulture)
    {
        this.ctx = ctx;
        this.textCulture = textCulture;
    }



    [return: NotNullIfNotNull(nameof(xbrush))]
    private IBrush? GetBrush(XBrush? xbrush)
    {
        if (xbrush is null)
            return null;

        return Media.MakeBrush(xbrush);
    }

    [return: NotNullIfNotNull(nameof(xpen))]
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


    private TypefaceInfo GetTypeface(XFont xfont)
    {
        return Typefaces.GetTypefaceInfo(xfont);
    }

    private static readonly TextDecorationCollection underlineAndStrikethrough = new TextDecorationCollection { new TextDecoration { Location = TextDecorationLocation.Underline }, new TextDecoration { Location = TextDecorationLocation.Strikethrough } };

    private TextDecorationCollection? GetTextDecorations(XFont xfont)
    {
        if (xfont.Strikeout && xfont.Underline)
            return underlineAndStrikethrough;
        if (xfont.Strikeout)
            return TextDecorations.Strikethrough;
        if (xfont.Underline)
            return TextDecorations.Underline;

        return null;
    }
}
