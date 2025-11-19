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
    public partial class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        [ObservableProperty]
        private RayTracerModel _rayTracerModel;
        public ObservableCollection<LightViewModel> Lights { get; } = new();
        public ObservableCollection<LightViewModel> EnabledLights { get; } = new ();

        [ObservableProperty]
        private LightViewModel _selectedLight;
        private int _lightCounter = 0;

        public IRelayCommand<string> Command { get; }
        public IRelayCommand<string> DeleteLightCommand { get; }


        public MainWindowViewModel()
        {

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



            Command = new RelayCommand<string>(OnCommand);
            DeleteLightCommand = new RelayCommand<string>(DeleteLight);

            AddNewLight();
            SelectedLight = Lights.First();

        }
        private void AddNewLight() { 
            _lightCounter++;
            var newLightModel = new Light() { name = $"Light {_lightCounter}", isEnabled = true };
            _rayTracerModel.Lights.Add(newLightModel);

            var newLightVM = new LightViewModel(newLightModel, DeleteLight);
            newLightVM.PropertyChanged += LightViewModel_PropertyChanged;
            Lights.Add(newLightVM);
            UpdateEnabledLights();
        }

        private void UpdateEnabledLights()
        {
            EnabledLights.Clear();
            foreach (var l in Lights.Where(l => l.IsEnabled))
                EnabledLights.Add(l);
        }

        private void LightViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LightViewModel.IsEnabled))
                UpdateEnabledLights();
        }



        public void OnCommand(string action)
        {
            switch (action)
            {
                case "Render":
                    var renderWindow = new RenderWindow();
                    renderWindow.Show();
                    break;

                case "DeleteLight":
                    if (_selectedLight != null)
                    {
                        Lights.Remove(_selectedLight);
                        _rayTracerModel.Lights.Remove(_selectedLight.Model);
                        UpdateEnabledLights();
                    }
                    break;

                case "AddLight":
                    AddNewLight();
                    break;

                default:
                    break;
            }
            
            
        }

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

        [ObservableProperty]
        private double cameraPositionX;
        partial void OnCameraPositionXChanged(double value)
        {
            Debug.WriteLine($"[ViewModel] CameraPositionX changed to {value}");
            _rayTracerModel.CameraPositionX = value; // should trigger model print
        }

        [ObservableProperty]
        private double cameraPositionY;
        partial void OnCameraPositionYChanged(double value)
        {
            _rayTracerModel.CameraPositionY = value;
        }

        [ObservableProperty]
        private double cameraPositionZ;
        partial void OnCameraPositionZChanged(double value)
        {
            _rayTracerModel.CameraPositionZ= value;
        }
        [ObservableProperty]
        private double cameraRotationX;
        partial void OnCameraRotationXChanged(double value)
        {
            _rayTracerModel.CameraRotationX = value;
        }
        [ObservableProperty]
        private double cameraRotationY;
        partial void OnCameraRotationYChanged(double value)
        {
            _rayTracerModel.CameraRotationY = value;
        }
        [ObservableProperty]
        private double cameraRotationZ;
        partial void OnCameraRotationZChanged(double value)
        {
            _rayTracerModel.CameraRotationZ = value;
        }
    }
}

