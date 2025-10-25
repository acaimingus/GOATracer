using System.Collections.Generic;
using GOATracer.Importer.Obj;

namespace GOATracer.Raytracer
{
    internal class Scene
    {
        public List<Light> Lights { get; set; }
        public Camera Camera { get; set; }
        public ImportedSceneDescription SceneDescription { get; set; }

        public int ImageHeight { get; }
        public int ImageWidth { get; }

        public Scene(List<Light> lights, Camera camera, ImportedSceneDescription sceneDescription)
        {
            this.Lights = lights;
            this.Camera = camera;
            this.SceneDescription = sceneDescription;

            this.ImageHeight = 800;
            this.ImageWidth = 450;
        }
    }
}