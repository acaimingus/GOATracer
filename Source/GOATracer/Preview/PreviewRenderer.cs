using GOATracer.Importer.Obj;
using GOATracer.Preview.OpenTKAvalonia;
using GOATracer.Preview.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using SystemNumerics = System.Numerics;


namespace GOATracer.Preview
{
    /// <summary>
    /// Provides a static method to render a scene preview using OpenTK.
    /// </summary>
    public class PreviewRenderer : BaseTkOpenGlControl
    {
        private ImportedSceneDescription? _scene;
        private Camera? _camera;
        private bool _sceneLoaded = false;
        private int _indexCount = 0;

        private List<float> _vertices = new List<float>();
        private List<uint> _indices = new List<uint>();

        // These are the handles to OpenGL objects. A handle is an integer representing where the object lives on the
        // graphics card. Consider them sort of like a pointer; we can't do anything with them directly, but we can
        // send them to OpenGL functions that need them.


        // What these objects are will be explained in OnLoad.
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _elementBufferObject;

        // This class is a wrapper around a shader, which helps us manage it.
        // The shader class's code is in the Shaders folder.
        private Shader _shader;

        public void SetScene(ImportedSceneDescription scene, Camera camera)
        {
            _scene = scene;
            _camera = camera;

            foreach (var vertexPoint in _scene.VertexPoints)
            {
                _vertices.Add(vertexPoint.X);
                _vertices.Add(vertexPoint.Y);
                _vertices.Add(vertexPoint.Z);
            }

            foreach (var obj in _scene.ObjectDescriptions)
            {
                foreach (var face in obj.FacePoints)
                {
                    // Assuming triangles for simplicity. If you have quads, you'd need to triangulate.
                    _indices.Add((uint)face.Indices[0].VertexIndex - 1);
                    _indices.Add((uint)face.Indices[1].VertexIndex - 1);
                    _indices.Add((uint)face.Indices[2].VertexIndex - 1);
                }
            }

            _indexCount = _indices.Count;

            // UploadMeshData();
        }


        // OpenTkInit (OnLoad) is called once when the control is created
        protected override void OpenTkInit()
        {
            // This will be the color of the background after we clear it, in normalized colors.
            // This is red.
            GL.ClearColor(1.0f, 0.3f, 0.3f, 1.0f);


            // IMPORTANT: Which first? VAO, VBO, or EBO?


            // Vertex Array Obejct (VAO) has the job of keeping track of what parts or what buffers correspond to what data & bind it
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // We need to send our vertices over to the graphics card so OpenGL can use them. -> create a VBO & send buffer to GPU
            // First, we need to create a buffer. This function returns a handle to it, but as of right now, it's empty.
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * sizeof(float), _vertices.ToArray(), BufferUsageHint.StaticDraw);


            // how the vertex shader will interpret the VBO data -> use the GL.VertexAttribPointer function
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            // Enable variable 0 in the shader.
            GL.EnableVertexAttribArray(0);


            // Element Buffer Object (EBO) for indices
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(uint), _indices.ToArray(), BufferUsageHint.StaticDraw);


            // Shaders are tiny programs that live on the GPU. OpenGL uses them to handle the vertex-to-pixel pipeline.
            _shader = new Shader("Preview/Shaders/shader.vert", "Preview/Shaders/shader.frag");

            _shader.Use();

            // Setup is now complete! Now we move to the OpenTkRender function to finally draw the triangle.
        }

        //OpenTkRender (OnRenderFrame) is called once a frame. The aspect ratio and keyboard state are configured prior to this being called.
        protected override void OpenTkRender()
        {
            // To see which triangles are in front of others:
            GL.Enable(EnableCap.DepthTest);

            // whatever
            GL.Enable(EnableCap.CullFace);

            // changes background color to dark blue
            GL.ClearColor(new Color4(0, 32, 48, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            // Bind the shader
            _shader.Use();

            if (_camera != null)
            {
                // Set up camera matrices
                _camera.AspectRatio = (float)Bounds.Width / (float)Bounds.Height;
                Matrix4 view = _camera.GetViewMatrix();
                Matrix4 projection = _camera.GetProjectionMatrix();
                Matrix4 model = Matrix4.Identity;

                _shader.SetMatrix4("model", model);
                _shader.SetMatrix4("view", view);
                _shader.SetMatrix4("projection", projection);
            }

            // Bind the VAO -> This is also in Init()
            GL.BindVertexArray(_vertexArrayObject);

            // And then call our drawing function.
            GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
        }

        //OpenTkTeardown is called when the control is being destroyed
        protected override void OpenTkTeardown()
        {
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            Console.WriteLine("UI: Tearing down gl component");
        }

        public void SetCamera(Camera camera)
        {
            _camera = camera;
        }

        private void UploadMeshData()
        {
            if (_scene?.VertexPoints == null || _scene.ObjectDescriptions == null)
            {
                _sceneLoaded = false;
                return;
            }
            

            // Flatten the vertex data from Vector3 to a simple float array
            foreach (var vertexPoint in _scene.VertexPoints)
            {
                _vertices.Add(vertexPoint.X);
                _vertices.Add(vertexPoint.Y);
                _vertices.Add(vertexPoint.Z);
            }

            // Flatten the face data into an element index list
            // OBJ indices are 1-based, so we subtract 1.
            foreach (var obj in _scene.ObjectDescriptions)
            {
                foreach (var face in obj.FacePoints)
                {
                    // Assuming triangles for simplicity. If you have quads, you'd need to triangulate.
                    _indices.Add((uint)face.Indices[0].VertexIndex - 1);
                    _indices.Add((uint)face.Indices[1].VertexIndex - 1);
                    _indices.Add((uint)face.Indices[2].VertexIndex - 1);
                }
            }

            _indexCount = _indices.Count;

            // Upload to GPU
            GL.BindVertexArray(_vertexArrayObject);

            // 1. Vertex Buffer Object (VBO)
            // Now, bind the buffer. OpenGL uses one global state, so after calling this,
            // all future calls that modify the VBO will be applied to this buffer until another buffer is bound instead.
            // The first argument is an enum, specifying what type of buffer we're binding. A VBO is an ArrayBuffer.
            // GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Finally, upload the vertices to the buffer.
            // GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);

            // 2. Element Buffer Object (EBO)
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(uint), _indices.ToArray(), BufferUsageHint.StaticDraw);

            // 3. Vertex Attributes
            // Tell OpenGL how to interpret the vertex data in the VBO.
            // Location 0 (aPosition in shader) has 3 floats per vertex.
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _sceneLoaded = _indexCount > 0;
        }


        private Matrix4 ConvertMatrix(SystemNumerics.Matrix4x4 matrix)
        {
            return new Matrix4(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
        }
    }
}
