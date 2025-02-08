using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using System.Text;

namespace MigraDocCore.Avalonia.Samples;

public class DocModel
{
    private readonly Func<Document> makeDocument;

    public DocModel(string header, Func<Document> makeDocument)
    {
        this.makeDocument = makeDocument;
        this.Header = header;
        this.Document = makeDocument();
    }

    public string Header { get; set; }
    public Document Document { get; set; }


    //public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> SavePdfCommand { get; }
    //public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> OpenPdfCommand { get; }

    public async void SavePdf()
    {
        try
        {
            if (Application.Current.ApplicationLifetime is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime lifetime)
            {
                var opts = new FilePickerSaveOptions() {
                    SuggestedFileName = $"{this.Header}.pdf",
                    DefaultExtension = "pdf",
                    FileTypeChoices = [new FilePickerFileType("PDF document") { Patterns = ["*.pdf"] }],
                    SuggestedStartLocation = await lifetime.MainWindow.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Pictures)
                };

                var pickedFile = await lifetime.MainWindow.StorageProvider.SaveFilePickerAsync(opts);
                if(pickedFile is null)
                    return;

                {
                    var pdfRenderer = new PdfDocumentRenderer(true) { Document = this.makeDocument() };
                    pdfRenderer.RenderDocument();
                    using var memory = new MemoryStream();
                    pdfRenderer.PdfDocument.Save(memory);
                    using var stream = await pickedFile.OpenWriteAsync();
                    await stream.WriteAsync(memory.ToArray());
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex}");
        }
    }

    public async void OpenPdf()
    {
        try
        {
            var path = $"{this.Header}-{Guid.NewGuid().ToString("N").ToUpper()}.pdf";

            {
                var pdfRenderer = new PdfDocumentRenderer(true) { Document = this.makeDocument() };
                pdfRenderer.RenderDocument();
                using var memory = new MemoryStream();
                pdfRenderer.PdfDocument.Save(memory);
                using var file = File.Create(path);
                await file.WriteAsync(memory.ToArray());
            }

            {
                using var fileopener = new Process();
                fileopener.StartInfo.FileName = "explorer";
                fileopener.StartInfo.Arguments = $"\"{path}\"";
                fileopener.Start();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex}");
        }
    }

    public async void SaveDdl()
    {
        try
        {
            if (Application.Current.ApplicationLifetime is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime lifetime)
            {
                var opts = new FilePickerSaveOptions() {
                    SuggestedFileName = $"{this.Header}.mdddl",
                    DefaultExtension = "mdddl",
                    FileTypeChoices = [new FilePickerFileType("DDL document") { Patterns = ["*.mdddl"] }],
                    SuggestedStartLocation = await lifetime.MainWindow.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
                };

                var pickedFile = await lifetime.MainWindow.StorageProvider.SaveFilePickerAsync(opts);
                if (pickedFile is null)
                {
                    return;
                }

                {
                    var document = this.makeDocument();
                    var ddlString = DocumentObjectModel.IO.DdlWriter.WriteToString(document);
                    using var stream = await pickedFile.OpenWriteAsync();
                    using var writer = new StreamWriter(stream, Encoding.UTF8);
                    await writer.WriteAsync(ddlString);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex}");
        }
    }
}

public class MainWindowViewModel : ReactiveObject
{
    public MainWindowViewModel()
    {
        this.Documents =
            [
                new DocModel("DocumentViewer", () => SampleDocuments.CreateSample1()),
                new DocModel("HelloMigraDoc", () => SampleDocuments.CreateSample2()),
                new DocModel("Invoice", () => SampleDocuments.CreateSample3()),
                new DocModel("Images", () => SampleDocuments.CreateSample4())
            ];
    }

    public DocModel[] Documents { get; set; }
}

public class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
