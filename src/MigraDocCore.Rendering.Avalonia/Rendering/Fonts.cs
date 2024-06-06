using System;
using Avalonia;
using Avalonia.Media;

namespace PdfSharpCore.Drawing.Avalonia;


internal class TypefaceInfo
{
    private readonly Typeface typeface;
    private readonly FontMetrics metrics;

    public TypefaceInfo(Typeface typeface, IGlyphTypeface glyphTypeface)
    {
        this.typeface = typeface;
        this.metrics = glyphTypeface.Metrics;
    }

    public Typeface Typeface => this.typeface;

    public double GetAscent(double fontEmHeight) => fontEmHeight * this.metrics.Ascent / this.metrics.DesignEmHeight;
    public double GetDescent(double fontEmHeight) => fontEmHeight * this.metrics.Descent / this.metrics.DesignEmHeight;
}

internal static class Typefaces
{
    private static readonly (string, FontWeight?, FontStyle?)[] fontTags =
        [
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
        ];


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

    public static TypefaceInfo GetTypefaceInfo(XFont xfont)
    {
        var (familyName, fontWeight, fontStyle) = LookupTags(xfont);

        var fontFamily = new FontFamily(familyName);
        var typeface = new Typeface(fontFamily, fontStyle, fontWeight);
        var manager = FontManager.Current;
        if(manager.TryGetGlyphTypeface(typeface, out var glyphTypeface))
        {
            return new TypefaceInfo(typeface, glyphTypeface);
        }
        else
        {
            var fallbackTypeface = new Typeface(manager.DefaultFontFamily, typeface.Style, typeface.Weight);
            if(manager.TryGetGlyphTypeface(typeface, out var fallbackGlyphTypeface))
            {
                return new TypefaceInfo(fallbackTypeface, fallbackGlyphTypeface);
            }
            else
            {
                throw new Exception("No glyph typeface found");
            }
        }
    }
}
