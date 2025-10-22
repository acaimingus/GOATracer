using System.Collections.Generic;
using System.Numerics;

namespace GOATracer.Importer.Obj;

/// <summary>
/// Represents the complete 3D scene imported from a .obj file, containing all geometry data
/// needed for rendering. Acts as the root container that holds all elements that make up the entire 3D scene.
/// Source: https://paulbourke.net/dataformats/obj/obj_spec.pdf
/// Source: https://paulbourke.net/dataformats/mtl/
/// </summary>
public class ImportedSceneDescription
{
    /// <summary>
    /// The original filename of the imported .obj file.
    /// </summary>
    public string? FileName { get; }

    /// <summary>
    /// Collection of all 3D objects in the scene, each containing their own faces and geometry.
    /// Objects are typically named groups within the .obj file (e.g., "Cube", "Sphere").
    /// </summary>
    public List<ObjectDescription>? ObjectDescriptions { get; }

    /// <summary>
    /// Master list of all vertex points in the scene describing the points of the geometry.
    /// </summary>
    public List<Vector3> VertexPoints { get; }

    /// <summary>
    /// Master list of all normal points in the scene describing the normals of the geometry.
    /// </summary>
    public List<Vector3> NormalPoints { get; }

    /// <summary>
    /// Master list of all texture points in the scene describing the mapping of images to the geometry.
    /// </summary>
    public List<Vector3> TexturePoints { get; }
    
    /// <summary>
    /// Master list of all materials available within the scene imported from one or multiple .mtl files.
    /// </summary>
    public Dictionary<string, ObjectMaterial> Materials { get; }

    /// <summary>
    /// Creates a new scene container and initializes empty collections for vertices and objects.
    /// </summary>
    /// <param name="fileName">Name of the source .obj file being imported</param>
    public ImportedSceneDescription(string fileName)
    {
        FileName = fileName;
        VertexPoints = new List<Vector3>();
        NormalPoints = new List<Vector3>();
        TexturePoints = new List<Vector3>();
        ObjectDescriptions = new List<ObjectDescription>();
        Materials = new Dictionary<string, ObjectMaterial>();
    }
}