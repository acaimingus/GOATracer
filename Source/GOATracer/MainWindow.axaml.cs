using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GOATracer.Descriptions;
using GOATracer.Importer.Obj;
using GOATracer.Preview;

namespace GOATracer;

public partial class MainWindow : Window
{
    private SceneDescription? _currentSceneDescription;
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ImportOptionClicked(object? sender, RoutedEventArgs e)
    {
        // Get the parent window to enable file dialog access
        var topLevel = TopLevel.GetTopLevel(this);

        // Show file picker dialog to let user select a .obj file to import
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
            string? filePath = files[0].Path?.LocalPath;

            // Set Import information in Log
            LogOutputTextBlock.Text += $"Importing scene from {filePath}...\n";
            // Invalidate the visual and force redraw so the message shows up
            LogOutputTextBlock.InvalidateVisual();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);
            
            // Import the .obj file and convert it into our scene data structure
            var objImporter = new ObjImporter();
            _currentSceneDescription = objImporter.ImportModel(filePath);
            
            // Set the import stats
            LoadedFilePathLabel.Content = $"Loaded file: { filePath }";
            FileSizeLabel.Content = $"File size: { new FileInfo(filePath).Length } bytes";
            VertexCountLabel.Content = $"Vertex count: {_currentSceneDescription.VertexPoints?.Count ?? 0}";
            MaterialCountLabel.Content = "Material count: 0";
            
            // Output detailed information about the imported 3D model for debugging
            PrintDebugInfo(_currentSceneDescription);
        }
    }

    private void PrintDebugInfo(SceneDescription sceneDescription)
    {
        var stringBuilder = new  StringBuilder();
        
        stringBuilder.AppendLine("=== SCENE DESCRIPTION ===");
        stringBuilder.AppendLine($"File Name: {sceneDescription.FileName}");
        stringBuilder.AppendLine($"Total Vertices: {sceneDescription.VertexPoints?.Count ?? 0}");
        stringBuilder.AppendLine($"Total Objects: {sceneDescription.ObjectDescriptions?.Count ?? 0}");

        // Print vertex points
        stringBuilder.AppendLine("\n--- VERTEX POINTS ---");
        if (sceneDescription.VertexPoints != null)
        {
            for (int i = 0; i < sceneDescription.VertexPoints.Count; i++)
            {
                var coords = sceneDescription.VertexPoints[i].GetCoordinates();
                stringBuilder.AppendLine($"Vertex {i + 1}: ({coords[0]:F3}, {coords[1]:F3}, {coords[2]:F3})");
            }
        }

        // Print object descriptions
        stringBuilder.AppendLine("\n--- OBJECTS ---");
        if (sceneDescription.ObjectDescriptions != null)
        {
            for (int i = 0; i < sceneDescription.ObjectDescriptions.Count; i++)
            {
                var obj = sceneDescription.ObjectDescriptions[i];
                stringBuilder.AppendLine($"Object {i + 1}: {obj.objectName ?? "Unnamed"}");
                stringBuilder.AppendLine($"  Faces: {obj.FacePoints?.Count ?? 0}");

                // Print face indices
                if (obj.FacePoints != null)
                {
                    for (int j = 0; j < obj.FacePoints.Count; j++)
                    {
                        var face = obj.FacePoints[j];
                        var indicesStr = string.Join(", ", face.Indices);
                        stringBuilder.AppendLine($"    Face {j + 1}: [{indicesStr}]");
                    }
                }
            }
        }
        stringBuilder.AppendLine("=== END DESCRIPTION ===\n");
        
        LogOutputTextBlock.Text = stringBuilder.ToString();
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
        if (_currentSceneDescription != null)
        {
            var cam = new Camera
            {
                Position = new Vector3(Convert.ToSingle(XPositionTextBox.Text, CultureInfo.InvariantCulture), Convert.ToSingle(YPositionTextBox.Text, CultureInfo.InvariantCulture), Convert.ToSingle(ZPositionTextBox.Text, CultureInfo.InvariantCulture)),
                Rotation = new Vector3(Convert.ToSingle(XRotationTextBox.Text, CultureInfo.InvariantCulture), Convert.ToSingle(YRotationTextBox.Text, CultureInfo.InvariantCulture), Convert.ToSingle(ZRotationTextBox.Text, CultureInfo.InvariantCulture)),
                Aspect = Convert.ToSingle(ImageWidthTextBox.Text) / Convert.ToSingle(ImageHeightTextBox.Text)  
            };

            RenderPreview.SetScene(_currentSceneDescription, cam);
            RenderPreview.RequestNextFrameRendering();

            // RenderResult.Source = SimpleRenderer.RenderWireframe(_currentSceneDescription, cam, Convert.ToInt32(ImageWidthTextBox.Text), Convert.ToInt32(ImageHeightTextBox.Text));
            // new preview:
            // RenderResult.Source = PreviewRenderer.Render(_currentSceneDescription, cam, Convert.ToInt32(ImageWidthTextBox.Text), Convert.ToInt32(ImageHeightTextBox.Text));
        }
        else
        {
            Console.WriteLine("No scene loaded.");
        }
    }
}