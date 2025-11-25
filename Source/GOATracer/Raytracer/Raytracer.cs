using Avalonia.Markup.Xaml.MarkupExtensions;
using GOATracer.Importer.Obj;
using System;
using System.Numerics;


// Source: https://www.youtube.com/watch?v=mTOllvinv-U
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

        public bool intersect(Vector3 rayOrigin, Vector3 rayDirection, out Vector3 hitPoint, out Vector3 normal, out Vector3 material)
        {
            hitPoint = Vector3.Zero;
            normal = Vector3.Zero;
            material = Vector3.Zero; // Default material

            // Query the octree for the closest intersection
            if (scene.SceneOctree.Intersect(rayOrigin, rayDirection, out Triangle hitTri, out float hitDistance, out float u, out float v))
            {
                hitPoint = rayOrigin + rayDirection * hitDistance;

                // Retrieve original face data from the hit triangle to access material and normal indices
                FaceVertex fv0 = hitTri.FV0;
                FaceVertex fv1 = hitTri.FV1;
                FaceVertex fv2 = hitTri.FV2;
                ObjectFace face = hitTri.OriginalFace;

                // Check for normal indices and interpolate if available
                bool hasNormals = scene.SceneDescription.NormalPoints != null &&
                                  fv0.NormalIndex > 0 &&
                                  fv1.NormalIndex > 0 &&
                                  fv2.NormalIndex > 0;

                if (hasNormals)
                {
                    Vector3 n0 = scene.SceneDescription.NormalPoints[fv0.NormalIndex.Value - 1];
                    Vector3 n1 = scene.SceneDescription.NormalPoints[fv1.NormalIndex.Value - 1];
                    Vector3 n2 = scene.SceneDescription.NormalPoints[fv2.NormalIndex.Value - 1];

                    normal = Vector3.Normalize((n0 * (1.0f - u - v)) + (n1 * u) + (n2 * v));
                }
                else
                {
                    // Fallback to geometric normal
                    normal = Vector3.Normalize(Vector3.Cross(hitTri.V1 - hitTri.V0, hitTri.V2 - hitTri.V0));
                }

                // Calculate material color (handles textures if present)
                if (face.Material != null && scene.SceneDescription.Materials.TryGetValue(face.Material, out var matProps))
                {
                    material = scene.GetMaterialColorForFace(face, fv0, fv1, fv2, u, v, matProps);
                }
                else
                {
                    material = new Vector3(0.5f, 0.5f, 0.5f);
                }

                return true;
            }

            return false;
        }

        public static bool RayTriangleIntersection(Vector3 rayOrigin, Vector3 rayDirection, Vector3 v0, Vector3 v1, Vector3 v2, out float hitDistance, out float u, out float v)
        {
            hitDistance = 0;
            u = 0;
            v = 0;

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;

            Vector3 h = Vector3.Cross(rayDirection, edge2);
            float a = Vector3.Dot(edge1, h);

            if (a > -0.000001f && a < 0.000001f)
            {
                return false;
            }

            float f = 1.0f / a;
            Vector3 s = rayOrigin - v0;
            u = f * Vector3.Dot(s, h);

            if (u < 0.0f || u > 1.0f)
            {
                return false;
            }

            Vector3 q = Vector3.Cross(s, edge1);
            v = f * Vector3.Dot(rayDirection, q);

            if (v < 0.0f || u + v > 1.0f)
            {
                return false;
            }

            hitDistance = f * Vector3.Dot(edge2, q);

            return hitDistance > 0.000001f;
        }

        public Vector3 shade(Vector3 normal, Vector3 materialDiffuseColor, Vector3 intersectionPoint, Vector3 lightDirection, Scene scene)
        {
            //materialDiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
            Vector3 lightColor = scene.Lights[0].Color;

            Vector3 ambientColor = materialDiffuseColor * 0.1f;
            Vector3 diffuseColor = materialDiffuseColor;
            Vector3 specularColor = new Vector3(1.0f, 1.0f, 1.0f);
            float shininess = 32.0f;

            Vector3 ambient = ambientColor;
            float diffuseFactor = Math.Max(0.0f, Vector3.Dot(normal, lightDirection));
            Vector3 diffuse = diffuseColor * lightColor * diffuseFactor;

            Vector3 viewDir = Vector3.Normalize(scene.Camera.Position - intersectionPoint);
            Vector3 reflectDir = Vector3.Reflect(-lightDirection, normal);
            float specularFactor = (float)Math.Pow(Math.Max(0.0f, Vector3.Dot(viewDir, reflectDir)), shininess);
            Vector3 specular = specularColor * lightColor * specularFactor;

            // Combine all components
            Vector3 finalColor = ambient + diffuse + specular;

            // Clamp the color to be between 0.0 and 1.0
            return Vector3.Clamp(finalColor, Vector3.Zero, Vector3.One);
        }
    }
}