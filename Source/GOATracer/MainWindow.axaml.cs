using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using GOATracer.Importer.Obj;

namespace GOATracer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private async void ImportButtonClicked(object? sender, RoutedEventArgs e)
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
            // Extract the local file path from the selected file
            string? filePath = files[0].Path?.LocalPath;
            
            // Import the .obj file and convert it into our scene data structure
            var objImporter = new ObjImporter();
            var sceneDescription = objImporter.ImportModel(filePath);
            
            // Output detailed information about the imported 3D model for debugging
            Console.WriteLine("=== SCENE DESCRIPTION ===");
            Console.WriteLine($"File Name: {sceneDescription.FileName}");
            Console.WriteLine($"Total Vertices: {sceneDescription.VertexPoints?.Count ?? 0}");
            Console.WriteLine($"Total Objects: {sceneDescription.ObjectDescriptions?.Count ?? 0}");
            
            // Print vertex points
            Console.WriteLine("\n--- VERTEX POINTS ---");
            if (sceneDescription.VertexPoints != null)
            {
                for (int i = 0; i < sceneDescription.VertexPoints.Count; i++)
                {
                    var coords = sceneDescription.VertexPoints[i].GetCoordinates();
                    Console.WriteLine($"Vertex {i + 1}: ({coords[0]:F3}, {coords[1]:F3}, {coords[2]:F3})");
                }
            }
            
            // Print object descriptions
            Console.WriteLine("\n--- OBJECTS ---");
            if (sceneDescription.ObjectDescriptions != null)
            {
                for (int i = 0; i < sceneDescription.ObjectDescriptions.Count; i++)
                {
                    var obj = sceneDescription.ObjectDescriptions[i];
                    Console.WriteLine($"Object {i + 1}: {obj.objectName ?? "Unnamed"}");
                    Console.WriteLine($"  Faces: {obj.FacePoints?.Count ?? 0}");
                    
                    // Print face indices
                    if (obj.FacePoints != null)
                    {
                        for (int j = 0; j < obj.FacePoints.Count; j++)
                        {
                            var face = obj.FacePoints[j];
                            var indicesStr = string.Join(", ", face.Indices);
                            Console.WriteLine($"    Face {j + 1}: [{indicesStr}]");
                        }
                    }
                }
            }
            Console.WriteLine("=== END DESCRIPTION ===\n");
            
        }
    }
}