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
        private Vector3 _position;
        private Vector3 _direction;
        private double _rotation;
        private double _fov;

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
            _direction = direction;
            _fov = fov;
            _rotation = rotation;
        }
    }
}
