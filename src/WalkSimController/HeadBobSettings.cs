using System;

namespace WalkSimController
{

public sealed class HeadBobSettings
{
    private float _frequencyHz = 1.9f;
    private float _amplitudeX = 0.013f;
    private float _amplitudeY = 0.008f;
    private float _decaySpeed = 6f;
    private float _phaseDecay = 8f;

    // Per-step angular impulses for a rotational bodycam feel.
    private float _pitchImpulse = 0.18f; // degrees
    private float _yawDrift = 0.06f; // degrees, subtle side-to-side drift

    public float FrequencyHz
    {
        get => _frequencyHz;
        set => _frequencyHz = MathF.Max(value, 0.01f);
    }
    public float AmplitudeX
    {
        get => _amplitudeX;
        set => _amplitudeX = MathF.Max(value, 0f);
    }
    public float AmplitudeY
    {
        get => _amplitudeY;
        set => _amplitudeY = MathF.Max(value, 0f);
    }
    public float DecaySpeed
    {
        get => _decaySpeed;
        set => _decaySpeed = MathF.Max(value, 0.01f);
    }
    public float PhaseDecay
    {
        get => _phaseDecay;
        set => _phaseDecay = MathF.Max(value, 0.01f);
    }

    /// <summary>Per-step pitch impulse. 0 = off, around 0.15-0.25 = bodycam feel.</summary>
    public float PitchImpulse
    {
        get => _pitchImpulse;
        set => _pitchImpulse = MathF.Max(value, 0f);
    }

    /// <summary>Subtle yaw drift. 0 = off, around 0.05-0.10 = bodycam feel.</summary>
    public float YawDrift
    {
        get => _yawDrift;
        set => _yawDrift = MathF.Max(value, 0f);
    }
}

}
