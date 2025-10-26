using Avalonia;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using GOATracer.Importer.Obj;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GOATracer.Preview;

public class PreviewRenderer : OpenGlControlBase
{
    private readonly float[] _vertices;
    private readonly int _vertexCountToDraw;
    private readonly Vector3 _lightPos = new(1.2f, 1.0f, 2.0f);
    private int _vertexBufferObject;
    private int _vaoModel;
    private int _vaoLamp;
    private Shader _lampShader;
    private Shader _lightingShader;
    private Camera _camera;
    private bool _firstMove = true;
    private Vector2 _lastPos;
    private bool _glLoaded;

    private HashSet<Key> _keys = new HashSet<Key>();

    // BindingsContext, damit OpenTK seine GL-Funktionen über Avalonia lädt
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
        this.Focusable = true;

        var vertexDataList = new List<float>();

        // Default normal, if no other is available
        var defaultNormal = Vector3.UnitY;

        foreach (var objDesc in sceneDescription.ObjectDescriptions!)
        {
            foreach (var face in objDesc.FacePoints)
            {
                // OBJ-faces can have more than 3 points => triangulation needed
                // Simple triangulation
                var rootVertex = face.Indices[0];

                for (var i = 1; i < face.Indices.Count - 1; i++)
                {
                    var v1 = face.Indices[i];
                    var v2 = face.Indices[i + 1];

                    // Add the data for the vertices of the triangle
                    foreach (var fv in new[] { rootVertex, v1, v2 })
                    {
                        // Get the position (Index is 1-based)
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

        GL.LoadBindings(new AvaloniaBindingsContext(gl));
        _glLoaded = true;

        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

        GL.Enable(EnableCap.DepthTest);

        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices,
            BufferUsageHint.StaticDraw);

        _lightingShader = new Shader("Shaders/shader.vert", "Shaders/lighting.frag");
        _lampShader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

        {
            _vaoModel = GL.GenVertexArray();
            GL.BindVertexArray(_vaoModel);

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

        _camera = new Camera(Vector3.UnitZ * 3, (float)(Bounds.Width / Bounds.Height));
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        base.OnOpenGlDeinit(gl);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (!_glLoaded) return;
        
        int w = Math.Max(1, (int)Bounds.Width);
        int h = Math.Max(1, (int)Bounds.Height);
        GL.Viewport(0, 0, w, h);

        HandleKeyboard();

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.BindVertexArray(_vaoModel);

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

    private void HandleKeyboard()
    {
        if (_camera is null) return;

        const float cameraSpeed = 0.5f;
        const float sensitivity = 0.2f;

        if (_keys.Contains(Key.W)) _camera.Position += _camera.Front * cameraSpeed;
        if (_keys.Contains(Key.S)) _camera.Position -= _camera.Front * cameraSpeed;
        if (_keys.Contains(Key.A)) _camera.Position -= _camera.Right * cameraSpeed;
        if (_keys.Contains(Key.D)) _camera.Position += _camera.Right * cameraSpeed;
        if (_keys.Contains(Key.Space)) _camera.Position += _camera.Up * cameraSpeed;
        if (_keys.Contains(Key.LeftShift) || _keys.Contains(Key.RightShift)) _camera.Position -= _camera.Up * cameraSpeed;
    }

    // Focus on the previewer when it is visible
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Focus();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        
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

    // from: https://github.com/opentk/LearnOpenTK/blob/master/Chapter2/2-BasicLighting/Window.cs
    public void ApplyMouseLook(float mouseX, float mouseY)
    {

        if (_camera is null) return;
        float sensitivity = 0.2f;

        var mouse = new Vector2(mouseX, mouseY);
        if (_firstMove)
        {
            _lastPos = mouse;
            _firstMove = false;
        }
        else
        {
            var deltaX = mouse.X - _lastPos.X;
            var deltaY = mouse.Y - _lastPos.Y;
            _lastPos = mouse;
            _camera.Yaw += deltaX * sensitivity;
            _camera.Pitch -= deltaY * sensitivity;
        }
    }
}