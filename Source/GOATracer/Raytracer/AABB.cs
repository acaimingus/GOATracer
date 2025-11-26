using System;
using System.Numerics;

namespace GOATracer.Raytracer
{
    internal struct AABB
    {
        public Vector3 Min;
        public Vector3 Max;

        public AABB(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Checks if a ray intersects the box (Slab method). Returns true if there is an intersection and sets tMinVal to the distance to the intersection.
        /// </summary>
        /// <param name="rayOrigin"></param>
        /// <param name="rayDir"></param>
        /// <param name="tMinVal"></param>
        /// <returns></returns>
        public bool Intersect(Vector3 rayOrigin, Vector3 rayDir, out float tMinVal)
        {
            tMinVal = 0;
            float tmin = (Min.X - rayOrigin.X) / rayDir.X;
            float tmax = (Max.X - rayOrigin.X) / rayDir.X;

            if (tmin > tmax) (tmin, tmax) = (tmax, tmin);

            float tymin = (Min.Y - rayOrigin.Y) / rayDir.Y;
            float tymax = (Max.Y - rayOrigin.Y) / rayDir.Y;

            if (tymin > tymax) (tymin, tymax) = (tymax, tymin);

            if ((tmin > tymax) || (tymin > tmax))
                return false;

            if (tymin > tmin) tmin = tymin;
            if (tymax < tmax) tmax = tymax;

            float tzmin = (Min.Z - rayOrigin.Z) / rayDir.Z;
            float tzmax = (Max.Z - rayOrigin.Z) / rayDir.Z;

            if (tzmin > tzmax) (tzmin, tzmax) = (tzmax, tzmin);

            if ((tmin > tzmax) || (tzmin > tmax))
                return false;

            if (tzmin > tmin) tmin = tzmin;

            tMinVal = tmin;
            // If tmax < 0, the box is behind the ray origin
            return tmax > 0;
        }

        /// <summary>
        /// Helper method: Expands the bounding-box to include a point
        /// </summary>
        /// <param name="p"></param>
        public void Grow(Vector3 p)
        {
            Min = Vector3.Min(Min, p);
            Max = Vector3.Max(Max, p);
        }
    }
}