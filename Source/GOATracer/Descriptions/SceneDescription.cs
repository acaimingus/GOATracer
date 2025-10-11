using System.Collections.Generic;

namespace GOATracer.Descriptions;

/// <summary>
/// Represents the complete 3D scene imported from a .obj file, containing all geometry data
/// needed for rendering. Acts as the root container that holds all vertices and objects
/// that make up the entire 3D scene.
/// </summary>
public class SceneDescription
{
    /// <summary>
    /// The original filename of the imported .obj file, used for identification and debugging.
    /// This helps track which file this scene data came from.
    /// </summary>
    public readonly string? FileName;
    
    /// <summary>
    /// Collection of all 3D objects in the scene, each containing their own faces and geometry.
    /// Objects are typically named groups within the .obj file (e.g., "Cube", "Sphere").
    /// </summary>
    public List<ObjectDescription>? ObjectDescriptions;
    
    /// <summary>
    /// Master list of all 3D vertex points (coordinates) used by all objects in the scene.
    /// Objects reference these vertices by index to define their faces and shapes.
    /// This shared vertex pool avoids duplication when multiple objects share the same points.
    /// </summary>
    public List<VertexPoint>? VertexPoints;

    /// <summary>
    /// Creates a new scene container and initializes empty collections for vertices and objects.
    /// Ready to be populated with 3D geometry data during the import process.
    /// </summary>
    /// <param name="fileName">Name of the source .obj file being imported</param>
    public SceneDescription(string fileName)
    {
        FileName = fileName;
        VertexPoints = new List<VertexPoint>();
        ObjectDescriptions = new List<ObjectDescription>();
    }
    
    
}