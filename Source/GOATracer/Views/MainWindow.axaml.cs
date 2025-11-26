namespace GOATracer.Views;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Importer.Obj;
using Models;
using Preview;
using ViewModels;
using System.Linq;

/// <summary>
/// Represents the main window of the GOATracer application.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// The renderer for the 3D preview.
    /// </summary>
    private PreviewRenderer? _previewRenderer;
    
    /// <summary>
    /// A flag indicating whether mouse look is active.
    /// </summary>
    private bool _mouseLookActive = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    /// <summary>
    /// Handler method for the Import option
    /// </summary>
    /// <param name="sender">Import option in the menu bar at the top</param>
    /// <param name="eventData">Event data</param>
    private async void ImportOptionClicked(object? sender, RoutedEventArgs eventData)
    {
        // Clear previous log messages
        LogOutputTextBlock.Text = "";

        // Get the parent window to enable file dialog access
        // Source: https://docs.avaloniaui.net/docs/basics/user-interface/file-dialogs
        var topLevel = TopLevel.GetTopLevel(this);

        // Show file picker dialog to let user select a .obj file to import
        // Source: https://docs.avaloniaui.net/docs/concepts/services/storage-provider/file-picker-options
        var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open .obj File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType(".obj files")
                {
                    Patterns = ["*.obj"]
                }
            ]
        });

        if (files.Count >= 1)
        {
            // Extract the local file path from the selected file
            var filePath = files[0].Path.LocalPath;
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Render);

            // Notify the user of the import starting
            LogOutputTextBlock.Text += "Importing " + filePath + "...\n\n";

            // Import the .obj file and convert it into our scene data structure
            var sceneDescription = ObjImporter.ImportModel(filePath);

            // Check if a scene was imported
            if (sceneDescription != null)
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    // Set the loaded scene in the view model
                    vm.LoadedScene = sceneDescription;

                    var cameraSettings = new CameraSettingsBinding();
                    SetupCameraBindings(vm, cameraSettings);

                    // Collect the lights specified by the user
                    var lights = vm.EnabledLights.Select(sceneLight => sceneLight.Model).ToList();
                    _previewRenderer = new PreviewRenderer(sceneDescription, lights, cameraSettings);
                    // Kill previous renderer if present (GC cleanup) and create a new renderer
                    RenderPanel.Children.Clear();
                    RenderPanel.Children.Add(_previewRenderer);

                    SetupLightListeners(vm);

                    // Notify the user that the import was successful
                    LogOutputTextBlock.Text += "Import was successful! Check output.log for more details.";
                }
            }
        }
    }

    /// <summary>
    /// Event handler for when a mouse button is pressed
    /// </summary>
    /// <param name="sender">Mouse button pressed</param>
    /// <param name="eventData">Event data</param>
    private void RenderPanel_PointerPressed(object? sender, PointerPressedEventArgs eventData)
    {
        // Exit the method if there is no preview renderer
        if (RenderPanel.Children.Count == 0)
        {
            return;
        }

        // Get the preview renderer
        var previewRenderer = (PreviewRenderer)RenderPanel.Children[0];
        previewRenderer?.Focus();

        // Check if the left mouse button is pressed
        var pt = eventData.GetCurrentPoint(RenderPanel);
        if (pt.Properties.IsLeftButtonPressed)
        {
            // Set the flag for mouse look
            _mouseLookActive = true;
            // Reset the mouse look flags in the preview renderer to eliminate mouse jump
            previewRenderer?.ResetMouseLook();
            // Capture the click
            eventData.Pointer.Capture(RenderPanel);
        }
    }

    /// <summary>
    /// Event handler for when mouse buttons are released for the preview panel
    /// </summary>
    /// <param name="sender">Released mouse button</param>
    /// <param name="eventData">Event data</param>
    private void RenderPanel_PointerReleased(object? sender, PointerReleasedEventArgs eventData)
    {
        if (eventData.InitialPressMouseButton == MouseButton.Left)
        {
            // Disable looking with the mouse in the preview
            _mouseLookActive = false;
        }
    }

    /// <summary>
    /// Event handler for mouse movements
    /// </summary>
    /// <param name="sender">Mouse movement</param>
    /// <param name="eventData">Event data</param>
    private void RenderPanel_PointerMoved(object? sender, PointerEventArgs eventData)
    {
        // If mouse look is not active or there is 
        if (!_mouseLookActive || RenderPanel.Children.Count == 0) return;

        // Get the position of the mouse
        var pos = eventData.GetPosition(RenderPanel);

        var previewRenderer = (PreviewRenderer)RenderPanel.Children[0];
        // Use the mouselook
        previewRenderer.ApplyMouseLook((float)pos.X, (float)pos.Y);
    }

    /// <summary>
    /// Sets up listeners for changes in the collection of lights.
    /// </summary>
    /// <param name="vm">The main window view model.</param>
    private void SetupLightListeners(MainWindowViewModel vm)
    {
        vm.EnabledLights.CollectionChanged += OnLightsCollectionChanged;

        foreach (var light in vm.EnabledLights)
        {
            light.PropertyChanged += OnLightPropertyChanged;
        }
    }

    /// <summary>
    /// Handles changes to the collection of lights (added or removed).
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void OnLightsCollectionChanged(object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (LightViewModel light in e.NewItems)
            {
                light.PropertyChanged += OnLightPropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (LightViewModel light in e.OldItems)
            {
                light.PropertyChanged -= OnLightPropertyChanged;
            }
        }

        UpdateRendererLights();
    }

    /// <summary>
    /// Handles property changes for an individual light.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    private void OnLightPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        UpdateRendererLights();
    }

    /// <summary>
    /// Updates the lights in the preview renderer based on the current view model state.
    /// </summary>
    private void UpdateRendererLights()
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var lights = vm.EnabledLights.Select(l => l.Model).ToList();

        _previewRenderer?.UpdateLights(lights);
    }

    /// <summary>
    /// Sets up two-way data bindings between the view model's camera properties and the camera settings binding.
    /// </summary>
    /// <param name="vm">The main window view model.</param>
    /// <param name="cameraSettings">The camera settings binding object.</param>
    private void SetupCameraBindings(MainWindowViewModel vm, CameraSettingsBinding cameraSettings)
    {
        cameraSettings.PositionX = (float)vm.CameraPositionX;
        cameraSettings.PositionY = (float)vm.CameraPositionY;
        cameraSettings.PositionZ = (float)vm.CameraPositionZ;
        cameraSettings.RotationX = (float)vm.CameraRotationX;
        cameraSettings.RotationY = (float)vm.CameraRotationY;
        cameraSettings.RotationZ = (float)vm.CameraRotationZ;

        vm.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(MainWindowViewModel.CameraPositionX):
                    cameraSettings.PositionX = (float)vm.CameraPositionX;
                    break;
                case nameof(MainWindowViewModel.CameraPositionY):
                    cameraSettings.PositionY = (float)vm.CameraPositionY;
                    break;
                case nameof(MainWindowViewModel.CameraPositionZ):
                    cameraSettings.PositionZ = (float)vm.CameraPositionZ;
                    break;
                case nameof(MainWindowViewModel.CameraRotationX):
                    cameraSettings.RotationX = (float)vm.CameraRotationX;
                    break;
                case nameof(MainWindowViewModel.CameraRotationY):
                    cameraSettings.RotationY = (float)vm.CameraRotationY;
                    break;
                case nameof(MainWindowViewModel.CameraRotationZ):
                    cameraSettings.RotationZ = (float)vm.CameraRotationZ;
                    break;
            }
        };

        cameraSettings.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(CameraSettingsBinding.PositionX):
                    vm.CameraPositionX = cameraSettings.PositionX;
                    break;
                case nameof(CameraSettingsBinding.PositionY):
                    vm.CameraPositionY = cameraSettings.PositionY;
                    break;
                case nameof(CameraSettingsBinding.PositionZ):
                    vm.CameraPositionZ = cameraSettings.PositionZ;
                    break;
                case nameof(CameraSettingsBinding.RotationX):
                    vm.CameraRotationX = cameraSettings.RotationX;
                    break;
                case nameof(CameraSettingsBinding.RotationY):
                    vm.CameraRotationY = cameraSettings.RotationY;
                    break;
                case nameof(CameraSettingsBinding.RotationZ):
                    vm.CameraRotationZ = cameraSettings.RotationZ;
                    break;
            }
        };
    }
}