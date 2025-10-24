using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GOATracer.Importer.Obj;
using GOATracer.Preview;

namespace GOATracer;

/// <summary>
/// Code behind for the main window.
/// </summary>
public partial class MainWindow : Window
{
    private ImportedSceneDescription? _currentSceneDescription;
    private readonly Camera _previewCamera = new();
    
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
            LoadedFilePathLabel.Content = $"Loaded file: { filePath }";
            FileSizeLabel.Content = $"File size: { new FileInfo(filePath).Length } bytes";
            VertexCountLabel.Content = $"Vertex count: { sceneDescription.VertexPoints.Count }";
            MaterialCountLabel.Content = $"Material count: { sceneDescription.Materials?.Count ?? 0 }";
            
            // Output detailed information about the imported 3D model for debugging
            PrintDebugInfo(sceneDescription);
            
            RenderPreview.SetScene(sceneDescription, _previewCamera);
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
        logBuilder.AppendLine($"--- VERTICES ({ importedSceneDescription.VertexPoints.Count }) ---");

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
            for (int i = 0; i < importedSceneDescription.NormalPoints.Count; i++)
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
            for (int i = 0; i < importedSceneDescription.TexturePoints.Count; i++)
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
            for (int objIndex = 0; objIndex < importedSceneDescription.ObjectDescriptions.Count; objIndex++)
            {
                var obj = importedSceneDescription.ObjectDescriptions[objIndex];
                logBuilder.AppendLine($"Object {objIndex}: {obj.ObjectName}");
                logBuilder.AppendLine($"  - Faces: {obj.FacePoints.Count}");
                
                // Log each face in the object
                for (int faceIndex = 0; faceIndex < obj.FacePoints.Count; faceIndex++)
                {
                    var face = obj.FacePoints[faceIndex];
                    logBuilder.Append($"    Face {faceIndex}: Material={face.Material}, Vertices=[");
                    
                    // Log each vertex in the face
                    for (int vIndex = 0; vIndex < face.Indices.Count; vIndex++)
                    {
                        var vertex = face.Indices[vIndex];
                        logBuilder.Append($"(v:{vertex.VertexIndex}");
                        
                        if (vertex.TextureIndex.HasValue)
                            logBuilder.Append($", vt:{vertex.TextureIndex}");
                        
                        if (vertex.NormalIndex.HasValue)
                            logBuilder.Append($", vn:{vertex.NormalIndex}");
                        
                        logBuilder.Append(")");
                        
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
        try
        {
            _previewCamera.Position = new Vector3(
                Convert.ToSingle(XPositionTextBox.Text, CultureInfo.InvariantCulture),
                Convert.ToSingle(YPositionTextBox.Text, CultureInfo.InvariantCulture),
                Convert.ToSingle(ZPositionTextBox.Text, CultureInfo.InvariantCulture));

            _previewCamera.Rotation = new Vector3(
                Convert.ToSingle(XRotationTextBox.Text, CultureInfo.InvariantCulture),
                Convert.ToSingle(YRotationTextBox.Text, CultureInfo.InvariantCulture),
                Convert.ToSingle(ZRotationTextBox.Text, CultureInfo.InvariantCulture));
            RenderPreview.SetCamera(_previewCamera);
        }
        catch(FormatException fe)
        {
            Console.WriteLine("Invalid number format in camera settings.");
        }
    }
}