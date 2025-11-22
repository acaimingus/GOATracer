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

        /// <summary>
        /// Wraps a Light model and provides property change notifications.
        /// </summary>
        /// <param name="light">Underlying light instance to expose to the UI</param>
        /// <param name="deleteAction">Command used to notify MainWindowViewModel that this Light should be removed by name</param>
        public LightViewModel(Light light, Action<string> deleteAction)
        {
            _light = light;
            DeleteCommand = new RelayCommand(() => deleteAction?.Invoke(Name));
        }

        /// <summary>
        /// Property to get or set the name of the light
        /// </summary>
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

        /// <summary>
        /// Property to get or set whether the light is enabled
        /// </summary>
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

        /// <summary>
        /// Gets or sets the X-coordinate of the light's position
        /// </summary>
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

        /// <summary>
        /// Gets or sets the Y-coordinate of the light's position
        /// </summary>
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

        /// <summary>
        /// Gets or sets the Z-coordinate of the light's position
        /// </summary>
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

        /// <summary>
        /// Gets the light model associated with this instance.
        /// </summary>
        public Light Model => _light;

    }
}
