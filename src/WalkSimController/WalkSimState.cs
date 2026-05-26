using System.Numerics;

namespace WalkSimController
{

/// <summary>
/// Atomic frame snapshot: read this instead of separate controller properties.
/// This avoids reading partial intermediate state.
///
/// Recommended engine application order:
///   worldCameraPos = BodyPosition + rotate(HeadAnchorLocal, Yaw) + rotate(CameraLocalOffset, Yaw+Pitch)
///   worldCameraRot = CameraRotation (Yaw / Pitch / Roll)
/// </summary>
public readonly struct WalkSimState
{
    public readonly Vector3 BodyPosition;
    public readonly Vector3 BodyVelocity;

    /// <summary>Body and walking direction without decorative bodycam offsets.</summary>
    public readonly float BodyYaw;

    /// <summary>Yaw used for movement during this frame.</summary>
    public readonly float MovementYaw;

    /// <summary>Raw control yaw without look lag or decorative offsets.</summary>
    public readonly float RawCameraYaw;
    public readonly CameraAngles CameraRotation;

    /// <summary>Camera offset in local head space from bob and breathing.</summary>
    public readonly Vector3 CameraLocalOffset;

    /// <summary>0 = idle, 1 = full speed. Useful for footsteps, breathing, and effects.</summary>
    public readonly float SpeedRatio;

    internal WalkSimState(
        Vector3 pos,
        Vector3 vel,
        float bodyYaw,
        float movementYaw,
        float rawCameraYaw,
        CameraAngles rot,
        Vector3 offset,
        float speed
    )
    {
        BodyPosition = pos;
        BodyVelocity = vel;
        BodyYaw = bodyYaw;
        MovementYaw = movementYaw;
        RawCameraYaw = rawCameraYaw;
        CameraRotation = rot;
        CameraLocalOffset = offset;
        SpeedRatio = speed;
    }
}

}
