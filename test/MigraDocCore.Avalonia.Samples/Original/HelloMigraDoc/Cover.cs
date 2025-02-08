using System;
using System.Reflection;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;

namespace MigraDocCore.Avalonia.Samples.HelloMigraDoc;

public class Cover
{
    /// <summary>
    /// Defines the cover page.
    /// </summary>
    public static void DefineCover(Document document)
    {
        var section = document.AddSection();

        var paragraph = section.AddParagraph();
        paragraph.Format.SpaceAfter = "3cm";

        var source = ImageSource.FromStream("Logo landscape.png", () => Assembly.GetExecutingAssembly().GetManifestResourceStream("Logo landscape.png"));
        var image = section.AddImage(source);
        image.Width = "10cm";

        paragraph = section.AddParagraph("A sample document that demonstrates the\ncapabilities of MigraDoc");
        paragraph.Format.Font.Size = 16;
        paragraph.Format.Font.Color = Colors.DarkRed;
        paragraph.Format.SpaceBefore = "8cm";
        paragraph.Format.SpaceAfter = "3cm";

        paragraph = section.AddParagraph("Rendering date: ");
        paragraph.AddDateField();
    }
}
