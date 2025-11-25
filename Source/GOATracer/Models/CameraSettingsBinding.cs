using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GOATracer.Models;

/// <summary>
/// Binds camera settings to the UI, allowing for two-way updates.
/// </summary>
public sealed class CameraSettingsBinding : INotifyPropertyChanged
{
    /// <summary>
    /// Private backing field for the X-coordinate of the camera's position.
    /// </summary>
    private float _positionX;

    /// <summary>
    /// Private backing field for the Y-coordinate of the camera's position.
    /// </summary>
    private float _positionY;

    /// <summary>
    /// Private backing field for the Z-coordinate of the camera's position.
    /// </summary>
    private float _positionZ;

    /// <summary>
    /// Private backing field for the X-coordinate of the camera's rotation.
    /// </summary>
    private float _rotationX;

    /// <summary>
    /// Private backing field for the Y-coordinate of the camera's rotation.
    /// </summary>
    private float _rotationY;

    /// <summary>
    /// Private backing field for the Z-coordinate of the camera's rotation.
    /// </summary>
    private float _rotationZ;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Occurs when the camera needs to be updated from the UI.
    /// </summary>
    public event Action? UiCameraUpdate;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    /// <summary>
    /// Gets or sets the X-coordinate of the camera's position.
    /// </summary>
    public float PositionX
    {
        get => _positionX;
        set
        {
            if (_positionX != value)
            {
                _positionX = value;
                OnPropertyChanged();
                UiCameraUpdate?.Invoke();
            }
        }
    }

    /// <summary>
    /// Gets or sets the Y-coordinate of the camera's position.
    /// </summary>
    public float PositionY
    {
        get => _positionY;
        set
        {
            if (_positionY != value)
            {
                _positionY = value;
                OnPropertyChanged();
                UiCameraUpdate?.Invoke();
            }
        }
    }


    /// <summary>
    /// Gets or sets the Z-coordinate of the camera's position.
    /// </summary>
    public float PositionZ
    {
        get => _positionZ;
        set
        {
            if (_positionZ != value)
            {
                _positionZ = value;
                OnPropertyChanged();
                UiCameraUpdate?.Invoke();
            }
        }
    }


    /// <summary>
    /// Gets or sets the X-coordinate of the camera's rotation.
    /// </summary>
    public float RotationX
    {
        get => _rotationX;
        set
        {
            if (_rotationX != value)
            {
                _rotationX = value;
                OnPropertyChanged();
                UiCameraUpdate?.Invoke();
            }
        }
    }

    /// <summary>
    /// Gets or sets the Y-coordinate of the camera's rotation.
    /// </summary>
    public float RotationY
    {
        get => _rotationY;
        set
        {
            if (_rotationY != value)
            {
                _rotationY = value;
                OnPropertyChanged();
                UiCameraUpdate?.Invoke();
            }
        }
    }

    /// <summary>
    /// Gets or sets the Z-coordinate of the camera's rotation.
    /// </summary>
    public float RotationZ
    {
        get => _rotationZ;
        set
        {
            if (_rotationZ != value)
            {
                _rotationZ = value;
                OnPropertyChanged();
                UiCameraUpdate?.Invoke();
            }
        }
    }

    /// <summary>
    /// Updates the camera's position.
    /// </summary>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="z">Z position</param>
    public void UpdatePosition(float x, float y, float z)
    {
        _positionX = x;
        _positionY = y;
        _positionZ = z;
        OnPropertyChanged(nameof(PositionX));
        OnPropertyChanged(nameof(PositionY));
        OnPropertyChanged(nameof(PositionZ));
    }

    /// <summary>
    /// Updates the camera's rotation.
    /// </summary>
    /// <param name="x">X rotation</param>
    /// <param name="y">Y rotation</param>
    /// <param name="z">Z rotation</param>
    public void UpdateRotation(float x, float y, float z)
    {
        _rotationX = x;
        _rotationY = y;
        _rotationZ = z;
        OnPropertyChanged(nameof(RotationX));
        OnPropertyChanged(nameof(RotationY));
        OnPropertyChanged(nameof(RotationZ));
    }
}