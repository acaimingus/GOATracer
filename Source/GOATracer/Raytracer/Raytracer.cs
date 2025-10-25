using Avalonia.Markup.Xaml.MarkupExtensions;
using GOATracer.Importer.Obj;
using System.Numerics;


// Source: https://www.youtube.com/watch?v=mTOllvinv-Uhttps://www.youtube.com/watch?v=mTOllvinv-U
// https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/barycentric-coordinates
namespace GOATracer.Raytracer
{
    internal class Raytracer
    {
        Scene scene;

        public Raytracer(Scene scene)
        {
            this.scene = scene;
        }

        public void render()
        {
            // For each pixel in the image
            for (int y = 0; y < scene.ImageHeight; y++)
            {
                for (int x = 0; x < scene.ImageWidth; x++)
                {
                    // Compute the ray direction from the camera through the pixel
                    Vector3 rayDirection = scene.Camera.GetRayDirection(x, y, scene.ImageWidth, scene.ImageHeight);
                    // Trace the ray through the scene
                    traceRay(scene.Camera.Position, rayDirection, scene);
                }
            }
        }

        // Trace a ray from support vector sv in direction dv through the scene and return the color
        public Vector3 traceRay(Vector3 sv, Vector3 dv, Scene scene)
        {
            // Find the first intersection with the scene
            if (intersect(sv, dv, out Vector3 intersectionPoint, out Vector3 normal, out Vector3 materialConstant))
            {
                // If we hit something, compute the shading
                // get the light direction
                Vector3 lightDirection = Vector3.Normalize(scene.Lights[0].Position - intersectionPoint);

                return shade(normal, materialConstant, intersectionPoint, lightDirection, scene);
            }
            else
            {
                // If the ray hits nothing, return a background color
                return Vector3.Zero;
            }
        }

        // Intersection of ray with scene objects, returns normal, material constant and point of intersection
        public bool intersect(Vector3 rayOrigin, Vector3 rayDirection, out Vector3 hitPoint, out Vector3 normal, out Vector3 material)
        {
            float closestDistance = float.PositiveInfinity;
            bool hitFound = false;

            // Initialize output parameters to default values.
            hitPoint = Vector3.Zero;
            normal = Vector3.Zero;
            material = Vector3.Zero; // Default material (black)

            // Iterate through all renderable objects provided by the scene description.
            foreach (var obj in scene.SceneDescription.ObjectDescriptions)
            {
                // Iterate through all faces that compose the current object.
                foreach (var face in obj.FacePoints)
                {
                    // A face must have at least 3 vertices to form a triangle.
                    if (face.Indices == null || face.Indices.Count < 3)
                    {
                        continue;
                    }

                    // --- Polygon Triangulation (Fan Method) ---
                    // We triangulate polygons by creating a "fan" of triangles 
                    // from the first vertex (v0) to all other vertices.
                    // e.g., (v0, v1, v2), (v0, v2, v3), (v0, v3, v4), ...

                    // Get the common vertex (v0) for the fan.
                    // NOTE: .obj files are 1-BASED, C# Lists are 0-BASED. We must subtract 1.
                    FaceVertex fv0 = face.Indices[0];
                    Vector3 v0 = scene.SceneDescription.VertexPoints[fv0.VertexIndex - 1];

                    // Iterate through the remaining vertices to form triangles.
                    for (int i = 1; i < face.Indices.Count - 1; i++)
                    {
                        // Get the next two vertices to form the triangle (v0, v_i, v_i+1)
                        FaceVertex fv1 = face.Indices[i];
                        FaceVertex fv2 = face.Indices[i + 1];

                        Vector3 v1 = scene.SceneDescription.VertexPoints[fv1.VertexIndex - 1];
                        Vector3 v2 = scene.SceneDescription.VertexPoints[fv2.VertexIndex - 1];

                        // Perform the ray-triangle intersection test.
                        // for correct normal and texture interpolation.
                        if (RayTriangleIntersection(rayOrigin, rayDirection, v0, v1, v2, out float hitDistance, out float u, out float v))
                        {
                            // Validate the intersection:
                            // 1. Is it closer than the current closest hit?
                            // 2. Is it in front of the ray (hitDistance > epsilon)?
                            // A small epsilon (e.g., 0.0001f) is crucial to prevent "shadow acne".
                            if (hitDistance > 0.0001f && hitDistance < closestDistance)
                            {
                                hitFound = true;
                                closestDistance = hitDistance;

                                // Calculate and store the precise intersection point.
                                hitPoint = rayOrigin + rayDirection * hitDistance;

                                // --- Normal Calculation ---
                                // Check if the file provided normals and this face uses them.
                                bool hasNormals = scene.SceneDescription.NormalPoints != null &&
                                                  fv0.NormalIndex > 0 &&
                                                  fv1.NormalIndex > 0 &&
                                                  fv2.NormalIndex > 0;

                                if (hasNormals)
                                {
                                    // Smooth shading: Interpolate vertex normals using barycentric coordinates.
                                    Vector3 n0 = scene.SceneDescription.NormalPoints[fv0.NormalIndex - 1 ?? 0];
                                    Vector3 n1 = scene.SceneDescription.NormalPoints[fv1.NormalIndex - 1 ?? 0];
                                    Vector3 n2 = scene.SceneDescription.NormalPoints[fv2.NormalIndex - 1 ?? 0];

                                    // w = 1.0f - u - v
                                    normal = Vector3.Normalize((n0 * (1.0f - u - v)) + (n1 * u) + (n2 * v));
                                }
                                else
                                {
                                    // Flat shading: Calculate the geometric normal of the triangle.
                                    normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));
                                }

                                // --- Material Retrieval ---
                                // Retrieve the material color from the scene's material library.
                                if (face.Material != null && scene.SceneDescription.Materials.TryGetValue(face.Material, out var matProps))
                                {
                                    // Assuming 'ObjectMaterial' has a 'Vector3 Diffuse' property.
                                    // material = matProps.Diffuse; 

                                    // Placeholder: Use a visible color if material is found.
                                    material = new Vector3(0.8f, 0.8f, 0.8f);
                                }
                                else
                                {
                                    // Use a default grey if no material is assigned.
                                    material = new Vector3(0.5f, 0.5f, 0.5f);
                                }
                            }
                        }
                    }
                }
            }

            return hitFound;
        }

        // Performs a ray-triangle intersection test (Möller-Trumbore algorithm).
        // https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/moller-trumbore-ray-triangle-intersection
        /// <summary>
        /// Performs a high-speed intersection test between a ray and a triangle
        /// using the Möller-Trumbore algorithm.
        /// 
        /// The function of this algorithm is to solve for the intersection 
        /// simultaneously and efficiently by avoiding a separate ray-plane 
        /// intersection test.
        /// 
        /// It provides three critical pieces of information:
        /// 1. A boolean (true/false) indicating if an intersection occurred.
        /// 2. The distance 't' (out hitDistance) from the ray's origin to the 
        ///    intersection point. This is used to find the closest object.
        /// 3. The barycentric coordinates 'u' and 'v' (out u, out v) of the
        ///    hit point on the triangle. These are essential for interpolating
        ///    vertex normals (for smooth shading) and texture coordinates.
        /// </summary>
        private bool RayTriangleIntersection(Vector3 rayOrigin, Vector3 rayDirection, Vector3 v0, Vector3 v1, Vector3 v2, out float hitDistance, out float u, out float v)
        {
            // Initialize output parameters
            hitDistance = 0;
            u = 0;
            v = 0;

            // Find vectors for two edges sharing vertex v0
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;

            // Begin calculating
            // h = rayDirection x edge2
            Vector3 h = Vector3.Cross(rayDirection, edge2);

            // a = edge1 ⋅ h
            float a = Vector3.Dot(edge1, h);

            // If the determinant 'a' is close to zero, the ray is parallel to the triangle plane.
            // No intersection is possible.
            if (a > -0.000001f && a < 0.000001f)
            {
                return false;
            }

            // Calculate the inverse determinant.
            float f = 1.0f / a;

            // s = rayOrigin - v0
            Vector3 s = rayOrigin - v0;

            // Calculate barycentric coordinate 'u'
            // u = f * (s ⋅ h)
            u = f * Vector3.Dot(s, h);

            // Check if the 'u' coordinate is outside the valid range [0, 1].
            if (u < 0.0f || u > 1.0f)
            {
                return false; // Intersection point is outside the triangle.
            }

            // Prepare to test 'v' parameter
            // q = s x edge1
            Vector3 q = Vector3.Cross(s, edge1);

            // Calculate barycentric coordinate 'v'
            // v = f * (rayDirection ⋅ q)
            v = f * Vector3.Dot(rayDirection, q);

            // Check if the 'v' coordinate is outside the valid range [0, 1]
            // or if (u + v) is greater than 1.
            if (v < 0.0f || u + v > 1.0f)
            {
                return false; // Intersection point is outside the triangle.
            }

            // At this stage, we know we have a valid intersection inside the triangle.
            // Calculate 't' (the hitDistance), which is the distance from the ray origin 
            // to the intersection point.
            // t = f * (edge2 ⋅ q)
            hitDistance = f * Vector3.Dot(edge2, q);

            // Check if the intersection is in front of the ray's origin.
            // (t > EPSILON) ensures we don't hit objects "behind" us.
            if (hitDistance > 0.000001f)
            {
                // A valid, forward-facing intersection was found.
                return true;
            }
            else
            {
                // The intersection is behind the ray origin.
                return false;
            }
        }


        // Shade the intersection point given normal, material constant, intersection point, light direction and scene
        public Vector3 shade(Vector3 normal, Vector3 materialConstant, Vector3 intersectionPoint, Vector3 lightDirecetion, Scene scene)
        {
            // ToDo: Phong shading model
            return materialConstant; // Placeholder: return material constant as color
        }
    }
}
