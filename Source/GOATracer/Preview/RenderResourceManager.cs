using GOATracer.Importer.Obj;
using System;
using System.Collections.Generic;

namespace GOATracer.Preview
{
    public class RenderResourceManager
    {
        private Dictionary<string, List<float>> _verticesByTexture = new();
        private Dictionary<string, int> _vaos = new();
        private Dictionary<string, int> _vbos = new();
        private Dictionary<string, int> _vertexCounts = new();
        private readonly float[] _lampVertices =
        {
            -0.5f, -0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f,  0.5f, -0.5f, 0.5f,  0.5f, -0.5f, -0.5f,  0.5f, -0.5f, -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f, 0.5f, -0.5f,  0.5f, 0.5f,  0.5f,  0.5f, 0.5f,  0.5f,  0.5f, -0.5f,  0.5f,  0.5f, -0.5f, -0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f, -0.5f,  0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f,  0.5f, -0.5f,  0.5f,  0.5f,
            0.5f,  0.5f,  0.5f, 0.5f,  0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f,  0.5f, 0.5f,  0.5f,  0.5f,
            -0.5f, -0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f,  0.5f, 0.5f, -0.5f,  0.5f, -0.5f, -0.5f,  0.5f, -0.5f, -0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f, 0.5f,  0.5f, -0.5f, 0.5f,  0.5f,  0.5f, 0.5f,  0.5f,  0.5f, -0.5f,  0.5f,  0.5f, -0.5f,  0.5f, -0.5f
        };
        private int _vaoLamp;
        private Dictionary<string, Texture> _loadedTextures = new();

        /// <summary>
        /// Gets or sets the identifier for the lamp buffer object.
        /// </summary>
        public int LampBufferObject { get; set; }

        /// <summary>
        /// Gets the vertex data for the lamp object.
        /// </summary>
        public float[] LampVertices => _lampVertices;

        /// <summary>
        /// Gets or sets the VAO lamp identifier.
        /// </summary>
        public int VaoLamp { get => _vaoLamp; set => _vaoLamp = value; }

        /// <summary>
        /// Loads textures from the materials in the specified scene description.
        /// </summary>
        /// <param name="sceneDescription">The scene description containing materials with texture file paths.</param>
        public void LoadTextures(ImportedSceneDescription? sceneDescription)
        {
            if (sceneDescription != null)
            {
                // go through Materials Dictionary and get DiffuseTexture (should be a path to image file)
                foreach (var material in sceneDescription.Materials!)
                {
                    if (!string.IsNullOrEmpty(material.Value.DiffuseTexture))
                    {
                        // Load texture images
                        var imagePath = material.Value.DiffuseTexture;
                        if (!_loadedTextures.ContainsKey(imagePath))
                        {
                            _loadedTextures[imagePath] = Texture.LoadFromFile(imagePath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads a shader from given vertex and fragment shader file paths.
        /// </summary>
        /// <param name="vertPath"></param>
        /// <param name="fragPath"></param>
        /// <returns></returns>
        public Shader LoadShader(string vertPath, string fragPath)
        {
            return new Shader(vertPath, fragPath);
        }

        public Dictionary<string, List<float>> GetVerticesByTexture() => _verticesByTexture;

        public Dictionary<string, int> GetVao() => _vaos;

        public int GetVaoLamp() => _vaoLamp;

        public int GetVao(string texturePath) => _vaos[texturePath];

        public void SetVao(string texturePath, int vao) => _vaos[texturePath] = vao;

        public int GetVbo(string texturePath) => _vbos[texturePath];

        public void SetVbo(string texturePath, int vbo) => _vbos[texturePath] = vbo;

        public void SetVertexCount(string texturePath, int count) => _vertexCounts[texturePath] = count;

        public int GetVertexCount(string texturePath) => _vertexCounts[texturePath];

        public Dictionary<string, Texture> GetLoadedTextures() => _loadedTextures;
    }
}
