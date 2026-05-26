using System;

namespace WalkSimController;

public sealed class MouseSettings
{
    private float _sensitivity = 0.15f;
    private float _lookLagSmoothTime = 0f;
    private float _pitchClampUp = 80f;
    private float _pitchClampDown = 80f;

    public float Sensitivity
    {
        get => _sensitivity;
        set => _sensitivity = MathF.Max(value, 0f);
    }

    /// <summary>0 = no lag; 0.03-0.06 = bodycam feel.</summary>
    public float LookLagSmoothTime
    {
        get => _lookLagSmoothTime;
        set => _lookLagSmoothTime = MathF.Max(value, 0f);
    }
    public float PitchClampUp
    {
        get => _pitchClampUp;
        set => _pitchClampUp = Math.Clamp(value, 0f, 90f);
    }
    public float PitchClampDown
    {
        get => _pitchClampDown;
        set => _pitchClampDown = Math.Clamp(value, 0f, 90f);
    }
}
