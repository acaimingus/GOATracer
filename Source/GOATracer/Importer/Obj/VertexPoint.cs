using System.Numerics;

namespace GOATracer.Importer.Obj;

/// <summary>
/// Represents a single point in 3D space that defines part of a 3D model's geometry.
/// </summary>
public class VertexPoint
{
    /// <summary>
    /// 3-dimensional vector describing the [x, y, z] coordinates of the point.
    /// </summary>
    public readonly Vector3 Coordinates;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="vector">Vector3 containing [x, y, z] coordinates in 3D world space</param>
    public VertexPoint(Vector3 vector)
    {
        Coordinates = vector;
    }
}