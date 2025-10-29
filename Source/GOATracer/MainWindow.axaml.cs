using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GOATracer.Importer.Obj;
using GOATracer.Preview;
using Window = Avalonia.Controls.Window;

namespace GOATracer;

/// <summary>
/// Code behind for the main window.
/// </summary>
public partial class MainWindow : Window
{
    private bool _mouseLookActive = false;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
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
            // Clear the last output in the log
            LogOutputTextBlock.Text = "";

            // Extract the local file path from the selected file
            var filePath = files[0].Path.LocalPath;

            // Set Import information in Log
            LogOutputTextBlock.Text += $"Importing scene from {filePath}...\n";
            // Invalidate the visual and force redraw so the message shows up
            LogOutputTextBlock.InvalidateVisual();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);

            // Import the .obj file and convert it into our scene data structure
            var sceneDescription = ObjImporter.ImportModel(filePath);

            // Set the import stats
            LoadedFilePathLabel.Content = $"Loaded file: {filePath}";
            FileSizeLabel.Content = $"File size: {new FileInfo(filePath).Length} bytes";
            VertexCountLabel.Content = $"Vertex count: {sceneDescription.VertexPoints.Count}";
            MaterialCountLabel.Content = $"Material count: {sceneDescription.Materials?.Count ?? 0}";

            // Output detailed information about the imported 3D model for debugging
            PrintDebugInfo(sceneDescription);
            
            // Load the preview for the imported object
            var previewRenderer = new PreviewRenderer(sceneDescription);
            RenderPanel.Children.Clear(); // kill previous renderer if any
            RenderPanel.Children.Add(previewRenderer);
        }
    }

    /// <summary>
    /// Debug method for printing out the imported data structure
    /// </summary>
    /// <param name="importedSceneDescription"></param>
    private void PrintDebugInfo(ImportedSceneDescription importedSceneDescription)
    {
        // Create a StringBuilder to efficiently build the log content
        var logBuilder = new StringBuilder();

        // Add scene name and basic information
        logBuilder.AppendLine($"=== SCENE: {importedSceneDescription.FileName} ===");
        logBuilder.AppendLine();

        // Log vertex data
        logBuilder.AppendLine($"--- VERTICES ({importedSceneDescription.VertexPoints.Count}) ---");

        for (var i = 0; i < importedSceneDescription.VertexPoints.Count; i++)
        {
            var vertex = importedSceneDescription.VertexPoints[i];
            logBuilder.AppendLine($"v[{i}]: ({vertex.X}, {vertex.Y}, {vertex.Z})");
        }

        logBuilder.AppendLine();

        // Log normal data
        logBuilder.AppendLine($"--- NORMALS ({importedSceneDescription.NormalPoints?.Count ?? 0}) ---");
        if (importedSceneDescription.NormalPoints != null)
        {
            for (var i = 0; i < importedSceneDescription.NormalPoints.Count; i++)
            {
                var normal = importedSceneDescription.NormalPoints[i];
                logBuilder.AppendLine($"vn[{i}]: ({normal.X}, {normal.Y}, {normal.Z})");
            }
        }
        logBuilder.AppendLine();

        // Log texture coordinates
        logBuilder.AppendLine($"--- TEXTURE COORDS ({importedSceneDescription.TexturePoints?.Count ?? 0}) ---");
        if (importedSceneDescription.TexturePoints != null)
        {
            for (var i = 0; i < importedSceneDescription.TexturePoints.Count; i++)
            {
                var tex = importedSceneDescription.TexturePoints[i];
                logBuilder.AppendLine($"vt[{i}]: ({tex.X}, {tex.Y}, {tex.Z})");
            }
        }
        logBuilder.AppendLine();

        // Log materials
        logBuilder.AppendLine($"--- MATERIALS ({importedSceneDescription.Materials?.Count ?? 0}) ---");
        if (importedSceneDescription.Materials != null)
        {
            foreach (var material in importedSceneDescription.Materials)
            {
                logBuilder.AppendLine($"Material: {material.Key}");
                logBuilder.AppendLine($"  - Diffuse: ({material.Value.ColorDiffuse?.X}, {material.Value.ColorDiffuse?.Y}, {material.Value.ColorDiffuse?.Z})");
                logBuilder.AppendLine($"  - Ambient: ({material.Value.ColorAmbient?.X}, {material.Value.ColorAmbient?.Y}, {material.Value.ColorAmbient?.Z})");
                logBuilder.AppendLine($"  - Specular: ({material.Value.ColorSpecular?.X}, {material.Value.ColorSpecular?.Y}, {material.Value.ColorSpecular?.Z})");
                logBuilder.AppendLine($"  - Specular Exponent: {material.Value.SpecularExponent}");
                logBuilder.AppendLine($"  - Optical Density: {material.Value.OpticalDensity}");
                logBuilder.AppendLine($"  - Dissolve: {material.Value.Dissolve}");
                logBuilder.AppendLine($"  - Illumination Model: {material.Value.IlluminationModel}");
                logBuilder.AppendLine($"  - Diffuse texture: {material.Value.DiffuseTexture}");
            }
        }
        logBuilder.AppendLine();

        // Log objects
        logBuilder.AppendLine($"--- OBJECTS ({importedSceneDescription.ObjectDescriptions?.Count ?? 0}) ---");
        if (importedSceneDescription.ObjectDescriptions != null)
        {
            for (var objIndex = 0; objIndex < importedSceneDescription.ObjectDescriptions.Count; objIndex++)
            {
                var obj = importedSceneDescription.ObjectDescriptions[objIndex];
                logBuilder.AppendLine($"Object {objIndex}: {obj.ObjectName}");
                logBuilder.AppendLine($"  - Faces: {obj.FacePoints.Count}");

                // Log each face in the object
                for (var faceIndex = 0; faceIndex < obj.FacePoints.Count; faceIndex++)
                {
                    var face = obj.FacePoints[faceIndex];
                    logBuilder.Append($"    Face {faceIndex}: Material={face.Material}, Vertices=[");

                    // Log each vertex in the face
                    for (var vIndex = 0; vIndex < face.Indices.Count; vIndex++)
                    {
                        var vertex = face.Indices[vIndex];
                        logBuilder.Append($"(v:{vertex.VertexIndex}");

                        if (vertex.TextureIndex.HasValue)
                            logBuilder.Append($", vt:{vertex.TextureIndex}");

                        if (vertex.NormalIndex.HasValue)
                            logBuilder.Append($", vn:{vertex.NormalIndex}");

                        logBuilder.Append(')');

                        if (vIndex < face.Indices.Count - 1)
                            logBuilder.Append(", ");
                    }
                    logBuilder.AppendLine("]");
                }
                logBuilder.AppendLine();
            }
        }

        // Write the log file to the executable directory
        File.WriteAllText("import.log", logBuilder.ToString());

        // Also log to the UI that the debug info was saved
        LogOutputTextBlock.Text += $"\nDebug info written.";
    }

    /// <summary>
    /// Handler for the menu option for exiting the program
    /// </summary>
    /// <param name="sender">"Exit" menu bar item</param>
    /// <param name="e">event data</param>
    private void ExitOptionClicked(object? sender, RoutedEventArgs e)
    {
        // Close the app
        this.Close();
    }

    private void RenderButtonClicked(object? sender, RoutedEventArgs e)
    {
        // From here the raytracer component will be called
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

    private void RenderPanel_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_mouseLookActive || RenderPanel.Children.Count == 0) return;

        var pos = e.GetPosition(RenderPanel);

        var previewRenderer = (PreviewRenderer)RenderPanel.Children[0];
        previewRenderer.ApplyMouseLook((float)pos.X, (float)pos.Y);
    }
}