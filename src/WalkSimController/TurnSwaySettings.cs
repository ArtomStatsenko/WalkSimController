using System;

namespace WalkSimController
{

public sealed class TurnSwaySettings
{
    private float _amount = 0.012f;
    private float _maxRollDegrees = 3f;
    private float _smoothTime = 0.18f;

    public float Amount
    {
        get => _amount;
        set => _amount = MathF.Max(value, 0f);
    }
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

}
