using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices; // Needed for GCHandle
using Avalonia;                       // Needed for PixelRect
using Avalonia.Media.Imaging;

namespace GOATracer.Raytracer
{
    public class Texture
    {
        private readonly int _width;
        private readonly int _height;
        private readonly byte[] _pixels; // Stores raw pixel data (BGRA)

        public Texture(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Texture not found: {filePath}");
                _width = 1;
                _height = 1;
                _pixels = new byte[] { 255, 0, 255, 255 }; // Magenta fallback
                return;
            }

            using (var bitmap = new Bitmap(filePath))
            {
                // Use PixelSize to get exact integer dimensions (avoids DPI scaling issues)
                _width = bitmap.PixelSize.Width;
                _height = bitmap.PixelSize.Height;

                _pixels = new byte[_width * _height * 4];

                // We must "pin" the array in memory so Avalonia can copy to it safely
                var handle = GCHandle.Alloc(_pixels, GCHandleType.Pinned);
                try
                {
                    IntPtr ptr = handle.AddrOfPinnedObject();

                    // Copy raw pixels from the bitmap into our byte array (.CopyPixels instead of .Lock because of Avalonia)
                    bitmap.CopyPixels(new PixelRect(0, 0, _width, _height), ptr, _pixels.Length, _width * 4);
                }
                finally
                {
                    handle.Free();
                }
            }
        }

        /// <summary>
        /// Get the color of the pixel at normalized coordinates (u, v).
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector3 GetPixel(float u, float v)
        {
            if (_pixels == null || _pixels.Length == 0) return new Vector3(1, 0, 1);

            // 1. Handle Wrapping (Repeat)
            u = u - (float)Math.Floor(u);
            v = v - (float)Math.Floor(v);

            // 2. Map 0..1 to Pixel Coordinates
            // We use (_width - 1) to ensure we don't go out of bounds
            int x = (int)(u * (_width - 1));
            int y = (int)((1.0f - v) * (_height - 1));

            // 3. Index into byte array (4 bytes per pixel)
            int index = (y * _width + x) * 4;

            // Safety check
            if (index < 0 || index > _pixels.Length - 4) return Vector3.Zero;

            // 4. Return Normalized Color (0.0 - 1.0)
            // Avalonia bitmaps are BGRA (Blue, Green, Red, Alpha)
            float b = _pixels[index] / 255.0f;
            float g = _pixels[index + 1] / 255.0f;
            float r = _pixels[index + 2] / 255.0f;

            return new Vector3(r, g, b);
        }
    }
}