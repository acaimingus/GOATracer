using Avalonia;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using GOATracer.Cameras;
using GOATracer.Importer.Obj;
using GOATracer.Lights;
using GOATracer.MVC;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GOATracer.Preview;

public class PreviewRenderer : OpenGlControlBase
{
    private readonly float[] _vertices;
    private readonly int _vertexCountToDraw;
    private List<Vector3> _lights;
    private int _vertexBufferObject;
    private readonly float[] _lampVertices =
    {
        -0.5f, -0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f,  0.5f, -0.5f, 0.5f,  0.5f, -0.5f, -0.5f,  0.5f, -0.5f, -0.5f, -0.5f, -0.5f,
        -0.5f, -0.5f,  0.5f, 0.5f, -0.5f,  0.5f, 0.5f,  0.5f,  0.5f, 0.5f,  0.5f,  0.5f, -0.5f,  0.5f,  0.5f, -0.5f, -0.5f,  0.5f,
        -0.5f,  0.5f,  0.5f, -0.5f,  0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f, -0.5f,  0.5f, -0.5f,  0.5f,  0.5f,
        0.5f,  0.5f,  0.5f, 0.5f,  0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f,  0.5f, 0.5f,  0.5f,  0.5f,
        -0.5f, -0.5f, -0.5f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f,  0.5f, 0.5f, -0.5f,  0.5f, -0.5f, -0.5f,  0.5f, -0.5f, -0.5f, -0.5f,
        -0.5f,  0.5f, -0.5f, 0.5f,  0.5f, -0.5f, 0.5f,  0.5f,  0.5f, 0.5f,  0.5f,  0.5f, -0.5f,  0.5f,  0.5f, -0.5f,  0.5f, -0.5f
    };
    private int _lampBufferObject;
    private int _vaoModel;
    private int _vaoLamp;
    private Shader _lampShader;
    private Shader _lightingShader;
    private Camera _camera;
    private readonly CameraSettingsBinding _cameraSettings;
    private bool _firstMove;
    private Vector2 _lastPos;
    private bool _glLoaded;
    private float _cameraSpeed;
    private List<Texture> _loadedTextures = [];
    private readonly ImportedSceneDescription sceneDescription;

    private const int MaxLights = 128;
    
    private readonly HashSet<Key> _keys = [];

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
        _firstMove = true;
        _cameraSpeed = 0.5f;
        this.sceneDescription = sceneDescription;

        UpdateLights(lights);

        _cameraSettings = cameraSettings;
        _cameraSettings.UiCameraUpdate += OnCameraSettingsChangedFromUi;

        // Make sure we can get keyboard focus
        this.Focusable = true;

        var vertexDataList = new List<float>();

        // Default normal, if no other is available
        var defaultNormal = Vector3.UnitY;

        // loop through all objects in the scene
        foreach (var objDesc in sceneDescription.ObjectDescriptions!)
        {
            // loop through all faces in the object
            foreach (var face in objDesc.FacePoints)
            {
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
                        vertexDataList.Add(pos.X);
                        vertexDataList.Add(pos.Y);
                        vertexDataList.Add(pos.Z);

                        if (fv.TextureIndex.HasValue && sceneDescription.TexturePoints != null)
                        {
                            // Get texture coordinates (Index is 1-based)
                            var tex = (Vector3)sceneDescription.TexturePoints[fv.TextureIndex - 1 ?? default(int)];
                            // Add texture data to vertex data
                            vertexDataList.Add(tex.X);
                            vertexDataList.Add(tex.Y);
                        }
                        else
                        {
                            // No texture coordinates, add default
                            vertexDataList.Add(0f);
                            vertexDataList.Add(0f);
                        }

                        // Get normals (Index is 1-based)
                        // Add a check in case NormalPoints is null or the index is missing
                        var norm = defaultNormal;
                        if (sceneDescription.NormalPoints != null && fv.NormalIndex.HasValue &&
                            fv.NormalIndex.Value <= sceneDescription.NormalPoints.Count)
                        {
                            norm = (Vector3)sceneDescription.NormalPoints[fv.NormalIndex.Value - 1];
                        }

                        // Add normal data to vertex data
                        vertexDataList.Add(norm.X);
                        vertexDataList.Add(norm.Y);
                        vertexDataList.Add(norm.Z);
                    }
                }
            }
        }

        _vertices = vertexDataList.ToArray();
        // Calculate the amount of vertices to draw
        // Because every vertex has 8 floats (position, texture and normal), the _vertexCountToDraw gets divided by 8
        _vertexCountToDraw = _vertices.Length / 8;
    }

    public void UpdateLights(List<Light> lights)
    {
        _lights = new List<Vector3>();

        if (lights.Count > 0)
        {
            // Convert each light from the light object list to a 3d point and save it in the local light list
            foreach (var vector in lights.Select(light => new Vector3(light.X, light.Y, light.Z)))
            {
                _lights.Add(vector);
            }
        }
    }

    private void OnCameraSettingsChangedFromUi()
    {
        _camera.Position = new Vector3(_cameraSettings.PositionX, _cameraSettings.PositionY, _cameraSettings.PositionZ);
        _camera.Pitch = _cameraSettings.RotationX;
        _camera.Yaw = _cameraSettings.RotationY;
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        // Allows OpenTK to load OpenGL functions using Avalonia's GL context
        GL.LoadBindings(new AvaloniaBindingsContext(gl));
        _glLoaded = true;

        // This will be the color of the background after we clear it, in normalized colors.
        // This is dark blue.
        GL.ClearColor(0.13f, 0.14f, 0.15f, 1.0f);

        LoadTextures(sceneDescription);

        // To see which triangles are in front of others
        GL.Enable(EnableCap.DepthTest);

        // We need to send our vertices over to the graphics card so OpenGL can use them -> create a VBO & send buffer to 
        // stores vertex data in GPU memory
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices,
            BufferUsageHint.StaticDraw);

        // Create a SEPARATE VBO for the lamp objects
        _lampBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _lampBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _lampVertices.Length * sizeof(float), _lampVertices, BufferUsageHint.StaticDraw);
        
        // Shaders are tiny programs that live on the GPU. OpenGL uses them to handle the vertex-to-pixel pipeline.
        _lightingShader = new Shader("Shaders/shader.vert", "Shaders/lighting.frag");
        _lampShader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

        {
            // Vertex Array Object (VAO) has the job of keeping track of what parts or what buffers correspond to what data & bind it
            // stores state of vertex attribute configurations (how data is laid out etc.)
            _vaoModel = GL.GenVertexArray();
            GL.BindVertexArray(_vaoModel);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

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
        }

        {
            _vaoLamp = GL.GenVertexArray();
            GL.BindVertexArray(_vaoLamp);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _lampBufferObject);

            var positionLocation = _lampShader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        }

        // set camera on position (0,0,3) and aspect ratio according to the control size
        _camera = new Camera(Vector3.UnitZ * 3, (float)(Bounds.Width / Bounds.Height));
    }

    private void LoadTextures(ImportedSceneDescription sceneDescription)
    {
        // go through Materials Dictionary and get DiffuseTexture (should be a path to image file)
        foreach (var material in sceneDescription.Materials!)
        {
            if (!string.IsNullOrEmpty(material.Value.DiffuseTexture))
            {
                Console.WriteLine($"Material {material.Key} has diffuse texture: {material.Value.DiffuseTexture}");
                // Load texture images
                string imagePath = material.Value.DiffuseTexture;
                _loadedTextures.Add(Texture.LoadFromFile(imagePath));
            }
        }
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        // check if GL is loaded
        if (!_glLoaded) return;

        // Handle controls in the viewport
        HandleKeyboard();
        
        // set viewport according to the control size
        var w = Math.Max(1, (int)Bounds.Width);
        var h = Math.Max(1, (int)Bounds.Height);
        GL.Viewport(0, 0, w, h);
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // bind VOA, how to interpret vertex data
        GL.BindVertexArray(_vaoModel);

        // use our shader
        _lightingShader.Use();

        _loadedTextures[0].Use(TextureUnit.Texture0);
        _lightingShader.SetInt("texture0", 0);

        _lightingShader.SetMatrix4("model", Matrix4.Identity);
        _lightingShader.SetMatrix4("view", _camera.GetViewMatrix());
        _lightingShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
        _lightingShader.SetVector3("viewPos", _camera.Position);

        // Here we set the material values of the cube, the material struct is just a container so to access
        // the underlying values we simply type "material.value" to get the location of the uniform
        _lightingShader.SetVector3("material.ambient", new Vector3(1.0f, 0.5f, 0.31f));
        _lightingShader.SetVector3("material.diffuse", new Vector3(1.0f, 0.5f, 0.31f));
        _lightingShader.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
        _lightingShader.SetFloat("material.shininess", 32.0f);
        
        var activeLightCount = Math.Min(_lights.Count, MaxLights);
        _lightingShader.SetInt("activeLightCount", activeLightCount);
        var time = DateTime.Now.Second + DateTime.Now.Millisecond / 1000f;

        for (var i = 0; i < activeLightCount; i++)
        {
            Vector3 lightColor;
            lightColor.X = (MathF.Sin((time + i * 0.5f) * 2.0f) + 1) / 2f;
            lightColor.Y = (MathF.Sin((time + i * 0.5f) * 0.7f) + 1) / 2f;
            lightColor.Z = (MathF.Sin((time + i * 0.5f) * 1.3f) + 1) / 2f;
            
            var ambientColor = lightColor * new Vector3(0.2f);
            var diffuseColor = lightColor * new Vector3(0.5f);
            
            _lightingShader.SetVector3($"lights[{i}].position", _lights[i]);
            _lightingShader.SetVector3($"lights[{i}].ambient", ambientColor);
            _lightingShader.SetVector3($"lights[{i}].diffuse", diffuseColor);
            _lightingShader.SetVector3($"lights[{i}].specular", new Vector3(1.0f, 1.0f, 1.0f));
        }

        // call to draw the vertices
        GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCountToDraw);
        GL.BindVertexArray(_vaoLamp);
        
        _lampShader.SetMatrix4("view", _camera.GetViewMatrix());
        _lampShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
        _lampShader.Use();

        foreach (var light in _lights.GetRange(0, activeLightCount))
        {
            var lampMatrix = Matrix4.Identity;
            lampMatrix *= Matrix4.CreateScale(0.2f);
            lampMatrix *= Matrix4.CreateTranslation(light);
            _lampShader.SetMatrix4("model", lampMatrix);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _lampVertices.Length / 3);
        }
        
        RequestNextFrameRendering();
    }

    /// <summary>
    /// Handles keyboard input for camera movement.
    /// Source: https://github.com/opentk/LearnOpenTK/blob/master/Chapter2/2-BasicLighting/Window.cs
    /// </summary>
    private void HandleKeyboard()
    {
        // Slow down camera speed
        if (_keys.Contains(Key.O))
        {
            _cameraSpeed -= 0.01f;
            
            // Bound the minimum speed of the camera to 0.1f
            if (_cameraSpeed < 0.1f)
            {
                _cameraSpeed = 0.1f;
            }
        }

        // Speed up camera speed
        if (_keys.Contains(Key.P))
        {
            _cameraSpeed += 0.01f;
        }

        // Move camera forward
        if (_keys.Contains(Key.W))
        {
            _camera.Position += _camera.Front * _cameraSpeed;
        }

        // Move camera backward
        if (_keys.Contains(Key.S))
        {
            _camera.Position -= _camera.Front * _cameraSpeed;
        }

        // Move camera to the left
        if (_keys.Contains(Key.A))
        {
            _camera.Position -= _camera.Right * _cameraSpeed;
        }

        // Move camera to the right
        if (_keys.Contains(Key.D))
        {
            _camera.Position += _camera.Right * _cameraSpeed;
        }

        // Raise the camera
        if (_keys.Contains(Key.Space))
        {
            _camera.Position += _camera.Up * _cameraSpeed;
        }

        // Lower the camera
        if (_keys.Contains(Key.LeftShift) || _keys.Contains(Key.RightShift))
        {
            _camera.Position -= _camera.Up * _cameraSpeed;
        }
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
    /// Event handler for key presses
    /// </summary>
    /// <param name="eventData">Event data</param>
    protected override void OnKeyDown(KeyEventArgs eventData)
    {
        base.OnKeyDown(eventData);
        _keys.Add(eventData.Key);
    }

    /// <summary>
    /// Event handler for key releases
    /// </summary>
    /// <param name="eventData"></param>
    protected override void OnKeyUp(KeyEventArgs eventData)
    {
        base.OnKeyUp(eventData);
        _keys.Remove(eventData.Key);
    }
    /// <summary>
    /// Updates the camera orientation based on mouse movement.
    /// Source: https://github.com/opentk/LearnOpenTK/blob/master/Chapter2/2-BasicLighting/Window.cs
    /// </summary>
    /// <param name="mouseX">Horizontal mouse movement</param>
    /// <param name="mouseY">Vertical mouse movement</param>
    public void ApplyMouseLook(float mouseX, float mouseY)
    {
        // Mouse sensitivity
        const float sensitivity = 0.4f;

        // current mouse position
        var mouse = new Vector2(mouseX, mouseY);
        if (_firstMove)
        {
            _lastPos = mouse;
            _firstMove = false;
        }
        else
        {
            // mouse movement on X axis
            var deltaX = mouse.X - _lastPos.X;
            // mouse movement on Y axis
            var deltaY = mouse.Y - _lastPos.Y;
            // update last position of the mouse
            _lastPos = mouse;
            // camera horizontal rotation
            _camera.Yaw += deltaX * sensitivity;
            // camera vertical rotation
            _camera.Pitch -= deltaY * sensitivity;
        }
    }

    /// <summary>
    /// Helper method for resetting MouseLook for each new click to remove mouse jump
    /// </summary>
    public void ResetMouseLook()
    {
        _firstMove = true;
    }
}