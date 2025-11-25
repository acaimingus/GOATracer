using System;
using CommunityToolkit.Mvvm.Input;
using GOATracer.Lights;

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
        public LightViewModel(Light light, Action<int> deleteAction)
        {
            _light = light;
            DeleteCommand = new RelayCommand(() => deleteAction?.Invoke(Id));
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
        public float LightPositionX
        {
            get => _light.X;
            set
            {
                if (_light.X != value) {

                    _light.X = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Y-coordinate of the light's position
        /// </summary>
        public float LightPositionY
        {
            get => _light.Y;
            set
            {
                if (_light.Y != value) {

                    _light.Y = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Z-coordinate of the light's position
        /// </summary>
        public float LightPositionZ
        {
            get => _light.Z;
            set
            {
                if (_light.Z != value) {

                    _light.Z = value;
                    OnPropertyChanged();
                }
            }
        }



    }
}
