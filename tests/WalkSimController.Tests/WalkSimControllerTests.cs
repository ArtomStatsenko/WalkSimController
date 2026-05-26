using System.Numerics;
using WalkSimController;
using NUnit.Framework;

namespace WalkSimController.Tests;

[TestFixture]
public sealed class WalkSimControllerTests
{
    [Test]
    public void Update_WithInvalidDeltaTime_DoesNotMutateState()
    {
        var controller = NewController();

        var before = controller.Update(0.016f, new FrameInput { MoveAxis = new Vector2(0f, 1f) });
        var after = controller.Update(
            float.NaN,
            new FrameInput
            {
                MoveAxis = new Vector2(1f, 0f),
                MouseDelta = new Vector2(1000f, 1000f),
            }
        );

        Assert.That(after.BodyPosition.X, Is.EqualTo(before.BodyPosition.X).Within(0.0001f));
        Assert.That(after.BodyPosition.Z, Is.EqualTo(before.BodyPosition.Z).Within(0.0001f));
        Assert.That(after.RawCameraYaw, Is.EqualTo(before.RawCameraYaw).Within(0.0001f));
        Assert.That(
            after.CameraRotation.Pitch,
            Is.EqualTo(before.CameraRotation.Pitch).Within(0.0001f)
        );
    }

    [Test]
    public void Update_WithDiagonalMoveAxis_ClampsMovementToWalkSpeed()
    {
        var controller = NewController();
        WalkSimState state = default;

        for (var i = 0; i < 120; i++)
            state = controller.Update(1f / 60f, new FrameInput { MoveAxis = new Vector2(1f, 1f) });

        var horizontalSpeed = MathF.Sqrt(
            state.BodyVelocity.X * state.BodyVelocity.X
                + state.BodyVelocity.Z * state.BodyVelocity.Z
        );

        Assert.That(
            horizontalSpeed,
            Is.EqualTo(controller.Settings.Movement.WalkSpeed).Within(0.001f)
        );
        Assert.That(state.SpeedRatio, Is.LessThanOrEqualTo(1.0001f));
    }

    [Test]
    public void Update_WithMouseDelta_WrapsYawAndClampsPitch()
    {
        var settings = NewSettings();
        settings.Mouse.Sensitivity = 1f;
        settings.Mouse.PitchClampUp = 45f;
        settings.Mouse.PitchClampDown = 30f;

        var controller = new WalkSimController(Vector3.Zero, settings);
        var state = controller.Update(
            0.016f,
            new FrameInput { MouseDelta = new Vector2(200f, -100f), }
        );

        Assert.That(state.RawCameraYaw, Is.EqualTo(-160f).Within(0.0001f));
        Assert.That(state.CameraRotation.Pitch, Is.EqualTo(45f).Within(0.0001f));

        state = controller.Update(0.016f, new FrameInput { MouseDelta = new Vector2(0f, 100f) });
        Assert.That(state.CameraRotation.Pitch, Is.EqualTo(-30f).Within(0.0001f));
    }

    [Test]
    public void Reset_ClearsVelocityAndRestoresPose()
    {
        var controller = NewController();

        for (var i = 0; i < 30; i++)
            controller.Update(1f / 60f, new FrameInput { MoveAxis = new Vector2(0f, 1f) });

        controller.Reset(new Vector3(2f, 3f, 4f), yaw: 90f, pitch: -10f);
        var state = controller.Update(0f, default);

        Assert.That(state.BodyPosition.X, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(state.BodyPosition.Y, Is.EqualTo(3f).Within(0.0001f));
        Assert.That(state.BodyPosition.Z, Is.EqualTo(4f).Within(0.0001f));
        Assert.That(state.BodyVelocity.Length(), Is.EqualTo(0f).Within(0.0001f));
        Assert.That(state.RawCameraYaw, Is.EqualTo(90f).Within(0.0001f));
        Assert.That(state.CameraRotation.Pitch, Is.EqualTo(-10f).Within(0.0001f));
        Assert.That(state.CameraLocalOffset.Length(), Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void SyncBodyPosition_PreservesHorizontalVelocity()
    {
        var controller = NewController();

        var moving = controller.Update(1f / 60f, new FrameInput { MoveAxis = new Vector2(0f, 1f) });
        controller.SyncBodyPosition(new Vector3(10f, 2f, -5f));
        var synced = controller.Update(0f, default);

        Assert.That(synced.BodyPosition.X, Is.EqualTo(10f).Within(0.0001f));
        Assert.That(synced.BodyPosition.Y, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(synced.BodyPosition.Z, Is.EqualTo(-5f).Within(0.0001f));
        Assert.That(synced.BodyVelocity.X, Is.EqualTo(moving.BodyVelocity.X).Within(0.0001f));
        Assert.That(synced.BodyVelocity.Z, Is.EqualTo(moving.BodyVelocity.Z).Within(0.0001f));
    }

    private static WalkSimController NewController() => new(Vector3.Zero, NewSettings());

    private static WalkSimSettings NewSettings()
    {
        var settings = new WalkSimSettings();
        settings.Bodycam.MotionIntensity = 0f;
        settings.Mouse.LookLagSmoothTime = 0f;
        return settings;
    }
}
