
using System.Collections.ObjectModel;
using GOATracer.Importer.Obj;


namespace GOATracer
{


    // Model representing the Ray Tracer settings and scene description
    public class RayTracerModel
    {

        public RayTracerModel(ImportedSceneDescription importedSceneDescription)
        {
            this._importedSceneDescription = importedSceneDescription;
        }

        public RayTracerModel() { }


        public double CameraPositionX { get; set; }
        public double CameraPositionY { get; set; }
        public double CameraPositionZ { get; set; }

        public double CameraRotationX { get; set; }
        public double CameraRotationY { get; set; }
        public double CameraRotationZ { get; set; }

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public ImportedSceneDescription _importedSceneDescription;
        public ObservableCollection<Light> Lights { get; } = new ObservableCollection<Light>();
    }
}
