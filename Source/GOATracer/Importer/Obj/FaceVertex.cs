namespace GOATracer.Importer.Obj;

/// <summary>
/// Class describing a singular vertex of a face of a 3d object.
/// Contains the vertex index, the texture index and the normal index for the vertex of the face.
/// </summary>
public class FaceVertex
{
    /// <summary>
    /// Vertex index from the master list.
    /// Cannot be null, because else there is no geometry.
    /// </summary>
    public int VertexIndex { get; }

    /// <summary>
    /// Texture index from the master list.
    /// Can be null if not present.
    /// </summary>
    public int? TextureIndex { get; }

    /// <summary>
    /// Normal index from the master list.
    /// Can be null if not present.
    /// </summary>
    public int? NormalIndex { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="vertexIndex">Vertex index</param>
    /// <param name="textureIndex">Texture index</param>
    /// <param name="normalIndex">Normal index</param>
    public FaceVertex(int vertexIndex, int? textureIndex, int? normalIndex)
    {
        VertexIndex = vertexIndex;
        TextureIndex = textureIndex;
        NormalIndex = normalIndex;
    }
}