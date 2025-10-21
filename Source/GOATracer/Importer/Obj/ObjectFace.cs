using System.Collections.Generic;

namespace GOATracer.Importer.Obj;

/// <summary>
/// Represents a polygon face in 3D space by defining which vertices are connected together.
/// Each face is typically a triangle or quad that forms part of a 3D object's surface.
/// Source: https://paulbourke.net/dataformats/obj/obj_spec.pdf
/// </summary>
public class ObjectFace
{
    public readonly List<FaceVertex> Indices;

    /// <summary>
    /// Constructor: Creates a new face definition with the specified vertex connections
    /// </summary>
    /// <param name="indices">The vertex indices that should be connected to form this face</param>
    public ObjectFace(List<FaceVertex> indices)
    {
        this.Indices = indices;
    }
}