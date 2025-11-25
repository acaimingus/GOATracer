using System.Collections.ObjectModel;
using GOATracer.Importer.Obj;
using GOATracer.Lights;

namespace GOATracer.Models
{ 
    /// <summary>
    /// Model representing the Ray Tracer settings and scene description
    /// </summary>
    public class RayTracerModel
    {
        public RayTracerModel(ImportedSceneDescription importedSceneDescription)
        {
            this.ImportedSceneDescription = importedSceneDescription;
        }

        public RayTracerModel()
        {
        }
        
        public double CameraPositionX { get; set; }
        public double CameraPositionY { get; set; }
        public double CameraPositionZ { get; set; }

        public double CameraRotationX { get; set; }
        public double CameraRotationY { get; set; }
        public double CameraRotationZ { get; set; }

        public ObservableCollection<Light> Lights => _lights;
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public ImportedSceneDescription ImportedSceneDescription;
        private readonly ObservableCollection<Light> _lights = new ObservableCollection<Light>();

        
    }
}
