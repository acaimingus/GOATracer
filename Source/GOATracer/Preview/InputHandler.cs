using Avalonia.Input;
using GOATracer.Cameras;
using GOATracer.MVC;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;


namespace GOATracer.Preview
{
    public class InputHandler
    {
        private float _cameraSpeed;
        private bool _firstMove;
        private Vector2 _lastPos;
        private readonly HashSet<Key> _keys = [];
        private PreviewScene _previewScene;
        private readonly CameraSettingsBinding _cameraSettings;

        public InputHandler(PreviewScene previewScene)
        {
            _firstMove = true;
            _cameraSpeed = 0.5f;
            _previewScene = previewScene;
            _cameraSettings = _previewScene.GetCameraSettingsBinding();
        }

        /// <summary>
        /// Handles keyboard input for camera movement.
        /// Source: https://github.com/opentk/LearnOpenTK/blob/master/Chapter2/2-BasicLighting/Window.cs
        /// </summary>
        public void HandleKeyboard()
        {
            // Boolean specifying if the camera has been moved
            var cameraMoved = false;

            var camera = _previewScene.GetCamera();

            // Slow down camera speed
            if (_keys.Contains(Key.O))
            {
                _cameraSpeed -= 0.01f;

                // Bound the minimum speed of the camera to 0.1f
                if (_cameraSpeed < 0.1f)
                {
                    _cameraSpeed = 0.1f;
                }
            }

            // Speed up camera speed
            if (_keys.Contains(Key.P))
            {
                _cameraSpeed += 0.01f;
            }

            // Move camera forward
            if (_keys.Contains(Key.W))
            {
                camera.Position += camera.Front * _cameraSpeed;
                cameraMoved = true;
            }

            // Move camera backward
            if (_keys.Contains(Key.S))
            {
                camera.Position -= camera.Front * _cameraSpeed;
                cameraMoved = true;
            }

            // Move camera to the left
            if (_keys.Contains(Key.A))
            {
                camera.Position -= camera.Right * _cameraSpeed;
                cameraMoved = true;
            }

            // Move camera to the right
            if (_keys.Contains(Key.D))
            {
                camera.Position += camera.Right * _cameraSpeed;
                cameraMoved = true;
            }

            // Raise the camera
            if (_keys.Contains(Key.Space))
            {
                camera.Position += camera.Up * _cameraSpeed;
                cameraMoved = true;
            }

            // Lower the camera
            if (_keys.Contains(Key.LeftShift) || _keys.Contains(Key.RightShift))
            {
                camera.Position -= camera.Up * _cameraSpeed;
                cameraMoved = true;
            }

            // Check if the camera has been moved and update the binding if it has
            if (cameraMoved)
            {
                _cameraSettings.UpdatePosition(camera.Position.X, camera.Position.Y, camera.Position.Z);
            }
        }

        /// <summary>
        /// Event handler for key presses
        /// </summary>
        /// <param name="eventData">Event data</param>
        public void OnKeyDown(KeyEventArgs eventData)
        {
            _keys.Add(eventData.Key);
        }

        /// <summary>
        /// Event handler for key releases
        /// </summary>
        /// <param name="eventData"></param>
        public void OnKeyUp(KeyEventArgs eventData)
        {
            _keys.Remove(eventData.Key);
        }

        /// <summary>
        /// Updates the camera orientation based on mouse movement.
        /// Source: https://github.com/opentk/LearnOpenTK/blob/master/Chapter2/2-BasicLighting/Window.cs
        /// </summary>
        /// <param name="mouseX">Horizontal mouse movement</param>
        /// <param name="mouseY">Vertical mouse movement</param>
        public void ApplyMouseLook(float mouseX, float mouseY)
        {
            // Mouse sensitivity
            const float sensitivity = 0.4f;

            var camera = _previewScene.GetCamera();

            // current mouse position
            var mouse = new Vector2(mouseX, mouseY);
            if (_firstMove)
            {
                _lastPos = mouse;
                _firstMove = false;
            }
            else
            {
                // mouse movement on X axis
                var deltaX = mouse.X - _lastPos.X;
                // mouse movement on Y axis
                var deltaY = mouse.Y - _lastPos.Y;
                // update last position of the mouse
                _lastPos = mouse;
                // camera horizontal rotation
                camera.Yaw += deltaX * sensitivity;
                // camera vertical rotation
                camera.Pitch -= deltaY * sensitivity;

                // Update the binding of the camera settings
                // Rotate the 2 dimensions by 90 degrees because of the different orientation
                _cameraSettings.UpdateRotation(camera.Yaw + 90.0f, camera.Pitch - 90.0f, 0.0f);
            }
        }

        /// <summary>
        /// Helper method for resetting MouseLook for each new click to remove mouse jump
        /// </summary>
        public void ResetMouseLook()
        {
            _firstMove = true;
        }
    }
}
