using System.Collections.Generic;

namespace GOATracer.Descriptions;

/// <summary>
/// Represents a single 3D object within a scene, containing its name and geometric surface data.
/// Each object consists of multiple faces/polygons that define its shape and appearance.
/// </summary>
public class ObjectDescription
{
    /// <summary>
    /// The human-readable name of this 3D object as defined in the .obj file.
    /// This corresponds to the "o ObjectName" lines in Wavefront .obj format.
    /// Will be null if no object name was specified in the file.
    /// </summary>
    public string? objectName;
    
    /// <summary>
    /// Collection of all faces (triangles/polygons) that make up this object's surface geometry.
    /// Each face defines which vertices should be connected to form part of the object's mesh.
    /// </summary>
    public List<FaceDescription> FacePoints;

    /// <summary>
    /// Initializes a new 3D object with an empty face list, ready to have geometry data added to it.
    /// </summary>
    public ObjectDescription()
    {
        FacePoints = new List<FaceDescription>();
    }
}