using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using PdfSharpCore.Drawing;

namespace MigraDocCore.Avalonia.Rendering;


internal class FontInfo
{
    private readonly Typeface typeface;
    private readonly double fontSize;
    private readonly FontMetrics metrics;

    public FontInfo(Typeface typeface, double fontSize)
    {
        this.typeface = typeface;
        this.fontSize = fontSize;
        this.metrics = new FontMetrics(typeface, fontSize);
    }

    public double Ascent => this.metrics.Ascent;
    public double Descent => this.metrics.Descent;

    public double StrikethroughPosition => this.metrics.StrikethroughPosition;
    public double StrikethroughThickness => this.metrics.StrikethroughThickness;

    public double UnderlinePosition => this.metrics.UnderlinePosition;
    public double UnderlineThickness => this.metrics.UnderlineThickness;

    public FormattedText Format(string text, TextAlignment textAlignment)
    {
        var ft = new FormattedText(text, this.typeface, this.fontSize, textAlignment, TextWrapping.NoWrap, Size.Infinity);
        return ft;
    }
}

internal static class Fonts
{
    private static readonly (string, FontWeight?, FontStyle?)[] fontTags =
        new (string, FontWeight?, FontStyle?)[] {
            ("Black", FontWeight.Black, null),
            ("Black Italic", FontWeight.Black, FontStyle.Italic),
            ("Bold", FontWeight.Bold, null),
            ("Bold Italic", FontWeight.Bold, FontStyle.Italic),
            ("Light", FontWeight.Light, null),
            ("Light Italic", FontWeight.Light, FontStyle.Italic),
            ("Semibold", FontWeight.SemiBold, null),
            ("Semibold Italic", FontWeight.SemiBold, FontStyle.Italic),
            ("Semilight", FontWeight.SemiLight, null),
            ("Semilight Italic", FontWeight.SemiLight, FontStyle.Italic)
        };


    private static (string, FontWeight, FontStyle) LookupTags(XFont xfont)
    {
        var name = xfont.FontFamily.Name.Trim();
        var defaultWeight = xfont.Bold ? FontWeight.Bold : FontWeight.Normal;
        var defaultStyle = xfont.Italic ? FontStyle.Italic : FontStyle.Normal;
        foreach (var (suffix, fontWeght, fontStyle) in fontTags)
            if (name.EndsWith(suffix))
            {
                var trimmed = name.Substring(0, name.Length - suffix.Length).Trim();
                return (trimmed, fontWeght ?? defaultWeight, fontStyle ?? defaultStyle);
            }
        return (name, defaultWeight, defaultStyle);
    }

    public static FontInfo MakeFont(XFont xfont)
    {
        var (familyName, fontWeight, fontStyle) = LookupTags(xfont);
        //Console.WriteLine(familyName);
        var fontFamily = new FontFamily(familyName);
        var typeface = new Typeface(fontFamily, fontStyle, fontWeight);
        return new FontInfo(typeface, xfont.Size);
    }
}
