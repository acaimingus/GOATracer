using System;
using System.Collections.Generic;
using System.Numerics;

namespace GOATracer.Raytracer
{
    internal class Octree
    {
        private OctreeNode root;

        public Octree(List<Triangle> allTriangles)
        {
            // 1. Calculate global bounding box for the entire scene
            if (allTriangles.Count == 0) return;

            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var t in allTriangles)
            {
                min = Vector3.Min(min, t.Bounds.Min);
                max = Vector3.Max(max, t.Bounds.Max);
            }

            // Add a small padding to avoid numerical errors at the boundaries
            min -= new Vector3(0.001f);
            max += new Vector3(0.001f);

            // 2. Build root node
            root = new OctreeNode(new AABB(min, max), allTriangles, 0);
        }

        /// <summary>
        /// Intersect ray with the octree to find the closest triangle hit. Returns true if a hit is found.
        /// </summary>
        /// <param name="rayOrigin"></param>
        /// <param name="rayDir"></param>
        /// <param name="hitTriangle"></param>
        /// <param name="hitDistance"></param>
        /// <param name="uOut"></param>
        /// <param name="vOut"></param>
        /// <returns></returns>
        public bool Intersect(Vector3 rayOrigin, Vector3 rayDir, out Triangle hitTriangle, out float hitDistance, out float uOut, out float vOut)
        {
            hitTriangle = null;
            hitDistance = float.PositiveInfinity;
            uOut = 0;
            vOut = 0;

            if (root == null) return false;

            return root.Intersect(rayOrigin, rayDir, ref hitTriangle, ref hitDistance, ref uOut, ref vOut);
        }
    }

    internal class OctreeNode
    {
        private AABB bounds;
        private OctreeNode[] children; // Either null (leaf) or 8 children
        private List<Triangle> triangles; // Only populated if leaf

        private const int MAX_DEPTH = 10; // Maximum depth of the tree
        private const int MIN_TRIANGLES = 10; // Threshold to stop splitting

        public OctreeNode(AABB nodeBounds, List<Triangle> nodeTriangles, int depth)
        {
            this.bounds = nodeBounds;

            // Termination condition: Few triangles or max depth reached
            if (nodeTriangles.Count <= MIN_TRIANGLES || depth >= MAX_DEPTH)
            {
                this.triangles = nodeTriangles;
                this.children = null;
            }
            else
            {
                this.triangles = null;
                this.children = new OctreeNode[8];

                // Calculate center and half-size for the 8 children
                Vector3 min = bounds.Min;
                Vector3 max = bounds.Max;
                Vector3 center = (min + max) * 0.5f;

                Vector3[] childMins = new Vector3[8];
                Vector3[] childMaxs = new Vector3[8];

                // Construction of the 8 octants
                for (int i = 0; i < 8; i++)
                {
                    // Bit 0 = X, Bit 1 = Y, Bit 2 = Z
                    Vector3 newMin = Vector3.Zero;
                    Vector3 newMax = Vector3.Zero;

                    newMin.X = ((i & 1) == 0) ? min.X : center.X;
                    newMax.X = ((i & 1) == 0) ? center.X : max.X;

                    newMin.Y = ((i & 2) == 0) ? min.Y : center.Y;
                    newMax.Y = ((i & 2) == 0) ? center.Y : max.Y;

                    newMin.Z = ((i & 4) == 0) ? min.Z : center.Z;
                    newMax.Z = ((i & 4) == 0) ? center.Z : max.Z;

                    childMins[i] = newMin;
                    childMaxs[i] = newMax;
                }

                // Distribute triangles to children
                List<Triangle>[] childTris = new List<Triangle>[8];
                for (int i = 0; i < 8; i++) childTris[i] = new List<Triangle>();

                foreach (var t in nodeTriangles)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        // Simple test: Does the triangle box intersect the child box?
                        if (BoxIntersectsBox(t.Bounds, childMins[i], childMaxs[i]))
                        {
                            childTris[i].Add(t);
                        }
                    }
                }

                // Recursively create children
                for (int i = 0; i < 8; i++)
                {
                    children[i] = new OctreeNode(new AABB(childMins[i], childMaxs[i]), childTris[i], depth + 1);
                }
            }
        }

        /// <summary>
        /// Check box intersection. 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private bool BoxIntersectsBox(AABB b, Vector3 min, Vector3 max)
        {
            if (b.Max.X < min.X || b.Min.X > max.X) return false;
            if (b.Max.Y < min.Y || b.Min.Y > max.Y) return false;
            if (b.Max.Z < min.Z || b.Min.Z > max.Z) return false;
            return true;
        }

        /// <summary>
        /// Traversal
        /// </summary>
        /// <param name="rayOrigin"></param>
        /// <param name="rayDir"></param>
        /// <param name="closestTri"></param>
        /// <param name="closestDist"></param>
        /// <param name="uOut"></param>
        /// <param name="vOut"></param>
        /// <returns></returns>
        public bool Intersect(Vector3 rayOrigin, Vector3 rayDir, ref Triangle closestTri, ref float closestDist, ref float uOut, ref float vOut)
        {
            // 1. Does the ray hit this node at all?
            if (!bounds.Intersect(rayOrigin, rayDir, out float boxDist))
            {
                return false;
            }

            // If the node is further away than the closest hit found so far, we don't need to search further
            if (boxDist > closestDist)
            {
                return false;
            }

            bool hit = false;

            // Case 2: Leaf node (Check triangles)
            if (children == null)
            {
                foreach (var tri in triangles)
                {
                    if (Raytracer.RayTriangleIntersection(rayOrigin, rayDir, tri.V0, tri.V1, tri.V2, out float t, out float u, out float v))
                    {
                        if (t > 0.0001f && t < closestDist)
                        {
                            closestDist = t;
                            closestTri = tri;
                            uOut = u;
                            vOut = v;
                            hit = true;
                        }
                    }
                }
                return hit;
            }

            // Case 3: Internal node (Check children)
            // Optimization: We could sort children by distance, but iterating all is sufficient for now
            // Since 'closestDist' is passed by ref, later children benefit from hits in earlier children.
            for (int i = 0; i < 8; i++)
            {
                if (children[i].Intersect(rayOrigin, rayDir, ref closestTri, ref closestDist, ref uOut, ref vOut))
                {
                    hit = true;
                }
            }

            return hit;
        }
    }
}