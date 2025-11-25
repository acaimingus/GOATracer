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
        /// <summary>
        /// Constructor for the RayTracerModel
        /// </summary>
        /// <param name="importedSceneDescription">Scene to be used</param>
        public RayTracerModel(ImportedSceneDescription importedSceneDescription)
        {
            this.ImportedSceneDescription = importedSceneDescription;
        }

        /// <summary>
        /// Constructor for when there is no scene yet
        /// </summary>
        public RayTracerModel()
        {
        }
        
        private readonly ObservableCollection<Light> _lights = new ObservableCollection<Light>();
        
        /// <summary>
        /// Gets or sets the X-coordinate of the camera's position.
        /// </summary>
        public double CameraPositionX { get; set; }
        
        /// <summary>
        /// Gets or sets the Y-coordinate of the camera's position.
        /// </summary>
        public double CameraPositionY { get; set; }
        
        /// <summary>
        /// Gets or sets the Z-coordinate of the camera's position.
        /// </summary>
        public double CameraPositionZ { get; set; }

        /// <summary>
        /// Gets or sets the X-axis rotation of the camera.
        /// </summary>
        public double CameraRotationX { get; set; }
        
        /// <summary>
        /// Gets or sets the Y-axis rotation of the camera.
        /// </summary>
        public double CameraRotationY { get; set; }
        
        /// <summary>
        /// Gets or sets the Z-axis rotation of the camera.
        /// </summary>
        public double CameraRotationZ { get; set; }

        /// <summary>
        /// A collection of lights in the scene.
        /// </summary>
        public ObservableCollection<Light> Lights => _lights;

        /// <summary>
        /// The width of the output image in pixels.
        /// </summary>
        public int ImageWidth { get; set; }
        
        /// <summary>
        /// The height of the output image in pixels.
        /// </summary>
        public int ImageHeight { get; set; }
        
        /// <summary>
        /// The description of the imported 3D scene.
        /// </summary>
        public ImportedSceneDescription? ImportedSceneDescription;
    }
}
