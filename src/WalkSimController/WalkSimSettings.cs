namespace WalkSimController;

public sealed class WalkSimSettings
{
    private float _maxDeltaTime = 1f / 30f;

    public MovementSettings Movement { get; } = new();
    public MouseSettings Mouse { get; } = new();
    public HeadBobSettings HeadBob { get; } = new();
    public StrafeTiltSettings StrafeTilt { get; } = new();
    public TurnSwaySettings TurnSway { get; } = new();
    public BreathingSettings Breathing { get; } = new();
    public BodycamSettings Bodycam { get; } = new();

    /// <summary>Upper dt limit for camera comfort during frame spikes.</summary>
    public float MaxDeltaTime
    {
        get => _maxDeltaTime;
        set => _maxDeltaTime = Math.Clamp(value, 1f / 240f, 0.2f);
    }
}
