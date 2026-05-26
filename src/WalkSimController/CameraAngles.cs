namespace WalkSimController;

public struct CameraAngles
{
    /// <summary>Body yaw around Y, in degrees, wrapped to (-180, 180].</summary>
    public float Yaw;

    /// <summary>Head pitch up/down, in degrees.</summary>
    public float Pitch;

    /// <summary>Camera roll from strafe tilt, turn sway, and breathing, in degrees.</summary>
    public float Roll;

    public override string ToString() => $"Yaw={Yaw:F2}° Pitch={Pitch:F2}° Roll={Roll:F2}°";
}
