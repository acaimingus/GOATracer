using Avalonia.Markup.Xaml.MarkupExtensions;
using GOATracer.Importer.Obj;
using System;
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


        public byte[] render()
        {
            byte[] buffer = new byte[scene.ImageWidth * scene.ImageHeight * 4];
            int width = scene.ImageWidth;
            int height = scene.ImageHeight;

            // For each pixel in the image
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Compute the ray direction from the camera through the pixel
                    Vector3 rayDirection = scene.Camera.GetRayDirection(x, y, scene.ImageWidth, scene.ImageHeight);

                    // Trace the ray through the scene
                    Vector3 color = traceRay(scene.Camera.Position, rayDirection, scene);

                    // Get the starting index for this pixel in the 1D buffer
                    int index = (y * width + x) * 4;

                    // Convert Vector3 color (0.0-1.0) to BGRA8888 bytes (0-255)
                    buffer[index] = (byte)(Math.Clamp(color.Z, 0, 1) * 255);     // Blue
                    buffer[index + 1] = (byte)(Math.Clamp(color.Y, 0, 1) * 255); // Green
                    buffer[index + 2] = (byte)(Math.Clamp(color.X, 0, 1) * 255); // Red
                    buffer[index + 3] = 255;
                }
            }
            return buffer;
        }

        // Trace a ray from support vector sv in direction dv through the scene and return the color
        public Vector3 traceRay(Vector3 sv, Vector3 dv, Scene scene)
        {
            // Find the first intersection with the scene
            if (intersect(sv, dv, out Vector3 intersectionPoint, out Vector3 normal, out Vector3 materialDiffuseColor))
            {
                // --- SHADOW CHECK ---
                // Get direction to the first light
                Vector3 lightDirection = Vector3.Normalize(scene.Lights[0].Position - intersectionPoint);

                // Move the shadow ray origin slightly off the surface to avoid "shadow acne"
                Vector3 shadowRayOrigin = intersectionPoint + normal * 0.0001f;

                // Check if another object is between this point and the light
                if (intersect(shadowRayOrigin, lightDirection, out _, out _, out _))
                {
                    // IN SHADOW: Return only a dim ambient light
                    // We use the material color multiplied by a dark ambient factor
                    Vector3 ambientLight = new Vector3(0.1f, 0.1f, 0.1f);
                    return materialDiffuseColor * ambientLight;
                }
                else
                {
                    // NOT IN SHADOW: Compute the full Phong shading
                    return shade(normal, materialDiffuseColor, intersectionPoint, lightDirection, scene);
                }
            }
            else
            {
                // If the ray hits nothing, return the background color
                return new Vector3(0.0f, 0.1f, 0.3f);
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
                                    material = matProps.Diffuse;
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

        // https://www.scratchapixel.com/lessons/3d-basic-rendering/phong-shader-BRDF/phong-illumination-models-brdf.html
        // Shade the intersection point given normal, material constant, intersection point, light direction and scene
        public Vector3 shade(Vector3 normal, Vector3 materialDiffuseColor, Vector3 intersectionPoint, Vector3 lightDirection, Scene scene)
        {
            materialDiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
            // --- 1. Define Light Properties ---
            Vector3 lightColor = scene.Lights[0].Color;

            // --- 2. Define Material Properties
            Vector3 ambientColor = materialDiffuseColor * 0.1f; // 10% ambient
            Vector3 diffuseColor = materialDiffuseColor;
            Vector3 specularColor = new Vector3(1.0f, 1.0f, 1.0f); // White highlights
            float shininess = 32.0f; // A standard shininess value

            // --- 3. Ambient Component (Global illumination) ---
            Vector3 ambient = ambientColor;

            // --- 4. Diffuse Component (Lambertian reflection) ---
            // Calculates how much the surface is facing the light
            float diffuseFactor = Math.Max(0.0f, Vector3.Dot(normal, lightDirection));
            Vector3 diffuse = diffuseColor * lightColor * diffuseFactor;

            // --- 5. Specular Component (Phong reflection) ---
            // Calculates the "shiny" highlight
            Vector3 viewDir = Vector3.Normalize(scene.Camera.Position - intersectionPoint);
            Vector3 reflectDir = Vector3.Reflect(-lightDirection, normal);
            float specularFactor = (float)Math.Pow(Math.Max(0.0f, Vector3.Dot(viewDir, reflectDir)), shininess);
            Vector3 specular = specularColor * lightColor * specularFactor;

            // --- 6. Combine all components ---
            Vector3 finalColor = ambient + diffuse + specular;

            // Clamp the color to be between 0.0 and 1.0
            return Vector3.Clamp(finalColor, Vector3.Zero, Vector3.One);
        }
    }
}
