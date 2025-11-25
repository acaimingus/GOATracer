using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GOATracer.Models;

public sealed class CameraSettingsBinding : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public event Action? UiCameraUpdate;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private float _positionX;
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
    private float _positionY;
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
    private float _positionZ;
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
    private float _rotationX;
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
    private float _rotationY;
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
    private float _rotationZ;
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

    public void UpdatePosition(float x, float y, float z)
    {
        _positionX = x;
        _positionY = y;
        _positionZ = z;
        OnPropertyChanged(nameof(PositionX));
        OnPropertyChanged(nameof(PositionY));
        OnPropertyChanged(nameof(PositionZ));
    }

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