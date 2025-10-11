using System.Collections.Generic;

namespace GOATracer.Descriptions;

/// <summary>
/// Represents a polygon face in 3D space by defining which vertices are connected together.
/// Each face is typically a triangle or quad that forms part of a 3D object's surface.
/// </summary>
public class FaceDescription
{
    /// <summary>
    /// List of vertex indices that define this face/polygon.
    /// These indices reference positions in the scene's master vertex list.
    /// For example: [1, 2, 3] means connect vertex 1 to vertex 2 to vertex 3 to form a triangle.
    /// </summary>
    public List<int> Indices;

    /// <summary>
    /// Creates a new face definition with the specified vertex connections
    /// </summary>
    /// <param name="indices">The vertex indices that should be connected to form this face</param>
    public FaceDescription(List<int> indices)
    {
        this.Indices = indices;
    }
}