using Avalonia;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using GOATracer.Importer.Obj;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace GOATracer.Preview;

public class PreviewRenderer : OpenGlControlBase
{
    private readonly float[] _vertices;
    private readonly int _vertexCountToDraw;
    private readonly Vector3 _lightPos = new(1.2f, 1.0f, 2.0f);
    private int _vertexBufferObject;
    private int _vaoModel;
    private int _vaoLamp;
    private Shader _lampShader = null!;
    private Shader _lightingShader = null!;
    private Camera _camera = null!;
    private bool _firstMove = true;
    private Vector2 _lastPos;
    private bool _glLoaded;
    private float _cameraSpeed = 0.5f;

    private readonly HashSet<Key> _keys = [];

    // BindingsContext, so OpenTK loads GL methods over Avalonia
    private sealed class AvaloniaBindingsContext : OpenTK.IBindingsContext
    {
        private readonly GlInterface _gl;
        public AvaloniaBindingsContext(GlInterface gl) => _gl = gl;
        public IntPtr GetProcAddress(string procName) => _gl.GetProcAddress(procName);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public PreviewRenderer(ImportedSceneDescription sceneDescription)
    {
        this.Focusable = true; // make sure we can get keyboard focus

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
                    var v1 = face.Indices[i];       // second vertex of the triangle
                    var v2 = face.Indices[i + 1];   // third vertex of the triangle

                    // Add all vertex data for the triangle for each triangle corner
                    foreach (var fv in new[] { rootVertex, v1, v2 })
                    {
                        // Get the vertex position (.obj Index is 1-based)
                        var pos = (Vector3)sceneDescription.VertexPoints[fv.VertexIndex - 1];
                        vertexDataList.Add(pos.X);
                        vertexDataList.Add(pos.Y);
                        vertexDataList.Add(pos.Z);

                        // Get normals (Index ist 1-based)
                        // Add a check in case NormalPoints is null or the index is missing
                        var norm = defaultNormal;
                        if (sceneDescription.NormalPoints != null && fv.NormalIndex.HasValue &&
                            fv.NormalIndex.Value <= sceneDescription.NormalPoints.Count)
                        {
                            norm = (Vector3)sceneDescription.NormalPoints[fv.NormalIndex.Value - 1];
                        }

                        // add normal data to vertex data
                        vertexDataList.Add(norm.X);
                        vertexDataList.Add(norm.Y);
                        vertexDataList.Add(norm.Z);
                    }
                }
            }
        }

        _vertices = vertexDataList.ToArray();
        // Calculate the amount of vertices to draw
        // Because every vertex has 6 floats (position and normal), the _vertexCountToDraw gets divided by 6
        _vertexCountToDraw = _vertices.Length / 6;
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        // allows OpenTK to load OpenGL functions using Avalonia's GL context
        GL.LoadBindings(new AvaloniaBindingsContext(gl));
        _glLoaded = true;

        // This will be the color of the background after we clear it, in normalized colors.
        // This is black.
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

        // To see which triangles are in front of others
        GL.Enable(EnableCap.DepthTest);

        // We need to send our vertices over to the graphics card so OpenGL can use them -> create a VBO & send buffer to 
        // stores vertex data in GPU memory
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices,
            BufferUsageHint.StaticDraw);

        // Shaders are tiny programs that live on the GPU. OpenGL uses them to handle the vertex-to-pixel pipeline.
        _lightingShader = new Shader("Shaders/shader.vert", "Shaders/lighting.frag");
        _lampShader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

        {
            // Vertex Array Object (VAO) has the job of keeping track of what parts or what buffers correspond to what data & bind it
            // stores state of vertex attribute configurations (how data is laid out etc.)
            _vaoModel = GL.GenVertexArray();
            GL.BindVertexArray(_vaoModel);

            // Shader attributes linked to VBO data (VAO stores the mapping)
            var positionLocation = _lightingShader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            var normalLocation = _lightingShader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float),
                3 * sizeof(float));
        }

        {
            _vaoLamp = GL.GenVertexArray();
            GL.BindVertexArray(_vaoLamp);

            var positionLocation = _lampShader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        }

        // set camera on position (0,0,3) and aspect ratio according to the control size
        _camera = new Camera(Vector3.UnitZ * 3, (float)(Bounds.Width / Bounds.Height));
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        // check if GL is loaded
        if (!_glLoaded) return;

        // set viewport according to the control size
        var w = Math.Max(1, (int)Bounds.Width);
        var h = Math.Max(1, (int)Bounds.Height);
        GL.Viewport(0, 0, w, h);

        HandleKeyboard();

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // bind VOA, how to interpret vertex data
        GL.BindVertexArray(_vaoModel);

        // use our shader
        _lightingShader.Use();

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

        // This is where we change the lights color over time using the sin function
        Vector3 lightColor;
        var time = DateTime.Now.Second + DateTime.Now.Millisecond / 1000f;
        lightColor.X = (MathF.Sin(time * 2.0f) + 1) / 2f;
        lightColor.Y = (MathF.Sin(time * 0.7f) + 1) / 2f;
        lightColor.Z = (MathF.Sin(time * 1.3f) + 1) / 2f;

        // The ambient light is less intensive than the diffuse light in order to make it less dominant
        var ambientColor = lightColor * new Vector3(0.2f);
        var diffuseColor = lightColor * new Vector3(0.5f);

        _lightingShader.SetVector3("light.position", _lightPos);
        _lightingShader.SetVector3("light.ambient", ambientColor);
        _lightingShader.SetVector3("light.diffuse", diffuseColor);
        _lightingShader.SetVector3("light.specular", new Vector3(1.0f, 1.0f, 1.0f));

        // call to draw the vertices
        GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCountToDraw);

        GL.BindVertexArray(_vaoLamp);

        _lampShader.Use();

        var lampMatrix = Matrix4.Identity;
        lampMatrix *= Matrix4.CreateScale(0.2f);
        lampMatrix *= Matrix4.CreateTranslation(_lightPos);

        _lampShader.SetMatrix4("model", lampMatrix);
        _lampShader.SetMatrix4("view", _camera.GetViewMatrix());
        _lampShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        
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

    // Focus on the previewer when it is visible
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _keys.Add(e.Key);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        _keys.Remove(e.Key);
    }
    /// <summary>
    /// Updates the camera orientation based on mouse movement.
    /// From: https://github.com/opentk/LearnOpenTK/blob/master/Chapter2/2-BasicLighting/Window.cs
    /// </summary>
    /// <param name="mouseX"></param>
    /// <param name="mouseY"></param>
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
}