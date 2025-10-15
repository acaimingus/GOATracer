using System.Numerics;

namespace GOATracer.Descriptions;

/// <summary>
/// Represents a single point in 3D space that defines part of a 3D model's geometry.
/// Vertices are the building blocks that get connected by faces to form the surface of 3D objects.
/// Immutable once created to ensure coordinate data integrity during ray tracing calculations.
/// </summary>
/// <param name="vector">Vector3 containing [x, y, z] coordinates in 3D world space</param>
public class VertexPoint(Vector3 vector)
{
    /// <summary>
    /// 3-dimensional vector describing the [x, y, z] coordinates of the point.
    /// </summary>
    private readonly Vector3 _coordinates = vector; 
        
    /// <summary>
    /// Method to query the coordinates of the vertex point.
    /// </summary>
    /// <returns>[x, y, z] coordinates of the vertex point.</returns>
    public Vector3 GetCoordinates()
    {
        return _coordinates;
    }
}