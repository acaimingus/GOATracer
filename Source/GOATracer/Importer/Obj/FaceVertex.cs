namespace GOATracer.Importer.Obj;

/// <summary>
/// Class describing a vertex of a face of a 3d object.
/// Contains the vertex index, the texture index and the normal index for this face.
/// </summary>
public class FaceVertex
{
    /// <summary>
    /// Vertex index from the master list.
    /// </summary>
    public readonly int VertexIndex;
    
    /// <summary>
    /// Texture index from the master list.
    /// Can be null if not present.
    /// </summary>
    public readonly int? TextureIndex;
    
    /// <summary>
    /// Normal index from the master list.
    /// Can be null if not present.
    /// </summary>
    public readonly int? NormalIndex;

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