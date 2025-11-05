using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GOATracer.Importer.Obj;
using GOATracer.Preview;
using GOATracer.Lights;
using Window = Avalonia.Controls.Window;

namespace GOATracer;

/// <summary>
/// Code behind for the main window.
/// </summary>
public partial class MainWindow : Window
{
    private bool _mouseLookActive;
    /// <summary>
    /// List to manage all the added lights to a scene
    /// </summary>
    private List<LightControl> _sceneLightList;
    /// <summary>
    /// Counter variable for giving each new light an ID, only goes up
    /// </summary>
    private int _nextLightId;
    /// <summary>
    /// Variable storing the imported scene description, only set if an import was made, else null
    /// </summary>
    private ImportedSceneDescription? _sceneDescription;

    private PreviewRenderer _previewRenderer;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        _mouseLookActive = false;
        // Initialize the list for the lights of the scene
        _sceneLightList = new List<LightControl>();
        // Start the counter for the next light ID at 1
        _nextLightId = 1;
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
            _sceneDescription = ObjImporter.ImportModel(filePath);

            // Set the import stats
            LoadedFilePathLabel.Content = $"Loaded file: {filePath}";
            FileSizeLabel.Content = $"File size: {new FileInfo(filePath).Length} bytes";
            VertexCountLabel.Content = $"Vertex count: {_sceneDescription.VertexPoints.Count}";
            MaterialCountLabel.Content = $"Material count: {_sceneDescription.Materials?.Count ?? 0}";

            // Output detailed information about the imported 3D model for debugging
            PrintDebugInfo();

            // Check if a scene was imported
            if (_sceneDescription != null)
            {
                // Collect the lights specified by the user
                var lights = _sceneLightList.Select(sceneLight => sceneLight.LightData).ToList();
                _previewRenderer = new PreviewRenderer(_sceneDescription, lights);
                // Kill previous renderer if present (GC cleanup) and create a new renderer
                RenderPanel.Children.Clear();
                RenderPanel.Children.Add(_previewRenderer);
            }
        }
    }

    private void DoLightsUpdate()
    {
        // Check if a scene is even loaded
        if (_sceneDescription != null)
        {
            // Collect the lights specified by the user
            var lights = _sceneLightList.Select(sceneLight => sceneLight.LightData).ToList();
            _previewRenderer.UpdateLights(lights);
        }
    }

    /// <summary>
    /// Debug method for printing out the imported data structure
    /// </summary>
    private void PrintDebugInfo()
    {
        // Create a StringBuilder to efficiently build the log content
        var logBuilder = new StringBuilder();

        // Add scene name and basic information
        if (_sceneDescription != null)
        {
            logBuilder.AppendLine($"=== SCENE: {_sceneDescription.FileName} ===");
            logBuilder.AppendLine();

            // Log vertex data
            logBuilder.AppendLine($"--- VERTICES ({_sceneDescription.VertexPoints.Count}) ---");

            for (var i = 0; i < _sceneDescription.VertexPoints.Count; i++)
            {
                var vertex = _sceneDescription.VertexPoints[i];
                logBuilder.AppendLine($"v[{i}]: ({vertex.X}, {vertex.Y}, {vertex.Z})");
            }

            logBuilder.AppendLine();

            // Log normal data
            logBuilder.AppendLine($"--- NORMALS ({_sceneDescription.NormalPoints?.Count ?? 0}) ---");
            if (_sceneDescription.NormalPoints != null)
            {
                for (var i = 0; i < _sceneDescription.NormalPoints.Count; i++)
                {
                    var normal = _sceneDescription.NormalPoints[i];
                    logBuilder.AppendLine($"vn[{i}]: ({normal.X}, {normal.Y}, {normal.Z})");
                }
            }

            logBuilder.AppendLine();

            // Log texture coordinates
            logBuilder.AppendLine($"--- TEXTURE COORDS ({_sceneDescription.TexturePoints?.Count ?? 0}) ---");
            if (_sceneDescription.TexturePoints != null)
            {
                for (var i = 0; i < _sceneDescription.TexturePoints.Count; i++)
                {
                    var tex = _sceneDescription.TexturePoints[i];
                    logBuilder.AppendLine($"vt[{i}]: ({tex.X}, {tex.Y}, {tex.Z})");
                }
            }

            logBuilder.AppendLine();

            // Log materials
            logBuilder.AppendLine($"--- MATERIALS ({_sceneDescription.Materials?.Count ?? 0}) ---");
            if (_sceneDescription.Materials != null)
            {
                foreach (var material in _sceneDescription.Materials)
                {
                    logBuilder.AppendLine($"Material: {material.Key}");
                    logBuilder.AppendLine(
                        $"  - Diffuse: ({material.Value.ColorDiffuse?.X}, {material.Value.ColorDiffuse?.Y}, {material.Value.ColorDiffuse?.Z})");
                    logBuilder.AppendLine(
                        $"  - Ambient: ({material.Value.ColorAmbient?.X}, {material.Value.ColorAmbient?.Y}, {material.Value.ColorAmbient?.Z})");
                    logBuilder.AppendLine(
                        $"  - Specular: ({material.Value.ColorSpecular?.X}, {material.Value.ColorSpecular?.Y}, {material.Value.ColorSpecular?.Z})");
                    logBuilder.AppendLine($"  - Specular Exponent: {material.Value.SpecularExponent}");
                    logBuilder.AppendLine($"  - Optical Density: {material.Value.OpticalDensity}");
                    logBuilder.AppendLine($"  - Dissolve: {material.Value.Dissolve}");
                    logBuilder.AppendLine($"  - Illumination Model: {material.Value.IlluminationModel}");
                    logBuilder.AppendLine($"  - Diffuse texture: {material.Value.DiffuseTexture}");
                }
            }

            logBuilder.AppendLine();

            // Log objects
            logBuilder.AppendLine($"--- OBJECTS ({_sceneDescription.ObjectDescriptions?.Count ?? 0}) ---");
            if (_sceneDescription.ObjectDescriptions != null)
            {
                for (var objIndex = 0; objIndex < _sceneDescription.ObjectDescriptions.Count; objIndex++)
                {
                    var obj = _sceneDescription.ObjectDescriptions[objIndex];
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
        else
        {
            LogOutputTextBlock.Text += "ERROR: The imported scene was NULL!";
        }
    }

    /// <summary>
    /// Handler for the menu option for exiting the program
    /// </summary>
    /// <param name="sender">"Exit" menu bar item</param>
    /// <param name="e">event data</param>
    private void ExitOptionClicked(object? sender, RoutedEventArgs e)
    {
        // Close the app
        Close();
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

    /// <summary>
    /// Event handler for adding a new light control to the UI
    /// </summary>
    /// <param name="sender">"Add a light"-Button</param>
    /// <param name="eventData">Event data</param>
    private void AddLightButtonClicked(object? sender, RoutedEventArgs eventData)
    {
        // Create a callback for the delete event of a light control
        var deleteCallback = RemoveALight;
        var editCallback = DoLightsUpdate;
        // Create a new light control with an ID and the callback what method to call for deletion
        var newLight = new LightControl(_nextLightId, deleteCallback, editCallback);
        // Add the light control class to the managing list
        _sceneLightList.Add(newLight);
        // Get the StackPanel to add the control to and add it
        var container = this.FindControl<StackPanel>("SceneLightList");
        container?.Children.Add(newLight.Control);
        // Increment the ID for the next light (this only goes up)
        _nextLightId++;
    }

    /// <summary>
    /// Event handler for when a light control gets removed from the UI, is called per Callback
    /// </summary>
    /// <param name="lightId">ID of the light to remove</param>
    private void RemoveALight(int lightId)
    {
        // Find the specified light ID in the list and unlink it
        var lightToRemove = _sceneLightList.FirstOrDefault(light => light.LightData.Id == lightId);
        if (lightToRemove != null)
        {
            _sceneLightList.Remove(lightToRemove);
        }
    }
}