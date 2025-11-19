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
    public class LightViewModel : ObservableObject
    {
        private readonly Light _light;
        public IRelayCommand DeleteCommand { get; }

        public LightViewModel(Light light, Action<string> deleteAction)
        {
            _light = light;
            DeleteCommand = new RelayCommand(() => deleteAction?.Invoke(Name));
        }

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

        public Light Model => _light;

    }
}
