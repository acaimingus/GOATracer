using GOATracer.Importer.Obj;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GOATracer.Preview
{
    /// <summary>
    /// Class for handling all the needed resources by the preview renderer
    /// </summary>
    public class RenderResourceManager
    {
        /// <summary>
        /// List of the vertices sorted by texture
        /// </summary>
        private readonly Dictionary<string, List<float>> _verticesByTexture = new();
        /// <summary>
        /// List of VAOs for each texture
        /// </summary>
        private readonly Dictionary<string, int> _vaos = new();
        /// <summary>
        /// List of VBOs for each texture
        /// </summary>
        private readonly Dictionary<string, int> _vbos = new();
        /// <summary>
        /// List of vertex counts for each texture
        /// </summary>
        private readonly Dictionary<string, int> _vertexCounts = new();
        /// <summary>
        /// List of all loaded textures
        /// </summary>
        private readonly Dictionary<string, Texture> _loadedTextures = new();

        /// <summary>
        /// Gets or sets the identifier for the lamp buffer object.
        /// </summary>
        public int LampBufferObject { get; set; }

        /// <summary>
        /// Gets the vertex data for the lamp object.
        /// </summary>
        public float[] LampVertices { get; } =
        [
            -0.5f, -0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f,  0.5f, -0.5f, 0.5f,  0.5f, -0.5f, -0.5f,  0.5f, -0.5f, -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f, 0.5f, -0.5f,  0.5f, 0.5f,  0.5f,  0.5f, 0.5f,  0.5f,  0.5f, -0.5f,  0.5f,  0.5f, -0.5f, -0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f, -0.5f,  0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f,  0.5f, -0.5f,  0.5f,  0.5f,
            0.5f,  0.5f,  0.5f, 0.5f,  0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f,  0.5f, 0.5f,  0.5f,  0.5f,
            -0.5f, -0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f,  0.5f, 0.5f, -0.5f,  0.5f, -0.5f, -0.5f,  0.5f, -0.5f, -0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f, 0.5f,  0.5f, -0.5f, 0.5f,  0.5f,  0.5f, 0.5f,  0.5f,  0.5f, -0.5f,  0.5f,  0.5f, -0.5f,  0.5f, -0.5f
        ];

        /// <summary>
        /// Gets or sets the VAO lamp identifier.
        /// </summary>
        public int VaoLamp { get; set; }

        /// <summary>
        /// Loads textures from the materials in the specified scene description.
        /// </summary>
        /// <param name="sceneDescription">The scene description containing materials with texture file paths.</param>
        public void LoadTextures(ImportedSceneDescription? sceneDescription)
        {
            // Quit if there is no sceneDescription
            if (sceneDescription == null) return;
            // go through Materials Dictionary and get DiffuseTexture (should be a path to image file)
            foreach (var imagePath in from material in sceneDescription.Materials!
                     where !string.IsNullOrEmpty(material.Value.DiffuseTexture)
                     select material.Value.DiffuseTexture
                     into imagePath
                     where !_loadedTextures.ContainsKey(imagePath)
                     select imagePath)
            {
                _loadedTextures[imagePath] = Texture.LoadFromFile(imagePath);
            }
        }

        /// <summary>
        /// Loads a shader from given vertex and fragment shader file paths.
        /// </summary>
        /// <param name="vertPath"></param>
        /// <param name="fragPath"></param>
        /// <returns></returns>
        public static Shader LoadShader(string vertPath, string fragPath)
        {
            return new Shader(vertPath, fragPath);
        }

        /// <summary>
        /// Retrieves the dictionary of vertices sorted by texture.
        /// </summary>
        /// <returns>A dictionary where keys are texture paths and values are lists of vertex data.</returns>
        public Dictionary<string, List<float>> GetVerticesByTexture() => _verticesByTexture;

        /// <summary>
        /// Retrieves the dictionary of Vertex Array Objects (VAOs) sorted by texture.
        /// </summary>
        /// <returns>A dictionary where keys are texture paths and values are VAO identifiers.</returns>
        public Dictionary<string, int> GetVao() => _vaos;

        /// <summary>
        /// Retrieves the VAO identifier for the lamp object.
        /// </summary>
        /// <returns>The VAO identifier for the lamp.</returns>
        public int GetVaoLamp() => VaoLamp;

        /// <summary>
        /// Retrieves the VAO identifier for a given texture path.
        /// </summary>
        /// <param name="texturePath">The file path of the texture.</param>
        /// <returns>The VAO identifier associated with the texture.</returns>
        public int GetVao(string texturePath) => _vaos[texturePath];

        /// <summary>
        /// Sets the VAO identifier for a given texture path.
        /// </summary>
        /// <param name="texturePath">The file path of the texture.</param>
        /// <param name="vao">The VAO identifier to set.</param>
        public void SetVao(string texturePath, int vao) => _vaos[texturePath] = vao;

        /// <summary>
        /// Retrieves the VBO identifier for a given texture path.
        /// </summary>
        /// <param name="texturePath">The file path of the texture.</param>
        /// <returns>The VBO identifier associated with the texture.</returns>
        public int GetVbo(string texturePath) => _vbos[texturePath];

        /// <summary>
        /// Sets the VBO identifier for a given texture path.
        /// </summary>
        /// <param name="texturePath">The file path of the texture.</param>
        /// <param name="vbo">The VBO identifier to set.</param>
        public void SetVbo(string texturePath, int vbo) => _vbos[texturePath] = vbo;

        /// <summary>
        /// Sets the vertex count for a given texture path.
        /// </summary>
        /// <param name="texturePath">The file path of the texture.</param>
        /// <param name="count">The number of vertices to set.</param>
        public void SetVertexCount(string texturePath, int count) => _vertexCounts[texturePath] = count;

        /// <summary>
        /// Retrieves the vertex count for a given texture path.
        /// </summary>
        /// <param name="texturePath">The file path of the texture.</param>
        /// <returns>The number of vertices associated with the texture.</returns>
        public int GetVertexCount(string texturePath) => _vertexCounts[texturePath];

        /// <summary>
        /// Retrieves the dictionary of loaded textures.
        /// </summary>
        /// <returns>A dictionary where keys are texture paths and values are Texture objects.</returns>
        public Dictionary<string, Texture> GetLoadedTextures() => _loadedTextures;
    }
}
