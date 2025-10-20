using GOATracer.Descriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace GOATracer.Raytracer
{
    internal class Scene
    {
        public List<Light> Lights { get; set; }
        public Camera Camera { get; set; }
        public SceneDescription SceneDescription { get; set; }
        public List<FaceDescription> FacePoints { get; set; }

        public int ImageHeight { get; }
        public int ImageWidth { get; }

        public Scene(List<Light> lights, Camera camera, SceneDescription sceneDescription)
        {
            this.Lights = lights;
            this.Camera = camera;
            this.SceneDescription = sceneDescription;

            this.ImageHeight = 800;
            this.ImageWidth = 450;

            this.FacePoints = sceneDescription.ObjectDescriptions?.Count > 0
                ? sceneDescription.ObjectDescriptions.SelectMany(o => o.FacePoints).ToList()
                : new List<FaceDescription>();
        }
    }
}