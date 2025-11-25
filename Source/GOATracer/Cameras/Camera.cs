using System;
using OpenTK.Mathematics;

namespace GOATracer.Cameras
{
    /// <summary>
    /// A simple class for setting up the camera;
    /// Source: https://github.com/opentk/LearnOpenTK
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// Near plane value
        /// </summary>
        private const float NearPlane = 0.01f;
        /// <summary>
        /// Far plane value
        /// </summary>
        private const float FarPlane = 100000000f;
        /// <summary>
        /// Private backing field of the front vector of the camera
        /// </summary>
        private Vector3 _front = -Vector3.UnitZ;
        /// <summary>
        /// Rotation around the X axis (radians)
        /// </summary>
        private float _pitch;
        /// <summary>
        /// Rotation around the Y axis (radians)
        /// </summary>
        private float _yaw = -MathHelper.PiOver2;
        /// <summary>
        /// The field of view of the camera (radians)
        /// </summary>
        private float _fov = MathHelper.PiOver2;
        /// <summary>
        /// This is simply the aspect ratio of the viewport, used for the projection matrix
        /// </summary>
        private float AspectRatio { get; }

        /// <summary>
        /// Constructor for the camera
        /// </summary>
        /// <param name="position">Vector specifying the camera positio</param>
        /// <param name="aspectRatio">Aspect ratio of the camera</param>
        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }
        /// <summary>
        /// The position of the camera
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Vector specifying where the camera is looking (aka the front of the camera)
        /// </summary>
        public Vector3 Front => _front;
        /// <summary>
        /// Up vector of the camera
        /// </summary>
        public Vector3 Up { get; private set; } = Vector3.UnitY;
        /// <summary>
        /// Right vector of the camera
        /// </summary>
        public Vector3 Right { get; private set; } = Vector3.UnitX;
        
        /// <summary>
        /// Property for the pitch of the camera
        /// </summary>
        public float Pitch
        {
            // We convert from degrees to radians as soon as the property is set to improve performance
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
                // of weird "bugs" when you are using euler angles for rotation.
                // If you want to read more about this you can try researching a topic called gimbal lock
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }
        
        /// <summary>
        /// Property for the yaw of the camera
        /// </summary>
        public float Yaw
        {
            // We convert from degrees to radians as soon as the property is set to improve performance
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }
        
        /// <summary>
        /// FOV property of the camera; The field of view (FOV) is the vertical angle of the camera view
        /// </summary>
        public float Fov
        {
            // We convert from degrees to radians as soon as the property is set to improve performance
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        /// <summary>
        /// Helper method for getting the view matrix of the camera
        /// </summary>
        /// <returns>View matrix of the camera</returns>
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, Up);
        }
        
        /// <summary>
        /// Helper method for getting the projection matrix
        /// </summary>
        /// <returns>the projection matrix of the camera</returns>
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, NearPlane, FarPlane);
        }

        /// <summary>
        /// Helper function for updating the direction vertices
        /// </summary>
        private void UpdateVectors()
        {
            // First, the front matrix is calculated using some basic trigonometry.
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
            _front = Vector3.Normalize(_front);

            // Calculate both the right and the up vector using cross product.
            // Note that we are calculating the right from the global up; this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
            Right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, _front));
        }
    }
}