using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GOATracer.Importer.Obj;

namespace GOATracer;

public partial class MainWindow : Window
{
    /// <summary>
    /// Variable storing the imported scene description, only set if an import was made, else null
    /// </summary>
    private ImportedSceneDescription? _sceneDescription;
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ExitOptionClicked(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }



    private void RenderClick(object? sender, RoutedEventArgs e)
    {
        var renderWindow = new RenderWindow();
        renderWindow.Show();
    }

    /// <summary>
    /// Handler method for the Import option
    /// </summary>
    /// <param name="sender">Import option in the menu bar at the top</param>
    /// <param name="eventData">Event data</param>
    private async void FileChooser_Click(object? sender, RoutedEventArgs eventData)
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
            _sceneDescription = ObjImporter.ImportModel(filePath);

            // Output detailed information about the imported 3D model for debugging
            PrintDebugInfo();
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
        }
    }
}
