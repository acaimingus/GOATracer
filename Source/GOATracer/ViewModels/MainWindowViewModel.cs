using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GOATracer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOATracer.ViewModels
{
    /// <summary>
    /// Represents the main view model for the application's main window, providing data binding and command handling
    /// for the user interface. 
    /// Uses the RayTracerModel as the underlying data model and interface to the model's data and logic.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        // RayTracer Model as an interface to the ViewModel
        [ObservableProperty]
        private RayTracerModel _rayTracerModel;
        // Collection of all lights, hold the same items as in the RayTracerModel
        public ObservableCollection<LightViewModel> Lights { get; } = new();
        // Collection of enabled lights for rendering and displaying in UI
        public ObservableCollection<LightViewModel> EnabledLights { get; } = new();
        //selected Light in the combobox of the UI
        [ObservableProperty]
        private LightViewModel _selectedLight;
        private int _lightCounter = 0;
        // Commands for UI interactions, DeleteLightCommand is passed to LightViewModel instances
        public IRelayCommand<string> Command { get; }
        public IRelayCommand<string> DeleteLightCommand { get; }


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
            DeleteLightCommand = new RelayCommand<string>(DeleteLight);

            AddNewLight();
            SelectedLight = Lights.First();

        }

        /// <summary>
        /// Adds a new light to the scene and updates the collection of enabled lights.
        /// </summary>
        private void AddNewLight()
        {
            _lightCounter++;
            var newLightModel = new Light() { name = $"Light {_lightCounter}", isEnabled = true };
            _rayTracerModel.Lights.Add(newLightModel);

            var newLightVM = new LightViewModel(newLightModel, DeleteLight);
            newLightVM.PropertyChanged += LightViewModel_PropertyChanged;
            Lights.Add(newLightVM);
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
            foreach (var l in Lights.Where(l => l.IsEnabled))
                EnabledLights.Add(l);
        }

        // Event handler for property changes in LightViewModel
        private void LightViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LightViewModel.IsEnabled))
                UpdateEnabledLights();
        }


        /// <summary>
        /// Executes the specified command based on the provided action string.
        /// </summary>
        /// <param name="action">A string representing the command to execute. Supported values are: 
        /// "Render": Opens a new render window.
        /// "DeleteLight": Deletes the currently selected light, if one is selected.
        /// "AddLight": Adds a new light to the scene.</param>
        public void OnCommand(string action)
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
        /// <param name="lightName">The name of the light to delete</param>
        private void DeleteLight(String lightName)
        {
            var lightVM = Lights.FirstOrDefault(l => l.Name == lightName);
            if (lightVM != null)
            {
                bool wasSelected = SelectedLight == lightVM;
                Lights.Remove(lightVM);
                _rayTracerModel.Lights.Remove(lightVM.Model);
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


        /// <summary>
        /// Property for the X position of the camera in the scene.
        /// </summary>
        [ObservableProperty]
        private double cameraPositionX;
        partial void OnCameraPositionXChanged(double value)
        {
            _rayTracerModel.CameraPositionX = value;
        }

        /// <summary>
        /// Property for the Y position of the camera in the scene.
        /// </summary>
        [ObservableProperty]
        private double cameraPositionY;
        partial void OnCameraPositionYChanged(double value)
        {
            _rayTracerModel.CameraPositionY = value;
        }

        /// <summary>
        /// Property for the Z position of the camera in the scene.
        /// </summary>
        [ObservableProperty]
        private double cameraPositionZ;
        partial void OnCameraPositionZChanged(double value)
        {
            _rayTracerModel.CameraPositionZ = value;
        }
        /// <summary>
        /// Property for the X rotation of the camera in the scene.
        /// </summary>
        [ObservableProperty]
        private double cameraRotationX;
        partial void OnCameraRotationXChanged(double value)
        {
            _rayTracerModel.CameraRotationX = value;
        }
        /// <summary>
        /// PRoperty for the Y rotation of the camera in the scene.
        /// </summary>
        [ObservableProperty]
        private double cameraRotationY;
        partial void OnCameraRotationYChanged(double value)
        {
            _rayTracerModel.CameraRotationY = value;
        }
        /// <summary>
        /// Property for the Z rotation of the camera in the scene.
        /// </summary>
        [ObservableProperty]
        private double cameraRotationZ;
        partial void OnCameraRotationZChanged(double value)
        {
            _rayTracerModel.CameraRotationZ = value;
        }
    }
}

