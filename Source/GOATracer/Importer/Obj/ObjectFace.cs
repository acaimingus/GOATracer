using System.Collections.Generic;

namespace GOATracer.Importer.Obj;

/// <summary>
/// Represents a polygon face in 3D space by defining which vertices are connected together.
/// Each face is typically a triangle or quad that forms part of a 3D object's surface.
/// </summary>
public class ObjectFace
{
    /// <summary>
    /// List of vertex indices that define this face/polygon.
    /// </summary>
    public readonly List<int> Indices;

    /// <summary>
    /// Constructor: Creates a new face definition with the specified vertex connections
    /// </summary>
    /// <param name="indices">The vertex indices that should be connected to form this face</param>
    public ObjectFace(List<int> indices)
    {
        this.Indices = indices;
    }
}