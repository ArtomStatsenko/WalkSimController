using System.Numerics;

namespace WalkSimController
{

public struct FrameInput
{
    /// <summary>WASD / stick input: X = strafe, Y = forward. Any vector is accepted and clamped.</summary>
    public Vector2 MoveAxis;

    /// <summary>Mouse delta for the current frame.</summary>
    public Vector2 MouseDelta;
}

}
