using Avalonia;
using GOATracer.Cameras;
using GOATracer.Lights;
using GOATracer.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOATracer.Preview
{
    public class PreviewScene
    {
        private Camera _camera;
        private readonly CameraSettingsBinding _cameraSettings;
        private List<Vector3> _lights;

        /// <summary>
        /// Constructor
        /// </summary>
        public PreviewScene(List<Light> lights, CameraSettingsBinding cameraSettings)
        {
            // Default 16:9 aspect ratio for initialization. This will be updated when the control size is known.
            _camera = new Camera(Vector3.UnitZ * 3, 1.77778f); 
            UpdateLights(lights);
            _cameraSettings = cameraSettings;
            _cameraSettings.UiCameraUpdate += OnCameraSettingsChangedFromUi;
        }

        /// <summary>
        /// Updates the light positions in the preview scene.
        /// </summary>
        /// <param name="lights"></param>
        public void UpdateLights(List<Light> lights)
        {
            _lights = new List<Vector3>();

            if (lights.Count > 0)
            {
                // Convert each light from the light object list to a 3d point and save it in the local light list
                foreach (var vector in lights.Select(light => new Vector3(light.X, light.Y, light.Z)))
                {
                    _lights.Add(vector);
                }
            }
        }

        /// <summary>
        /// Gets the list of light positions.
        /// </summary>
        /// <returns></returns>
        public List<Vector3> GetLights()
        {
            return _lights;
        }

        /// <summary>
        /// Updates the camera's position and orientation based on the current UI settings.
        /// </summary>
        /// <remarks>This method synchronizes the camera's position and rotation with the values specified
        /// in the associated UI settings. It should be called whenever the camera settings are  modified through the
        /// user interface to ensure the camera reflects the updated values.</remarks>
        public void OnCameraSettingsChangedFromUi()
        {
            _camera.Position = new Vector3(_cameraSettings.PositionX, _cameraSettings.PositionY, _cameraSettings.PositionZ);
            _camera.Pitch = _cameraSettings.RotationX;
            _camera.Yaw = _cameraSettings.RotationY;
        }

        /// <summary>
        /// Configures the camera with the size of the control and initializes event handling for camera updates.
        /// </summary>
        /// <param name="size">The dimensions of the control, used to calculate the camera's aspect ratio.</param>
        public void SetupCamera(Size size)
        {
            // set camera on position (0,0,3) and aspect ratio according to the control size
            _camera = new Camera(Vector3.UnitZ * 3, (float)(size.Width / size.Height));
            _cameraSettings.UiCameraUpdate += OnCameraSettingsChangedFromUi;
        }

        /// <summary>
        /// Gets the current camera instance.
        /// </summary>
        /// <returns>The Camera instance currently in use.</returns>
        public Camera GetCamera()
        {
            return _camera;
        }

        /// <summary>
        /// Updates the camera's position and rotation in the associated camera settings.
        /// </summary>
        public void UpdateCameraPositionToBinding()
        {
            _cameraSettings.UpdatePosition(_camera.Position.X, _camera.Position.Y, _camera.Position.Z);
            _cameraSettings.UpdateRotation(_camera.Pitch, _camera.Yaw, 0f);
        }

        /// <summary>
        /// Retrieves the current camera settings binding.
        /// </summary>
        public CameraSettingsBinding GetCameraSettingsBinding()
        {
            return _cameraSettings;
        }
    }
}
