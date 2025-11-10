using System.Collections.Generic;
using GOATracer.Importer.Obj;

namespace GOATracer.Raytracer
{
    internal class Scene
    {
        public List<Light> Lights { get; set; }
        public Camera Camera { get; set; }
        public ImportedSceneDescription SceneDescription { get; set; }

        public int ImageHeight { get; set;  }
        public int ImageWidth { get; set; }

        public Scene(List<Light> lights, Camera camera, ImportedSceneDescription sceneDescription, int imageWidth, int imageHeight)
        {
            this.Lights = lights;
            this.Camera = camera;
            this.SceneDescription = sceneDescription;

            this.ImageHeight = imageHeight;
            this.ImageWidth = imageWidth;
        }
    }
}