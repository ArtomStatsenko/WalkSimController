using System;

namespace WalkSimController
{

public sealed class BreathingSettings
{
    private float _frequencyHz = 0.25f;
    private float _amplitudeY = 0.003f;
    private float _amplitudeRoll = 0.4f;
    private float _secondaryFreqMultiplier = 0.7f;
    private float _blendSpeed = 3f;
    private float _walkBlend = 0.15f;

    public float FrequencyHz
    {
        get => _frequencyHz;
        set => _frequencyHz = MathF.Max(value, 0.001f);
    }
    public float AmplitudeY
    {
        get => _amplitudeY;
        set => _amplitudeY = MathF.Max(value, 0f);
    }
    public float AmplitudeRoll
    {
        get => _amplitudeRoll;
        set => _amplitudeRoll = MathF.Max(value, 0f);
    }
    public float SecondaryFreqMultiplier
    {
        get => _secondaryFreqMultiplier;
        set => _secondaryFreqMultiplier = MathF.Max(value, 0.01f);
    }
    public float BlendSpeed
    {
        get => _blendSpeed;
        set => _blendSpeed = MathF.Max(value, 0.01f);
    }

    /// <summary>Breathing amount while walking: 0 = off, around 0.1-0.2 = alive but comfortable.</summary>
    public float WalkBlend
    {
        get => _walkBlend;
        set => _walkBlend = Math.Clamp01(value);
    }
}

}
