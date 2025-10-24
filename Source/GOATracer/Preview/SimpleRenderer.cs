using System;
using System.Numerics;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GOATracer.Descriptions;
using Vector = Avalonia.Vector;

namespace GOATracer.Preview;

public static class SimpleRenderer
{
    public static WriteableBitmap RenderWireframe(SceneDescription scene, Camera camera, int width, int height)
    {
        // BGRA32 framebuffer (4 bytes for each pixel)
        var frameBuffer = new byte[width * height * 4];

        // Camera matrix
        var view = camera.GetViewMatrix();
        var projection = camera.GetProjectionMatrix();
        var viewProjection = view * projection;

        // Projecting the scene and drawing the wireframe
        foreach (var obj in scene.ObjectDescriptions!)
        foreach (var face in obj.FacePoints)
        {
            if (face.Indices.Count < 3) continue;

            var projected = new Vector2[face.Indices.Count];

            for (var i = 0; i < face.Indices.Count; i++)
            {
                var v = scene.VertexPoints![face.Indices[i] - 1].GetCoordinates();
                var clip = Vector4.Transform(new Vector4(v, 1f), viewProjection);

                // Perspective division
                if (clip.W != 0) clip /= clip.W;

                // NDC [-1..1] -> Screen [0..width/height]
                var x = (clip.X * 0.5f + 0.5f) * (width - 1);
                var y = (1f - (clip.Y * 0.5f + 0.5f)) * (height - 1);
                projected[i] = new Vector2(x, y);
            }

            // Lines of the face
            for (var i = 0; i < face.Indices.Count; i++)
            {
                var j = (i + 1) % face.Indices.Count;
                DrawLine(frameBuffer, width, height, projected[i], projected[j]);
            }
        }

        // Create the bitmap and copy the framebuffer into it
        var bmp = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using var locked = bmp.Lock();

        // BGRA32
        var sourceStride = width * 4;
        var dstBase = locked.Address;

        for (var y = 0; y < height; y++)
        {
            var srcOffset = y * sourceStride;
            var dstRow = dstBase + y * locked.RowBytes;

            unsafe
            {
                fixed (byte* pSrc = &frameBuffer[srcOffset])
                {
                    Buffer.MemoryCopy(pSrc, (void*)dstRow, locked.RowBytes, sourceStride);
                }
            }
        }

        return bmp;
    }

    private static void DrawLine(byte[] frameBuffer, int width, int height, Vector2 a, Vector2 b)
    {
        int x0 = (int)a.X, y0 = (int)a.Y;
        int x1 = (int)b.X, y1 = (int)b.Y;

        int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        var err = dx - dy;

        while (true)
        {
            PutPixel(width, height, frameBuffer, x0, y0, 255, 255, 255, 255);

            if (x0 == x1 && y0 == y1) break;

            var e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private static void PutPixel(int width, int height, byte[] frameBuffer, int x, int y, byte r, byte g, byte b,
        byte a)
    {
        if ((uint)x >= (uint)width || (uint)y >= (uint)height) return;
        // BGRA32
        var idx = (y * width + x) * 4;
        // B
        frameBuffer[idx + 0] = b;
        // G
        frameBuffer[idx + 1] = g;
        // R
        frameBuffer[idx + 2] = r;
        // A
        frameBuffer[idx + 3] = a;
    }
}