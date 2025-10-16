using System;
using System.Numerics;

namespace GOATracer.Preview;

public class Camera
{
    private readonly float _far = 100f;
    private readonly float _fov = 60f;
    private readonly float _near = 0.1f;
    public float Aspect = 1.0f;
    public Vector3 Position { get; set; } = new(0, 0, 3);
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public Matrix4x4 GetViewMatrix()
    {
        // Calculate base (Pitch/Yaw/Roll)
        var q = Quaternion.CreateFromYawPitchRoll(
            Rotation.Y * MathF.PI / 180f,
            Rotation.X * MathF.PI / 180f,
            Rotation.Z * MathF.PI / 180f);
        var forward = Vector3.Transform(-Vector3.UnitZ, q);
        var up = Vector3.Transform(Vector3.UnitY, q);
        return Matrix4x4.CreateLookAt(Position, Position + forward, up);
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(
            _fov * MathF.PI / 180f, Aspect, _near, _far);
    }
}