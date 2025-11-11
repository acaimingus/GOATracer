using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GOATracer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOATracer.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        [ObservableProperty]
        private RayTracerModel _rayTracerModel;
        public ObservableCollection<Light> Lights => _rayTracerModel.Lights;

        public ObservableCollection<Light> EnabledLights { get;} = new ObservableCollection<Light>();




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
            
            Lights.Add(new Light { name = "Light 1", isEnabled = false });

        }

        public void SetLightEnabled(Light light, bool enabled)
        {
            light.isEnabled = enabled;
            EnabledLights.Clear();
            foreach (var l in Lights.Where(l => l.isEnabled))
                EnabledLights.Add(l);
            if (enabled && !EnabledLights.Contains(light))
                EnabledLights.Add(light);
            else if (!enabled && EnabledLights.Contains(light))
                EnabledLights.Remove(light);
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
                    if (SelectedLight != null)
                    {
                        Lights.Remove(SelectedLight);
                    }
                    break;
                case "AddLight":
                    var newLight = new Light() { name = $"Light {Lights.Count + 1}", isEnabled = true };
                    Lights.Add(newLight);
                    break;

                default:
                    break;
            }
            
            
        }

        private void DeleteLight(String lightName)
        {
            var light = Lights.FirstOrDefault(l => l.name == lightName);
            if (light != null)
                Lights.Remove(light);
        }


        [ObservableProperty]
        private Light selectedLight;

        [ObservableProperty]
        private double cameraPositionX;
        partial void OnCameraPositionXChanged(double value)
        {
            _rayTracerModel.CameraPositionX = value;
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

