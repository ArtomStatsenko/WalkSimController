using System;

namespace WalkSimController;

public sealed class StrafeTiltSettings
{
    private float _maxRollDegrees = 2.5f;
    private float _smoothTime = 0.15f;

    public float MaxRollDegrees
    {
        get => _maxRollDegrees;
        set => _maxRollDegrees = MathF.Max(value, 0f);
    }
    public float SmoothTime
    {
        get => _smoothTime;
        set => _smoothTime = MathF.Max(value, 0.001f);
    }
}
