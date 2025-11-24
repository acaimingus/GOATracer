namespace GOATracer.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GOATracer.ViewModels;
using GOATracer.Importer.Obj;
using System;
using System.IO;
using System.Text;

public partial class MainWindow : Window
{
    // Constructor initializes the MainWindow and sets its DataContext to MainWindowViewModel, start of the program
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new MainWindowViewModel();
    }
    // Event handler for the Exit menu option, closes the application when clicked
    private void ExitOptionClicked(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
    /// <summary>
    /// Handler method for the Import option
    /// </summary>
    /// <param name="sender">Import option in the menu bar at the top</param>
    /// <param name="eventData">Event data</param>
    private async void ImportOptionClicked(object? sender, RoutedEventArgs eventData)
    {
        // Get the parent window to enable file dialog access
        // Source: https://docs.avaloniaui.net/docs/basics/user-interface/file-dialogs
        var topLevel = TopLevel.GetTopLevel(this);

        // Show file picker dialog to let user select a .obj file to import
        // Source: https://docs.avaloniaui.net/docs/concepts/services/storage-provider/file-picker-options
        var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open .obj File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType(".obj files")
                {
                    Patterns = ["*.obj"]
                }
            ]
        });

        if (files.Count >= 1)
        {
            // Extract the local file path from the selected file
            var filePath = files[0].Path.LocalPath;
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);
            // Import the .obj file and convert it into our scene data structure
            var sceneDescription = ObjImporter.ImportModel(filePath);
        }
    }
}
