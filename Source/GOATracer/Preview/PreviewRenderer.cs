using Avalonia;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using GOATracer.Importer.Obj;
using GOATracer.Lights;
using GOATracer.MVC;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GOATracer.Preview;

public class PreviewRenderer : OpenGlControlBase
{
    
    private const int MaxLights = 128;
    private bool _glLoaded;
    private Shader _lampShader;
    private Shader _lightingShader;

    private readonly ImportedSceneDescription _sceneDescription;
    private readonly PreviewScene _previewScene;
    private readonly InputHandler _inputHandler;
    private readonly RenderResourceManager _renderResourceManager = new();

    /// <summary>
    /// BindingsContext, so OpenTK loads GL methods over Avalonia
    /// </summary>
    private sealed class AvaloniaBindingsContext : OpenTK.IBindingsContext
    {
        private readonly GlInterface _gl;
        public AvaloniaBindingsContext(GlInterface gl) => _gl = gl;
        public IntPtr GetProcAddress(string procName) => _gl.GetProcAddress(procName);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public PreviewRenderer(ImportedSceneDescription sceneDescription, List<Light> lights, CameraSettingsBinding cameraSettings)
    {
        _sceneDescription = sceneDescription;
        _previewScene = new PreviewScene(lights, cameraSettings);
        _inputHandler = new InputHandler(_previewScene);
        var verticesByTexture = _renderResourceManager.GetVerticesByTexture();

        // Make sure we can get keyboard focus
        this.Focusable = true;

        var vertexDataList = new List<float>();

        foreach (var objectDescription in sceneDescription.ObjectDescriptions!)
        {
            foreach (var face in objectDescription.FacePoints)
            {
                var texturePath = "default";
                if (sceneDescription.Materials is { Count: > 0 } && sceneDescription.Materials.TryGetValue(face.Material, out var material))
                {
                    if (!string.IsNullOrEmpty(material.DiffuseTexture))
                    {
                        texturePath = material.DiffuseTexture;
                    }
                }

                if (!verticesByTexture.ContainsKey(texturePath))
                {
                    verticesByTexture[texturePath] = new List<float>();
                }

                var currentVertexList = verticesByTexture[texturePath];
                
                // OBJ-faces can have more than 3 points => triangulation needed
                // Simple triangulation
                var rootVertex = face.Indices[0];

                for (var i = 1; i < face.Indices.Count - 1; i++)
                {
                    // Second vertex of the triangle
                    var v1 = face.Indices[i];
                    // Third vertex of the triangle
                    var v2 = face.Indices[i + 1];

                    // Add all vertex data for the triangle for each triangle corner
                    foreach (var fv in new[] { rootVertex, v1, v2 })
                    {
                        // Get the vertex position (.obj Index is 1-based)
                        var pos = (Vector3)sceneDescription.VertexPoints[fv.VertexIndex - 1];
                        currentVertexList.Add(pos.X);
                        currentVertexList.Add(pos.Y);
                        currentVertexList.Add(pos.Z);

                        if (fv.TextureIndex.HasValue && sceneDescription.TexturePoints != null)
                        {
                            // Get texture coordinates (Index is 1-based)
                            var tex = (Vector3)sceneDescription.TexturePoints[fv.TextureIndex - 1 ?? default(int)];
                            // Add texture data to vertex data
                            currentVertexList.Add(tex.X);
                            currentVertexList.Add(tex.Y);
                        }
                        else
                        {
                            // No texture coordinates, add default
                            currentVertexList.Add(0f);
                            currentVertexList.Add(0f);
                        }

                        // Default normal, if no other is available
                        var defaultNormal = Vector3.UnitY;
                        
                        // Get normals (Index is 1-based)
                        // Add a check in case NormalPoints is null or the index is missing
                        var norm = defaultNormal;
                        if (sceneDescription.NormalPoints != null && fv.NormalIndex.HasValue &&
                            fv.NormalIndex.Value <= sceneDescription.NormalPoints.Count)
                        {
                            norm = (Vector3)sceneDescription.NormalPoints[fv.NormalIndex.Value - 1];
                        }

                        // Add normal data to vertex data
                        currentVertexList.Add(norm.X);
                        currentVertexList.Add(norm.Y);
                        currentVertexList.Add(norm.Z);
                    }
                }
            }
        }
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        // Allows OpenTK to load OpenGL functions using Avalonia's GL context
        GL.LoadBindings(new AvaloniaBindingsContext(gl));
        _glLoaded = true;

        // This will be the color of the background after we clear it, in normalized colors.
        // This is gray
        GL.ClearColor(0.13f, 0.14f, 0.15f, 1.0f);

        _renderResourceManager.LoadTextures(_sceneDescription);

        // To see which triangles are in front of others
        GL.Enable(EnableCap.DepthTest);

        _lightingShader = _renderResourceManager.LoadShader("Shaders/shader.vert", "Shaders/lighting.frag");
        _lampShader = _renderResourceManager.LoadShader("Shaders/shader.vert", "Shaders/shader.frag");

        var verticesByTexture = _renderResourceManager.GetVerticesByTexture();

        foreach (var (texturePath, vertices) in  verticesByTexture)
        {
            var vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            
            var vertexArray = vertices.ToArray();
            GL.BufferData(BufferTarget.ArrayBuffer, vertexArray.Length * sizeof(float), vertexArray, BufferUsageHint.StaticDraw);
            
            var vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            
            // Shader attributes linked to VBO data (VAO stores the mapping)
            var positionLocation = _lightingShader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            
            // texture coordinates attribute
            var texCoordLocation = _lightingShader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            
            var normalLocation = _lightingShader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));
            
            _renderResourceManager.SetVao(texturePath, vao);
            _renderResourceManager.SetVbo(texturePath, vbo);
            _renderResourceManager.SetVertexCount(texturePath, vertexArray.Length / 8);

            // _vbos[texturePath] = vbo;
            // _vaos[texturePath] = vao;
            // _vertexCounts[texturePath] = vertexArray.Length / 8;
        }

        // Create a SEPARATE VBO for the lamp objects
        _renderResourceManager.LampBufferObject = GL.GenBuffer();
        float[] lampVertices = _renderResourceManager.LampVertices;

        GL.BindBuffer(BufferTarget.ArrayBuffer, _renderResourceManager.LampBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, lampVertices.Length * sizeof(float), lampVertices, BufferUsageHint.StaticDraw);
        
        {
            int vaoLamp = GL.GenVertexArray();
            _renderResourceManager.VaoLamp = vaoLamp;
            GL.BindVertexArray(vaoLamp);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _renderResourceManager.LampBufferObject);

            var positionLocation = _lampShader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        }
        
        _previewScene.SetupCamera(Bounds.Size);
        _previewScene.UpdateCameraPositionToBinding();
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        // check if GL is loaded
        if (!_glLoaded) return;

        var _camera = _previewScene.GetCamera();
        var _lights = _previewScene.GetLights();

        // Handle controls in the viewport
        _inputHandler.HandleKeyboard();
        
        // Add a scaling factor for displays which use it
        var scalingFactor = this.VisualRoot.RenderScaling;
        
        // set viewport according to the control size
        var w = Math.Max(1, (int)(Bounds.Width * scalingFactor));
        var h = Math.Max(1, (int)(Bounds.Height * scalingFactor));
        GL.Viewport(0, 0, w, h);
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // use our shader
        _lightingShader.Use();
        
        _lightingShader.SetInt("texture0", 0);

        _lightingShader.SetMatrix4("model", Matrix4.Identity);
        _lightingShader.SetMatrix4("view", _camera.GetViewMatrix());
        _lightingShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
        _lightingShader.SetVector3("viewPos", _camera.Position);

        var activeLightCount = Math.Min(_lights.Count, MaxLights);
        _lightingShader.SetInt("activeLightCount", activeLightCount);
        var time = DateTime.Now.Second + DateTime.Now.Millisecond / 1000f;

        for (var i = 0; i < activeLightCount; i++)
        {
            // white light
            Vector3 lightColor = new Vector3(1f, 1f, 1f);

            var ambientColor = lightColor * 0.05f;

            _lightingShader.SetVector3($"lights[{i}].position", _lights[i]);
            _lightingShader.SetVector3($"lights[{i}].ambient", ambientColor);
            _lightingShader.SetVector3($"lights[{i}].diffuse", lightColor);
            _lightingShader.SetVector3($"lights[{i}].specular", new Vector3(1.0f, 1.0f, 1.0f));
        }

        var loadedTextures = _renderResourceManager.GetLoadedTextures();
        var vaos = _renderResourceManager.GetVao();

        foreach (var (texturePath, vao) in vaos)
        {
            var hasTexture = false;
            
            if (loadedTextures.TryGetValue(texturePath, out var texture))
            {
                texture.Use(TextureUnit.Texture0);
                _lightingShader.SetInt("texture0", 0);
                hasTexture = true;
            }
            else if (loadedTextures.TryGetValue("default", out var defaultTexture))
            {
                defaultTexture.Use(TextureUnit.Texture0);
                _lightingShader.SetInt("texture0", 0);
                hasTexture = true;
            }
            
            _lightingShader.SetInt("hasTexture", hasTexture ? 1 : 0);
            
            var material = _sceneDescription.Materials?
                .FirstOrDefault(m => m.Value.DiffuseTexture == texturePath)
                .Value;
                    
            var matAmbient = new Vector3(0.1f, 0.1f, 0.1f);
            var matDiffuse = new Vector3(1.0f, 1.0f, 1.0f);
            var matSpecular = new Vector3(0.01f, 0.01f, 0.01f);
            var matShininess = 32.0f;
        
            if (material != null)
            {
                if (material.ColorAmbient != null)
                {
                    matAmbient = (Vector3)material.ColorAmbient;
                }
                if (material.ColorDiffuse != null)
                {
                    matDiffuse = (Vector3)material.ColorDiffuse;
                }
                if (material.ColorSpecular != null)
                {
                    matSpecular = (Vector3)material.ColorSpecular;
                }

                if (material.SpecularExponent != null)
                {
                    if (material.SpecularExponent != 0.0f)
                    {
                        matShininess =  (float)material.SpecularExponent;
                    }
                }
            }
        
            // Here we set the material values of the cube, the material struct is just a container so to access
            // the underlying values we simply type "material.value" to get the location of the uniform
            _lightingShader.SetVector3("material.ambient", matAmbient);
            _lightingShader.SetVector3("material.diffuse", matDiffuse);
            _lightingShader.SetVector3("material.specular", matSpecular);
            _lightingShader.SetFloat("material.shininess", matShininess);
            
            _lightingShader.SetMatrix4("model", Matrix4.Identity);
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0,  _renderResourceManager.GetVertexCount(texturePath));
        }
        
        GL.BindVertexArray(_renderResourceManager.GetVaoLamp());
        
        _lampShader.SetMatrix4("view", _camera.GetViewMatrix());
        _lampShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
        _lampShader.Use();

        foreach (var light in _lights.GetRange(0, activeLightCount))
        {
            var lampMatrix = Matrix4.Identity;
            lampMatrix *= Matrix4.CreateScale(0.2f);
            lampMatrix *= Matrix4.CreateTranslation(light);
            _lampShader.SetMatrix4("model", lampMatrix);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _renderResourceManager.LampVertices.Length / 3);
        }
        
        RequestNextFrameRendering();
    }

    public void UpdateLights(List<Light> lights)
    {
        _previewScene.UpdateLights(lights);
    }

    /// <summary>
    /// Focus on the previewer when it is visible
    /// </summary>
    /// <param name="eventData">Event data</param>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs eventData)
    {
        base.OnAttachedToVisualTree(eventData);
        Focus();
    }

    /// <summary>
    /// Event handler for key presses. Delegates to InputHandler.
    /// </summary>
    /// <param name="eventData">Event data</param>
    protected override void OnKeyDown(KeyEventArgs eventData)
    {
        _inputHandler.OnKeyDown(eventData);
    }

    /// <summary>
    /// Event handler for key releases. Delegates to InputHandler.
    /// </summary>
    /// <param name="eventData"></param>
    protected override void OnKeyUp(KeyEventArgs eventData)
    {
        _inputHandler.OnKeyUp(eventData);
    }

    /// <summary>
    /// Helper method for resetting MouseLook for each new click to remove mouse jump. Delegates to InputHandler.
    /// </summary>
    public void ResetMouseLook()
    {
        _inputHandler.ResetMouseLook();
    }

    /// <summary>
    /// Updates the camera orientation based on mouse movement. Delegates to InputHandler.
    /// </summary>
    /// <param name="x">Horizontal mouse movement.</param>
    /// <param name="y">Vertical mouse movement.</param>
    public void ApplyMouseLook(float x, float y)
    {
        _inputHandler.ApplyMouseLook(x, y);
    }
}