using Avalonia.Controls.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

/// Raytracer namespace containing core raytracing classes
/// such as Raytracer, Scene, Camera, Light, etc.
/// Source: https://www.youtube.com/watch?v=mTOllvinv-U


namespace GOATracer.Raytracer
{
    internal class Raytracer
    {
        Scene scene;

        public Raytracer(Scene scene)
        {
            this.scene = scene;
        }

        public void render()
        {
                       // For each pixel in the image
            for (int y = 0; y < scene.ImageHeight; y++)
            {
                for (int x = 0; x < scene.ImageWidth; x++)
                {
                    // Compute the ray direction from the camera through the pixel
                    Vector3 rayDirection = scene.Camera.GetRayDirection(x, y, scene.ImageWidth, scene.ImageHeight);
                    // Trace the ray through the scene
                    traceRay(scene.Camera.Position, rayDirection, scene);
                }
            }
        }

        // Trace a ray from support vector sv in direction dv through the scene
        public void traceRay(Vector3 sv, Vector3 dv, Scene scene)
        {

        }

        // Intersection of ray with scene objects, returns normal, material constant and point of intersection
        public void intersect(Vector3 p, Vector3 d)
        {
            
        }

        // Shade the intersection point given normal, material constant, intersection point, light direction and scene
        public void shade(Vector3 normal, Vector3 materialConstant, Vector3 intersectionPoint, Vector3 lightDirecetion, Scene scene)
        {

        }
    }
}
