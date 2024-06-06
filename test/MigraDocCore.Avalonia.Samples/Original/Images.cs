using System;
using System.Reflection;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;

namespace Images
{
    /// <summary>
    /// This is a sample program for MigraDoc documents.
    /// </summary>
    class Images
    {
        /// <summary>
        /// Creates an absolutely minimalistic document.
        /// </summary>
        public static Document CreateDocument()
        {
            // Create a new MigraDoc document.
            var document = new Document();

            // Add a section to the document.
            var section = document.AddSection();

            // Add a paragraph to the section.
            var paragraph = section.AddParagraph();

            // Add some text to the paragraph.
            paragraph.AddFormattedText("Hello, MigraDoc!", TextFormat.Italic);
            paragraph.Format.Font.Size = 20;
            var source = ImageSource.FromStream("MigraDoc.png", () => Assembly.GetExecutingAssembly().GetManifestResourceStream("MigraDoc.png"));
            var image = section.AddImage(source);

            return document;
        }
    }
}
