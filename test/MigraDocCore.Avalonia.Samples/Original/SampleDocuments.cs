using System.Reflection;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using SixLabors.ImageSharp.PixelFormats;

namespace MigraDocCore.Avalonia.Samples;

sealed class SampleDocuments
{
    static SampleDocuments()
    {
        ImageSource.ImageSourceImpl = new PdfSharpCore.Utils.ImageSharpImageSource<Rgba32>();
    }

    /// <summary>
    /// Creates the initial sample document.
    /// </summary>
    public static Document CreateSample1()
    {
        // Create a new MigraDoc document
        var document = new Document();

        // Add a section to the document
        var section = document.AddSection();

        // Add a paragraph to the section
        var paragraph = section.AddParagraph();

        // Add some text to the paragraph
        paragraph.AddFormattedText("Hi!");

        paragraph = section.AddParagraph();
        paragraph.AddText("This is a MigraDoc document created for the DocumentViewer sample application.");
        paragraph.AddText("The DocumentViewer demonstrates all techniques you need to preview and print a MigraDoc document, and convert it to a PDF, RTF, or image file.");

        section.AddParagraph();
        section.AddParagraph();
        paragraph = section.AddParagraph("A4 portrait");
        paragraph.Format.Font.Size = "1.5cm";

        section.AddPageBreak();
        section.AddParagraph().AddText("Page 2");

        section = document.AddSection();
        section.PageSetup.Orientation = Orientation.Landscape;

        paragraph = section.AddParagraph("A4 landscape");
        paragraph.Format.Font.Size = "1.5cm";

        section.AddPageBreak();
        section.AddParagraph().AddText("Page 4");

        section = document.AddSection();
        section.PageSetup.Orientation = Orientation.Portrait;
        section.PageSetup.PageFormat = PageFormat.A5;

        paragraph = section.AddParagraph("A5 portrait");
        paragraph.Format.Font.Size = "1.5cm";

        section.AddPageBreak();
        section.AddParagraph().AddText("Page 6");

        return document;
    }

    /// <summary>
    /// Creates the document from sample HelloMigraDoc.
    /// </summary>
    public static Document CreateSample2()
    {
        return HelloMigraDoc.Documents.CreateDocument();
    }

    public static Document CreateSample3()
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Invoice.xml");
        return new Invoice.InvoiceForm(stream).CreateDocument();
    }

    public static Document CreateSample4()
    {
        return Images.Images.CreateDocument();
    }
}
