using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GOATracer.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using GOATracer.Lights;
using GOATracer.Models;

namespace GOATracer.ViewModels
{
    /// <summary>
    /// Represents the main view model for the application's main window, providing data binding and command handling
    /// for the user interface. 
    /// Uses the RayTracerModel as the underlying data model and interface to the model's data and logic.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        /// <summary>
        /// Collection of all lights, hold the same items as in the RayTracerModel
        /// </summary>
        public ObservableCollection<LightViewModel> Lights { get; } = new();
        /// <summary>
        /// Collection of enabled lights for rendering and displaying in UI
        /// </summary>
        public ObservableCollection<LightViewModel> EnabledLights { get; } = new();
        /// <summary>
        /// Commands for UI interactions, DeleteLightCommand is passed to LightViewModel instances
        /// </summary>
        public IRelayCommand<string> Command { get; }
        /// <summary>
        /// Command for Deleting the lights
        /// </summary>
        public RelayCommand<int> DeleteLightCommand { get; }
        /// <summary>
        /// Selected Light in the combobox of the UI
        /// </summary>
        [ObservableProperty]
        private LightViewModel _selectedLight;
        /// <summary>
        /// RayTracer Model as an interface to the ViewModel
        /// </summary>
        [ObservableProperty]
        private RayTracerModel _rayTracerModel;
        /// <summary>
        /// Counter for the light amount
        /// </summary>
        private int _lightCounter = 0;

        /// <summary>
        /// Property for the X position of the camera in the scene.
        /// </summary>
        [ObservableProperty]
        private double _cameraPositionX;
        partial void OnCameraPositionXChanged(double value)
        {
            _rayTracerModel.CameraPositionX = value;
        }

        /// <summary>
        /// Property for the Y position of the camera in the scene.
        /// </summary>
        [ObservableProperty]
        private double _cameraPositionY;
        partial void OnCameraPositionYChanged(double value)
        {
            _rayTracerModel.CameraPositionY = value;
        }

        /// <summary>
        /// Property for the Z position of the camera in the scene.
        /// </summary>
        [ObservableProperty]
        private double _cameraPositionZ;
        partial void OnCameraPositionZChanged(double value)
        {
            _rayTracerModel.CameraPositionZ = value;
        }
        /// <summary>
        /// Property for the X rotation of the camera in the scene.
        /// </summary>
        [ObservableProperty]
        private double _cameraRotationX;
        partial void OnCameraRotationXChanged(double value)
        {
            _rayTracerModel.CameraRotationX = value;
        }
        /// <summary>
        /// PRoperty for the Y rotation of the camera in the scene.
        /// </summary>
        [ObservableProperty]
        private double _cameraRotationY;
        partial void OnCameraRotationYChanged(double value)
        {
            _rayTracerModel.CameraRotationY = value;
        }
        /// <summary>
        /// Property for the Z rotation of the camera in the scene.
        /// </summary>
        [ObservableProperty]
        private double _cameraRotationZ;
        partial void OnCameraRotationZChanged(double value)
        {
            _rayTracerModel.CameraRotationZ = value;
        }

        public MainWindowViewModel()
        {
            // Initialize RayTracerModel with default camera settings
            _rayTracerModel = new RayTracerModel()
            {
                CameraPositionX = 1,
                CameraPositionY = 1,
                CameraPositionZ = 1,
                CameraRotationX = 1,
                CameraRotationY = 1,
                CameraRotationZ = 1,
                ImageWidth = 800,
                ImageHeight = 600
            };


            // Initialize commands
            Command = new RelayCommand<string>(OnCommand);
            DeleteLightCommand = new RelayCommand<int>(DeleteLight);

            AddNewLight();
            SelectedLight = Lights.First();

        }

        /// <summary>
        /// Adds a new light to the scene and updates the collection of enabled lights.
        /// </summary>
        private void AddNewLight()
        {
            _lightCounter++;
            var newLightModel = new Light(_lightCounter);
            RayTracerModel.Lights.Add(newLightModel);

            var newLightVm = new LightViewModel(newLightModel, DeleteLight);
            newLightVm.PropertyChanged += LightViewModel_PropertyChanged;
            Lights.Add(newLightVm);
            UpdateEnabledLights();
        }

        /// <summary>
        /// Updates the collection of enabled lights by clearing the current list and adding all lights that are
        /// enabled. This ensures that the EnabledLights collection always reflects the current state of the Lights collection.
        /// Also important to keep the enabled lights in sync for the UI and rendering
        /// </summary>
        private void UpdateEnabledLights()
        {
            EnabledLights.Clear();
            foreach (var l in Lights.Where(l => l.IsEnabled)) {
                EnabledLights.Add(l);
            }
        }

        // Event handler for property changes in LightViewModel
        private void LightViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LightViewModel.IsEnabled)) {

                UpdateEnabledLights();
            }
        }


        /// <summary>
        /// Executes the specified command based on the provided action string.
        /// </summary>
        /// <param name="action">A string representing the command to execute. Supported values are: 
        /// "Render": Opens a new render window.
        /// "DeleteLight": Deletes the currently selected light, if one is selected.
        /// "AddLight": Adds a new light to the scene.</param>
        private void OnCommand(string action)
        {
            switch (action)
            {
                case "Render":
                    var renderWindow = new RenderWindow();
                    renderWindow.Show();
                    break;
                case "AddLight":
                    AddNewLight();
                    break;

                default:
                    break;
            }


        }

        /// <summary>
        /// Deletes a light with the specified name from the collection of lights in both the ViewModel and the Model.
        /// Deletion executed by name, not by reference.
        /// </summary>
        /// <param name="lightId">The ID of the light to delete</param>
        private void DeleteLight(int lightId)
        {
            var lightVm = Lights.FirstOrDefault(l => l.Id == lightId);
            if (lightVm != null)
            {

                var wasSelected = SelectedLight == lightVm;
                Lights.Remove(lightVm);
                RayTracerModel.Lights.Remove(lightVm.Model);
                UpdateEnabledLights();

                if (wasSelected)
                {

                    if (Lights.Count > 0)
                    {

                        SelectedLight = Lights.First();
                    }
                    else
                    {
                        SelectedLight = null;
                    }
                }

            }
        }

    }
}

