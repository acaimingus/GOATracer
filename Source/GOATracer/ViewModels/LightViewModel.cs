using System;
using CommunityToolkit.Mvvm.Input;

namespace GOATracer.ViewModels
{
    /// <summary>
    /// Light Wrapper so the light model doesn't need to implement INotifyPropertyChanged
    /// </summary>
    public class LightViewModel : ViewModelBase
    {
        /// <summary>
        /// The underlying Light model
        /// </summary>
        private readonly Light _light;
        /// <summary>
        /// Command to delete this light, invokes the provided action with the light's name
        /// </summary>
        public IRelayCommand DeleteCommand { get; }

        /// <summary>
        /// Gets the light model associated with this instance.
        /// </summary>
        public Light Model => _light;

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
            get => _light.Name;
            set { 
                if (_light.Name != value) {

                    _light.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Id
        {
            get => _light.Id;
            set
            {
                if (_light.Id != value) {
                    _light.Id = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Property to get or set whether the light is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => _light.IsEnabled;
            set
            {
                if (_light.IsEnabled != value) {

                    _light.IsEnabled = value;
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
                if (_light.LightPositionX != value) {

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
                if (_light.LightPositionY != value) {

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
                if (_light.LightPositionZ != value) {

                    _light.LightPositionZ = value;
                    OnPropertyChanged();
                }
            }
        }



    }
}
