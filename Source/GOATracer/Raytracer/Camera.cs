using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GOATracer.Raytracer
{
    internal class Camera
    {
        // Private backing fields
        private Vector3 _position;
        private Vector3 _direction;
        private double _rotation; // Camera roll in degrees
        private double _fov;      // Vertical FOV in degrees

        // Public properties
        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vector3 Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public double Fov
        {
            get { return _fov; }
            set { _fov = value; }
        }
        public double Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public Camera(Vector3 position, Vector3 direction, double fov, double rotation)
        {
            _position = position;
            _direction = Vector3.Normalize(direction);
            _fov = fov;
            _rotation = rotation;
        }

        // --- MANUAL RIGHT-HANDED MATRICES (AS PER REQUIREMENT) ---

        /// <summary>
        /// Creates a Right-Handed LookAt matrix (like OpenGL's gluLookAt).
        /// </summary>
        private static Matrix4x4 CreateCustomLookAtRightHanded(Vector3 position, Vector3 target, Vector3 worldUp)
        {
            Vector3 zAxis = Vector3.Normalize(position - target); // "forward"
            Vector3 xAxis = Vector3.Normalize(Vector3.Cross(worldUp, zAxis)); // "right"
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis); // "up"

            return new Matrix4x4(
                xAxis.X, yAxis.X, zAxis.X, 0,
                xAxis.Y, yAxis.Y, zAxis.Y, 0,
                xAxis.Z, yAxis.Z, zAxis.Z, 0,
                -Vector3.Dot(xAxis, position), -Vector3.Dot(yAxis, position), -Vector3.Dot(zAxis, position), 1.0f
            );
        }

        /// <summary>
        /// Creates a Right-Handed Perspective FOV matrix (like OpenGL's gluPerspective).
        /// </summary>
        private static Matrix4x4 CreateCustomPerspectiveFieldOfViewRightHanded(float fovRad, float aspectRatio, float nearPlane, float farPlane)
        {
            float f = (float)(1.0 / Math.Tan(fovRad / 2.0));

            return new Matrix4x4(
                f / aspectRatio, 0, 0, 0,
                0, f, 0, 0,
                0, 0, (farPlane + nearPlane) / (nearPlane - farPlane), -1.0f,
                0, 0, (2.0f * farPlane * nearPlane) / (nearPlane - farPlane), 0.0f
            );
        }

        // --- MATRIX GETTERS (NOW USING CUSTOM METHODS) ---

        private Matrix4x4 GetViewMatrix()
        {
            Vector3 forward = _direction;
            Vector3 target = _position + forward;
            Vector3 worldUp = (Math.Abs(forward.Y) < 0.999f) ? Vector3.UnitY : Vector3.UnitX;

            Matrix4x4 rollMatrix = Matrix4x4.CreateFromAxisAngle(forward, (float)(_rotation * Math.PI / 180.0));
            Vector3 finalUp = Vector3.TransformNormal(worldUp, rollMatrix);

            // Use our custom Right-Handed function
            return CreateCustomLookAtRightHanded(_position, target, finalUp);
        }

        private Matrix4x4 GetProjectionMatrix(float aspectRatio)
        {
            float fovRad = (float)(_fov * (Math.PI / 180.0));
            float nearPlane = 0.1f;
            float farPlane = 1000.0f;

            // Use our custom Right-Handed function
            return CreateCustomPerspectiveFieldOfViewRightHanded(fovRad, aspectRatio, nearPlane, farPlane);
        }

        // --- GETRAYDIRECTION (REMAINS THE SAME) ---

        public Vector3 GetRayDirection(int x, int y, int imageWidth, int imageHeight)
        {
            float aspectRatio = (float)imageWidth / imageHeight;
            Matrix4x4 viewMatrix = GetViewMatrix();
            Matrix4x4 projectionMatrix = GetProjectionMatrix(aspectRatio);

            Matrix4x4.Invert(viewMatrix, out var invView);
            Matrix4x4.Invert(projectionMatrix, out var invProjection);

            float ndcX = (float)((x + 0.5) / imageWidth * 2.0 - 1.0);
            float ndcY = (float)(1.0 - (y + 0.5) / imageHeight * 2.0);

            Vector4 ray_ndc = new Vector4(ndcX, ndcY, 1.0f, 1.0f);
            Vector4 ray_view = Vector4.Transform(ray_ndc, invProjection);
            ray_view /= ray_view.W;

            Vector4 ray_world_dir_h = Vector4.Transform(new Vector4(ray_view.X, ray_view.Y, ray_view.Z, 0.0f), invView);
            Vector3 ray_world_dir = new Vector3(ray_world_dir_h.X, ray_world_dir_h.Y, ray_world_dir_h.Z);

            return Vector3.Normalize(ray_world_dir);
        }
    }
}