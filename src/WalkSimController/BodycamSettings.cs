namespace WalkSimController
{

public sealed class BodycamSettings
{
    private float _motionIntensity = 1f;
    private float _angularIntensity = 1f;
    private float _microVariation = 0.22f;
    private float _maxStepImpulseDebt = 2f;
    private float _maxTotalRollDegrees = 4f;
    private float _maxPitchAdditiveDegrees = 2f;

    /// <summary>
    /// Main comfort control: 0 = clean controlled camera, 1 = lively bodycam,
    /// 1.2-1.5 = stronger and less comfortable.
    /// </summary>
    public float MotionIntensity
    {
        get => _motionIntensity;
        set => _motionIntensity = Math.Clamp(value, 0f, 1.5f);
    }

    /// <summary>
    /// Separate control for yaw/pitch/roll bodycam motion. Lower this first
    /// if the camera feels alive but starts to cause motion sickness.
    /// </summary>
    public float AngularIntensity
    {
        get => _angularIntensity;
        set => _angularIntensity = Math.Clamp(value, 0f, 1.5f);
    }

    /// <summary>Soft step variation so bob does not read like a perfect metronome.</summary>
    public float MicroVariation
    {
        get => _microVariation;
        set => _microVariation = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>Limits accumulated step impulse debt during frame spikes.</summary>
    public float MaxStepImpulseDebt
    {
        get => _maxStepImpulseDebt;
        set => _maxStepImpulseDebt = Math.Clamp(value, 0f, 4f);
    }

    /// <summary>Final safety clamp for the sum of roll effects.</summary>
    public float MaxTotalRollDegrees
    {
        get => _maxTotalRollDegrees;
        set => _maxTotalRollDegrees = Math.Clamp(value, 0f, 12f);
    }

    /// <summary>Maximum pitch added on top of raw pitch by look lag and step impulses.</summary>
    public float MaxPitchAdditiveDegrees
    {
        get => _maxPitchAdditiveDegrees;
        set => _maxPitchAdditiveDegrees = Math.Clamp(value, 0f, 8f);
    }
}

}
