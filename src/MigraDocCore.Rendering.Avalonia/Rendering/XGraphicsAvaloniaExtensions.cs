
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace PdfSharpCore.Drawing.Avalonia;


public static class XGraphicsAvaloniaExtensions
{
    public static IXGraphicsRenderer CreateXGraphicsRenderer(this DrawingContext context, CultureInfo textCulture)
    {
        return new XGraphicsRendererToDrawingContext(context, textCulture);
    }

    public static IDisposable PushXGraphicsUnitTransform(this DrawingContext context, XGraphicsUnit pageUnit)
    {
        var scale =
            pageUnit switch
            {
                XGraphicsUnit.Presentation => 1.0,
                XGraphicsUnit.Point => 96.0 / 72.0,
                XGraphicsUnit.Inch => 96.0,
                XGraphicsUnit.Millimeter => 96.0 / 25.4,
                XGraphicsUnit.Centimeter => 96.0 / 2.54,
                _ => 1.0
            };
        return context.PushTransform(Matrix.CreateScale(scale, scale));
    }
}
