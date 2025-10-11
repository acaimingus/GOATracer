namespace GOATracer.Importer.Obj.Helpers;

/// <summary>
/// Represents a single point in 3D space that defines part of a 3D model's geometry.
/// Vertices are the building blocks that get connected by faces to form the surface of 3D objects.
/// Immutable once created to ensure coordinate data integrity during ray tracing calculations.
/// </summary>
/// <param name="coordinates">Array containing [x, y, z] coordinates in 3D world space</param>
public class VertexPoint(double[] coordinates)
{
    /// <summary>
    /// X-axis position in 3D world space (typically left/right movement)
    /// </summary>
    private readonly double _x = coordinates[0];
    
    /// <summary>
    /// Y-axis position in 3D world space (typically up/down movement)
    /// </summary>
    private readonly double _y = coordinates[1];
    
    /// <summary>
    /// Z-axis position in 3D world space (typically forward/backward depth)
    /// </summary>
    private readonly double _z = coordinates[2];

    /// <summary>
    /// Retrieves the complete 3D coordinates of this vertex point.
    /// Used by rendering systems and face definitions to access position data.
    /// </summary>
    /// <returns>Array containing [x, y, z] coordinates in that exact order</returns>
    public double[] GetCoordinates()
    {
        return [_x, _y, _z];
    }
}