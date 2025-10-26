using System;
using System.Collections.Generic;
using System.Linq;
using GOATracer.Importer.Obj;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GOATracer.Preview
{
    // In this tutorial we take the code from the last tutorial and create some level of abstraction over it allowing more
    // control of the interaction between the light and the material.
    // At the end of the web version of the tutorial we also had a bit of fun creating a disco light that changes
    // color of the cube over time.
    //
    // Source: https://github.com/opentk/LearnOpenTK
    public class Window : GameWindow
    {
        private readonly float[] _vertices;

        private readonly Vector3 _lightPos = new(1.2f, 1.0f, 2.0f);

        private int _vertexBufferObject;

        private int _vaoModel;

        private int _vaoLamp;

        private Shader _lampShader;

        private Shader _lightingShader;

        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;
        
        private readonly int _vertexCountToDraw;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings,
            ImportedSceneDescription sceneDescription)
            : base(gameWindowSettings, nativeWindowSettings)
        {
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

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

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

            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

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

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused)
            {
                return;
            }

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (KeyboardState.IsKeyDown(Keys.W))
            {
                // Forward
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time;
            }

            if (KeyboardState.IsKeyDown(Keys.S))
            {
                // Backwards
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time;
            }

            if (KeyboardState.IsKeyDown(Keys.A))
            {
                // Left
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time;
            }

            if (KeyboardState.IsKeyDown(Keys.D))
            {
                // Right
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time;
            }

            if (KeyboardState.IsKeyDown(Keys.Space))
            {
                // Up
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time;
            }

            if (KeyboardState.IsKeyDown(Keys.LeftShift))
            {
                // Down
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time;
            }

            if (_firstMove)
            {
                _lastPos = new Vector2(MouseState.X, MouseState.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = MouseState.X - _lastPos.X;
                var deltaY = MouseState.Y - _lastPos.Y;
                _lastPos = new Vector2(MouseState.X, MouseState.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }
    }
}