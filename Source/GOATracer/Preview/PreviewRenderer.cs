using GOATracer.Importer.Obj;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using GOATracer.Preview.OpenTKAvalonia;
using GOATracer.Preview.Shaders;
using SystemNumerics = System.Numerics;


namespace GOATracer.Preview
{
    /// <summary>
    /// Provides a static method to render a scene preview using OpenTK.
    /// </summary>
    public class PreviewRenderer : BaseTkOpenGlControl
    {
        // private UiOpenGlShader? _shader;

        private ImportedSceneDescription? _scene;
        private Camera? _camera;
        private bool _sceneLoaded = false;
        private int _indexCount = 0;

        public void SetScene(ImportedSceneDescription scene, Camera camera)
        {
            _scene = scene;
            _camera = camera;

            UploadMeshData();
        }

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

        // OpenTkInit (OnLoad) is called once when the control is created
        protected override void OpenTkInit()
        {
            // This will be the color of the background after we clear it, in normalized colors.
            // Normalized colors are mapped on a range of 0.0 to 1.0, with 0.0 representing black, and 1.0 representing
            // the largest possible value for that channel.
            // This is red.
            GL.ClearColor(1.0f, 0.3f, 0.3f, 1.0f);

            // We need to send our vertices over to the graphics card so OpenGL can use them.
            // To do this, we need to create what's called a Vertex Buffer Object (VBO).
            // These allow you to upload a bunch of data to a buffer, and send the buffer to the graphics card.
            // This effectively sends all the vertices at the same time.

            // First, we need to create a buffer. This function returns a handle to it, but as of right now, it's empty.
            // _vertexBufferObject = GL.GenBuffer();

            // Now, bind the buffer. OpenGL uses one global state, so after calling this,
            // all future calls that modify the VBO will be applied to this buffer until another buffer is bound instead.
            // The first argument is an enum, specifying what type of buffer we're binding. A VBO is an ArrayBuffer.
            // There are multiple types of buffers, but for now, only the VBO is necessary.
            // The second argument is the handle to our buffer.
            // GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            // Finally, upload the vertices to the buffer.
            // Arguments:
            //   Which buffer the data should be sent to.
            //   How much data is being sent, in bytes. You can generally set this to the length of your array, multiplied by sizeof(array type).
            //   The vertices themselves.
            //   How the buffer will be used, so that OpenGL can write the data to the proper memory space on the GPU.
            //   There are three different BufferUsageHints for drawing:
            //     StaticDraw: This buffer will rarely, if ever, update after being initially uploaded.
            //     DynamicDraw: This buffer will change frequently after being initially uploaded.
            //     StreamDraw: This buffer will change on every frame.
            //   Writing to the proper memory space is important! Generally, you'll only want StaticDraw,
            //   but be sure to use the right one for your use case.
            // GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * sizeof(float), _vertices.Count, BufferUsageHint.StaticDraw);

            // One notable thing about the buffer we just loaded data into is that it doesn't have any structure to it. It's just a bunch of floats (which are actaully just bytes).
            // The opengl driver doesn't know how this data should be interpreted or how it should be divided up into vertices. To do this opengl introduces the idea of a 
            // Vertex Array Obejct (VAO) which has the job of keeping track of what parts or what buffers correspond to what data. In this example we want to set our VAO up so that 
            // it tells opengl that we want to interpret 12 bytes as 3 floats and divide the buffer into vertices using that.
            // To do this we generate and bind a VAO (which looks deceptivly similar to creating and binding a VBO, but they are different!).
            // _vertexArrayObject = GL.GenVertexArray();
            // GL.BindVertexArray(_vertexArrayObject);

            // Now, we need to setup how the vertex shader will interpret the VBO data; you can send almost any C datatype (and a few non-C ones too) to it.
            // While this makes them incredibly flexible, it means we have to specify how that data will be mapped to the shader's input variables.

            // To do this, we use the GL.VertexAttribPointer function
            // This function has two jobs, to tell opengl about the format of the data, but also to associate the current array buffer with the VAO.
            // This means that after this call, we have setup this attribute to source data from the current array buffer and interpret it in the way we specified.
            // Arguments:
            //   Location of the input variable in the shader. the layout(location = 0) line in the vertex shader explicitly sets it to 0.
            //   How many elements will be sent to the variable. In this case, 3 floats for every vertex.
            //   The data type of the elements set, in this case float.
            //   Whether or not the data should be converted to normalized device coordinates. In this case, false, because that's already done.
            //   The stride; this is how many bytes are between the last element of one vertex and the first element of the next. 3 * sizeof(float) in this case.
            //   The offset; this is how many bytes it should skip to find the first element of the first vertex. 0 as of right now.
            // Stride and Offset are just sort of glossed over for now, but when we get into texture coordinates they'll be shown in better detail.
            // GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            // Enable variable 0 in the shader.
            // GL.EnableVertexAttribArray(0);

            // to see whcih triangles are in front of others
            GL.Enable(EnableCap.DepthTest);

            // We've got the vertices done, but how exactly should this be converted to pixels for the final image?
            // Modern OpenGL makes this pipeline very free, giving us a lot of freedom on how vertices are turned to pixels.
            // The drawback is that we actually need two more programs for this! These are called "shaders".
            // Shaders are tiny programs that live on the GPU. OpenGL uses them to handle the vertex-to-pixel pipeline.
            // Check out the Shader class in Common to see how we create our shaders, as well as a more in-depth explanation of how shaders work.
            // shader.vert and shader.frag contain the actual shader code.
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

            // Now, enable the shader.
            // Just like the VBO, this is global, so every function that uses a shader will modify this one until a new one is bound instead.
            // _shader.Use

            _vertexBufferObject = GL.GenBuffer();
            _elementBufferObject = GL.GenBuffer();
            _vertexArrayObject = GL.GenVertexArray();

            // Setup is now complete! Now we move to the OpenTkRender function to finally draw the triangle.
        }

        //OpenTkRender (OnRenderFrame) is called once a frame. The aspect ratio and keyboard state are configured prior to this being called.
        protected override void OpenTkRender()
        {
            if (!_sceneLoaded || _camera == null)
            {
                // Clear the screen anyway to show the background color
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                return;
            }
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // GL.Enable(EnableCap.DepthTest);
            // GL.Enable(EnableCap.CullFace);

            // GL.ClearColor(new Color4(0, 32, 48, 255));
            // GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            // This clears the image, using what you set as GL.ClearColor earlier.
            // OpenGL provides several different types of data that can be rendered.
            // You can clear multiple buffers by using multiple bit flags.
            // However, we only modify the color, so ColorBufferBit is all we need to clear.
            //GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            /*ImGui.Begin("Color chooser");
            ImGui.ColorEdit3("Triangle color", ref _color);
            ImGui.End();*/

            // To draw an object in OpenGL, it's typically as simple as binding your shader,
            // setting shader uniforms (not done here, will be shown in a future tutorial)
            // binding the VAO,
            // and then calling an OpenGL function to render.

            // Bind the shader
            _shader.Use();

            // Change triangle color
            // int vertexColorLocation = GL.GetUniformLocation(_shader.Handle, "ourColor");
            // GL.Uniform4(vertexColorLocation, 1.0f, 0.0f, 0.0f, 1.0f);

            // Set up camera matrices
            _camera.Aspect = (float)Bounds.Width / (float)Bounds.Height;
            Matrix4 view = ConvertMatrix(_camera.GetViewMatrix());
            Matrix4 projection = ConvertMatrix(_camera.GetProjectionMatrix());
            Matrix4 model = Matrix4.Identity;

            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", view);
            _shader.SetMatrix4("projection", projection);

            // Bind the VAO
            GL.BindVertexArray(_vertexArrayObject);

            // And then call our drawing function.
            // For this tutorial, we'll use GL.DrawArrays, which is a very simple rendering function.
            // Arguments:
            //   Primitive type; What sort of geometric primitive the vertices represent.
            //     OpenGL used to support many different primitive types, but almost all of the ones still supported
            //     is some variant of a triangle. Since we just want a single triangle, we use Triangles.
            //   Starting index; this is just the start of the data you want to draw. 0 here.
            //   How many vertices you want to draw. 3 for a triangle.
            GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);

            // OpenTK windows are what's known as "double-buffered". In essence, the window manages two buffers.
            // One is rendered to while the other is currently displayed by the window.
            // This avoids screen tearing, a visual artifact that can happen if the buffer is modified while being displayed.
            // After drawing, call this function to swap the buffers. If you don't, it won't display what you've rendered.


            //Clean up the opengl state back to how we got it
            // GL.Disable(EnableCap.DepthTest);
        }

        //OpenTkTeardown is called when the control is being destroyed
        protected override void OpenTkTeardown()
        {
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            Console.WriteLine("UI: Tearing down gl component");
        }
        private void UploadMeshData()
        {
            if (_scene?.VertexPoints == null || _scene.ObjectDescriptions == null)
            {
                _sceneLoaded = false;
                return;
            }

            var vertices = new List<float>();
            var indices = new List<uint>();

            // Flatten the vertex data from Vector3 to a simple float array
            foreach (var vertexPoint in _scene.VertexPoints)
            {
                vertices.Add(vertexPoint.X);
                vertices.Add(vertexPoint.Y);
                vertices.Add(vertexPoint.Z);
            }

            // Flatten the face data into an element index list
            // OBJ indices are 1-based, so we subtract 1.
            foreach (var obj in _scene.ObjectDescriptions)
            {
                foreach (var face in obj.FacePoints)
                {
                    // Assuming triangles for simplicity. If you have quads, you'd need to triangulate.
                    indices.Add((uint)face.Indices[0].VertexIndex - 1);
                    indices.Add((uint)face.Indices[1].VertexIndex - 1);
                    indices.Add((uint)face.Indices[2].VertexIndex - 1);
                }
            }

            _indexCount = indices.Count;

            // Upload to GPU
            GL.BindVertexArray(_vertexArrayObject);

            // 1. Vertex Buffer Object (VBO)
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);

            // 2. Element Buffer Object (EBO)
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

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
