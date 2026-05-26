using System;

namespace WalkSimController;

public sealed class MovementSettings
{
    private float _walkSpeed = 3.2f;
    private float _smoothTime = 0.12f;

    public float WalkSpeed
    {
        get => _walkSpeed;
        set => _walkSpeed = MathF.Max(value, 0.01f);
    }
    public float SmoothTime
    {
        get => _smoothTime;
        set => _smoothTime = MathF.Max(value, 0.001f);
    }
}
