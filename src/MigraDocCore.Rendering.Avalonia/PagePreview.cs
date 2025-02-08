using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using MigraDocCore.Rendering;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Avalonia;

namespace MigraDocCore.Avalonia;

public class PagePreview : Control
{
    public static readonly DirectProperty<PagePreview, (DocumentRenderer?, int)> PageProperty =
        AvaloniaProperty.RegisterDirect<PagePreview, (DocumentRenderer?, int)>
            (nameof(Page), o => o.Page, (o, v) => o.Page = v);


    private (DocumentRenderer?, int) page;
    private PageInfo? info;

    public (DocumentRenderer?, int) Page
    {
        get => this.page;
        set
        {
            if(value == this.page)
                return;

            this.page = value;
            var (renderer, pageNumber) = value;
            this.info =
                ((renderer is not null) && pageNumber >= 1 && pageNumber <= renderer.FormattedDocument.PageCount)
                    ? renderer.FormattedDocument.GetPageInfo(pageNumber)
                    : null;

            (this.Width, this.Height) = GetDimensions(this.info);
        }
    }

    public override void Render(DrawingContext context)
    {
        if(this.info is not null)
        {
            var size = new XSize(this.info.Width, this.info.Height);

            var (renderer, pageNumber) = this.page;
            if(renderer is not null && pageNumber >= 1 && pageNumber <= renderer.FormattedDocument.PageCount)
            {
                using var gfx = XGraphics.FromRenderer(context.CreateXGraphicsRenderer(CultureInfo.CurrentCulture), size, XGraphicsUnit.Point, XPageDirection.Downwards);
                using var _ = context.PushXGraphicsUnitTransform(gfx.PageUnit);
                renderer.RenderPage(gfx, pageNumber, PageRenderOptions.All);
            }

            var (width, height) = GetDimensions(this.info);
            var outline = new Rect(0, 0, width, height);
            context.DrawRectangle(null, new Pen(Brushes.DarkGray, 1.0), outline);
        }

        base.Render(context);
    }

    private static (double, double) GetDimensions(PageInfo? info)
    {
        if(info is null)
            return (double.NaN, double.NaN);
        if(info.Orientation == PdfSharpCore.PageOrientation.Landscape)
            return (info.Height.Presentation, info.Width.Presentation);
        else
            return (info.Width.Presentation, info.Height.Presentation);
    }
}
