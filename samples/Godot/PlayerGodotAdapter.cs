using System;
using Godot;
using WalkSimController;
using CoreVector2 = System.Numerics.Vector2;
using CoreVector3 = System.Numerics.Vector3;

public partial class PlayerGodotAdapter : CharacterBody3D
{
    [Export]
    public Node3D CameraRig = null!;

    [Export]
    public Camera3D Camera = null!;

    private readonly WalkSimController.WalkSimController _controller = new(CoreVector3.Zero);
    private Vector2 _mouseDelta;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventMouseMotion motion)
            _mouseDelta += motion.Relative;
    }

    public override void _PhysicsProcess(double delta)
    {
        var state = _controller.Update((float)delta, GatherInput());
        _mouseDelta = Vector2.Zero;

        Velocity = new Vector3(state.BodyVelocity.X, Velocity.Y, -state.BodyVelocity.Z);
        MoveAndSlide();

        _controller.SyncBodyPosition(ToCore(GlobalPosition));
        ApplyCamera(state);
    }

    private FrameInput GatherInput() =>
        new()
        {
            MoveAxis = new CoreVector2(
                Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
                Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_back")
            ),
            MouseDelta = new CoreVector2(_mouseDelta.X, _mouseDelta.Y),
        };

    private void ApplyCamera(WalkSimState state)
    {
        Rotation = new Vector3(0f, DegToRad(-state.RawCameraYaw), 0f);

        var bodycamYawOffset = DeltaAngleDeg(state.RawCameraYaw, state.CameraRotation.Yaw);
        CameraRig.Rotation = new Vector3(
            DegToRad(state.CameraRotation.Pitch),
            DegToRad(-bodycamYawOffset),
            DegToRad(state.CameraRotation.Roll)
        );

        Camera.Position = new Vector3(
            state.CameraLocalOffset.X,
            state.CameraLocalOffset.Y,
            state.CameraLocalOffset.Z
        );
    }

    private static CoreVector3 ToCore(Vector3 v) => new(v.X, v.Y, -v.Z);

    private static float DegToRad(float deg) => deg * (MathF.PI / 180f);

    private static float DeltaAngleDeg(float from, float to)
    {
        var delta = (to - from) % 360f;
        if (delta > 180f)
            delta -= 360f;
        if (delta <= -180f)
            delta += 360f;
        return delta;
    }
}
