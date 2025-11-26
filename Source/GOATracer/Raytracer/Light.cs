using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GOATracer.Raytracer
{
    internal class Light
    {
        private Vector3 _position;
        private double _intensity;

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public double Intensity
        {
            get { return _intensity; }
            set { _intensity = value; }
        }

        public Vector3 Color
        {
            get; set;
        }

        public Light(Vector3 position, double intensity, Vector3 color)
        {
            this._position = position;
            this._intensity = intensity;
            this.Color = color;
        }
    }
}