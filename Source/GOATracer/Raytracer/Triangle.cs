using GOATracer.Importer.Obj;
using System.Numerics;

namespace GOATracer.Raytracer
{
    internal class Triangle
    {
        public Vector3 V0, V1, V2;       // The 3D positions (for intersection testing)
        public FaceVertex FV0, FV1, FV2; // The indices (for texture/normal)
        public ObjectFace OriginalFace;  // Reference to material
        public AABB Bounds;
        public Vector3 Centroid;

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2,
                        FaceVertex fv0, FaceVertex fv1, FaceVertex fv2,
                        ObjectFace originalFace)
        {
            V0 = v0; V1 = v1; V2 = v2;
            FV0 = fv0; FV1 = fv1; FV2 = fv2;
            OriginalFace = originalFace;

            Vector3 min = Vector3.Min(v0, Vector3.Min(v1, v2));
            Vector3 max = Vector3.Max(v0, Vector3.Max(v1, v2));
            Bounds = new AABB(min, max);
            Centroid = (v0 + v1 + v2) / 3.0f;
        }
    }
}