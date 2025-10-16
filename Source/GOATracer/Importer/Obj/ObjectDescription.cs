using System.Collections.Generic;

namespace GOATracer.Importer.Obj;

/// <summary>
/// Represents a single 3D object within a scene, containing its name and geometric surface data.
/// </summary>
public class ObjectDescription
{
    /// <summary>
    /// The human-readable name of this 3D object as defined in the .obj file.
    /// This corresponds to the "o ObjectName" lines in Wavefront .obj format.
    /// Will be null if no object name was specified in the file.
    /// </summary>
    public string? ObjectName;
    
    /// <summary>
    /// Collection of all faces (triangles/polygons) that make up this object's surface geometry.
    /// </summary>
    public readonly List<ObjectFace> FacePoints;

    /// <summary>
    /// Initializes a new 3D object with an empty face list, ready to have geometry data added to it.
    /// </summary>
    public ObjectDescription()
    {
        FacePoints = new List<ObjectFace>();
    }
}