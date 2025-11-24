namespace GOATracer.Views;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GOATracer.Cameras;
using GOATracer.Importer.Obj;
using GOATracer.MVC;
using GOATracer.Preview;
using GOATracer.ViewModels;
using System.Linq;

public partial class MainWindow : Window
{
    private PreviewRenderer _previewRenderer;
    private bool _mouseLookActive = false;

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

            // Check if a scene was imported
            if (sceneDescription != null)
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    var cameraSettings = new CameraSettingsBinding();

                    // Collect the lights specified by the user
                    var lights = vm.EnabledLights.Select(sceneLight => sceneLight.Model).ToList();
                    _previewRenderer = new PreviewRenderer(sceneDescription, lights, cameraSettings);
                    // Kill previous renderer if present (GC cleanup) and create a new renderer
                    RenderPanel.Children.Clear();
                    RenderPanel.Children.Add(_previewRenderer);
                }
            }
        }
    }

    /// <summary>
    /// Event handler for when a mouse button is pressed
    /// </summary>
    /// <param name="sender">Mouse button pressed</param>
    /// <param name="eventData">Event data</param>
    private void RenderPanel_PointerPressed(object? sender, PointerPressedEventArgs eventData)
    {
        // Exit the method if there is no preview renderer
        if (RenderPanel.Children.Count == 0)
        {
            return;
        }

        // Get the preview renderer
        var previewRenderer = (PreviewRenderer)RenderPanel.Children[0];
        previewRenderer?.Focus();

        // Check if the left mouse button is pressed
        var pt = eventData.GetCurrentPoint(RenderPanel);
        if (pt.Properties.IsLeftButtonPressed)
        {
            // Set the flag for mouse look
            _mouseLookActive = true;
            // Reset the mouse look flags in the preview renderer to eliminate mouse jump
            previewRenderer?.ResetMouseLook();
            // Capture the click
            eventData.Pointer.Capture(RenderPanel);
        }
    }

    /// <summary>
    /// Event handler for when mouse buttons are released for the preview panel
    /// </summary>
    /// <param name="sender">Released mouse button</param>
    /// <param name="eventData">Event data</param>
    private void RenderPanel_PointerReleased(object? sender, PointerReleasedEventArgs eventData)
    {
        if (eventData.InitialPressMouseButton == MouseButton.Left)
        {
            // Disable looking with the mouse in the preview
            _mouseLookActive = false;
        }
    }

    /// <summary>
    /// Event handler for mouse movements
    /// </summary>
    /// <param name="sender">Mouse movement</param>
    /// <param name="eventData">Event data</param>
    private void RenderPanel_PointerMoved(object? sender, PointerEventArgs eventData)
    {
        // If mouse look is not active or there is 
        if (!_mouseLookActive || RenderPanel.Children.Count == 0) return;

        // Get the position of the mouse
        var pos = eventData.GetPosition(RenderPanel);

        var previewRenderer = (PreviewRenderer)RenderPanel.Children[0];
        // Use the mouselook
        previewRenderer.ApplyMouseLook((float)pos.X, (float)pos.Y);
    }
}
