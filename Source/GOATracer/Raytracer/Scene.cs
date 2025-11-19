using GOATracer.Importer.Obj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GOATracer.Raytracer
{
    internal class Scene
    {
        public List<Light> Lights { get; set; }
        public Camera Camera { get; set; }
        public ImportedSceneDescription SceneDescription { get; set; }
        public List<ObjectFace> FacePoints { get; set; }

        // --- Store loaded textures here ---
        public Dictionary<string, Texture> TextureCache { get; set; } = new Dictionary<string, Texture>();
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }

        // Main constructor with image dimensions
        public Scene(List<Light> lights, Camera camera, ImportedSceneDescription sceneDescription, int imageWidth, int imageHeight)
        {
            this.Lights = lights;
            this.Camera = camera;
            this.SceneDescription = sceneDescription;

            this.ImageHeight = imageHeight;
            this.ImageWidth = imageWidth;

            this.FacePoints = sceneDescription.ObjectDescriptions?.Count > 0
                ? sceneDescription.ObjectDescriptions.SelectMany(o => o.FacePoints).ToList()
                : new List<ObjectFace>();

            // --- Load the textures when the Scene is created ---
            LoadTextures();
        }

        // Helper to load all textures mentioned in the materials
        private void LoadTextures()
        {
            if (SceneDescription.Materials == null) return;

            foreach (var material in SceneDescription.Materials.Values)
            {
                // If the material has a texture filename (parsed by ObjImporter)...
                if (!string.IsNullOrEmpty(material.DiffuseTexture))
                {
                    // ...and we haven't loaded it yet...
                    if (!TextureCache.ContainsKey(material.DiffuseTexture))
                    {
                        // ...load it into memory!
                        Console.WriteLine($"Loading texture: {material.DiffuseTexture}");
                        TextureCache[material.DiffuseTexture] = new Texture(material.DiffuseTexture);
                    }
                }
            }
        }

        // --- NEW: The function your Raytracer is trying to call ---
        public Vector3 GetMaterialColorForFace(ObjectFace face, FaceVertex fv0, FaceVertex fv1, FaceVertex fv2, float u, float v, ObjectMaterial materialProps)
        {
            // 1. Check if we have a texture loaded for this material
            if (!string.IsNullOrEmpty(materialProps.DiffuseTexture) && TextureCache.ContainsKey(materialProps.DiffuseTexture))
            {
                // 2. Check if the vertices have Texture Coordinates (UVs)
                if (SceneDescription.TexturePoints != null &&
                    fv0.TextureIndex.HasValue &&
                    fv1.TextureIndex.HasValue &&
                    fv2.TextureIndex.HasValue)
                {
                    // 3. Get the UVs (Note: OBJ indices are 1-based, so we subtract 1)
                    Vector3 t0 = SceneDescription.TexturePoints[fv0.TextureIndex.Value - 1];
                    Vector3 t1 = SceneDescription.TexturePoints[fv1.TextureIndex.Value - 1];
                    Vector3 t2 = SceneDescription.TexturePoints[fv2.TextureIndex.Value - 1];

                    // 4. Interpolate the UVs based on where the ray hit the triangle
                    Vector2 uv0 = new Vector2(t0.X, t0.Y);
                    Vector2 uv1 = new Vector2(t1.X, t1.Y);
                    Vector2 uv2 = new Vector2(t2.X, t2.Y);

                    Vector2 interpolatedUV = (uv0 * (1.0f - u - v)) + (uv1 * u) + (uv2 * v);

                    // 5. Ask the Texture class for the color at this UV coordinate
                    return TextureCache[materialProps.DiffuseTexture].GetPixel(interpolatedUV.X, interpolatedUV.Y);
                }
            }

            // Fallback: If no texture, return the basic diffuse color
            return materialProps.ColorDiffuse ?? new Vector3(0.5f, 0.5f, 0.5f);
        }

        // Backwards compatible Constructor (default values)
        public Scene(List<Light> lights, Camera camera, ImportedSceneDescription sceneDescription)
            : this(lights, camera, sceneDescription, 800, 450)
        {
        }
    }
}