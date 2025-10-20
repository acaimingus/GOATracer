using GOATracer.Descriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GOATracer.Raytracer
{
    internal class Scene
    {
        List<Light> lights;
        Camera camera = new Camera(new System.Numerics.Vector3(0, 0, 0), new System.Numerics.Vector3(0, 0, 1), 90, 0);
        SceneDescription sceneDescription;
        public List<FaceDescription> facePoints;

        public Scene(List<Light> lights, Camera camera, SceneDescription sceneDescription) {
            this.lights = lights;
            this.camera = camera;
            this.sceneDescription = sceneDescription;
            facePoints = sceneDescription.ObjectDescriptions?.Count > 0 ? sceneDescription.ObjectDescriptions.SelectMany(o => o.FacePoints).ToList() : new List<FaceDescription>();
        }
    }
}
