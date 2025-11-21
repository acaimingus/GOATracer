using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GOATracer.ViewModels
{

    // Light Wrapper so the light model doesnt need to implement INotifyPropertyChanged
    public class LightViewModel : ViewModelBase
    {
        // The underlying Light model
        private readonly Light _light;
        // Command to delete this light, invokes the provided action with the light's name
        public IRelayCommand DeleteCommand { get; }


        public LightViewModel(Light light, Action<string> deleteAction)
        {
            _light = light;
            DeleteCommand = new RelayCommand(() => deleteAction?.Invoke(Name));
        }

        // Properties that wrap the Light model's properties and notify on changes
        public string Name
        {
            get => _light.name;
            set { 
                if (_light.name != value)
                {
                    _light.name = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEnabled
        {
            get => _light.isEnabled;
            set
            {
                if (_light.isEnabled != value)
                {
                    _light.isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public double LightPositionX
        {
            get => _light.LightPositionX;
            set
            {
                if (_light.LightPositionX != value)
                {
                    _light.LightPositionX = value;
                    OnPropertyChanged();
                }
            }
        }

        public double LightPositionY
        {
            get => _light.LightPositionY;
            set
            {
                if (_light.LightPositionY != value)
                {
                    _light.LightPositionY = value;
                    OnPropertyChanged();
                }
            }
        }

        public double LightPositionZ
        {
            get => _light.LightPositionZ;
            set
            {
                if (_light.LightPositionZ != value)
                {
                    _light.LightPositionZ = value;
                    OnPropertyChanged();
                }
            }
        }

        // Exposes the underlying Light model
        public Light Model => _light;

    }
}
