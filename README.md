[![Ko-Fi](https://img.shields.io/badge/Ko--fi-Support-ff5e5b?logo=ko-fi&logoColor=white)](https://ko-fi.com/artomstatsenko)

# WalkSimController

Engine-agnostic first-person walking and bodycam camera controller for Godot, Unity, and plain .NET projects.

WalkSimController keeps movement and camera feel in a small .NET library. It has no dependency on Godot or Unity runtime types, so engine integration stays as a thin adapter layer.

## Features

- Smooth first-person walking velocity with diagonal input clamping.
- Mouse look with yaw wrapping, pitch clamps, and optional look lag.
- Head bob, strafe tilt, turn sway, and breathing camera motion.
- Deterministic per-frame API that returns an immutable `WalkSimState`.
- `System.Numerics` inputs and outputs for easy use from game engines or simulations.
- NUnit test coverage for core movement, mouse, reset, and sync behavior.

## Project Layout

```text
src/WalkSimController/          Core .NET library
tests/WalkSimController.Tests/  NUnit tests
samples/Godot/                  Minimal Godot C# adapter
samples/Unity/                  Minimal Unity C# adapter
```

## Quick Start

Reference `src/WalkSimController/WalkSimController.csproj` from your game project, or copy the `src/WalkSimController` source into your solution. The core library targets `netstandard2.1` for broad compatibility with Unity, Godot C#, and modern .NET projects.

```csharp
using System.Numerics;
using WalkSimController;

var controller = new WalkSimController.WalkSimController(Vector3.Zero);

var state = controller.Update(
    1f / 60f,
    new FrameInput
    {
        MoveAxis = new Vector2(0f, 1f),
        MouseDelta = new Vector2(2f, -1f),
    }
);

// Apply state.BodyVelocity to your character body.
// Apply state.CameraRotation and state.CameraLocalOffset to your camera rig.
```

After your engine resolves collisions, sync the real body position back into the controller:

```csharp
controller.SyncBodyPosition(realBodyPosition);
```

For teleport, respawn, or loading a saved game:

```csharp
controller.Reset(position, yaw: 90f, pitch: 0f);
```

## Engine Integration

The controller does not move engine objects directly. Each frame:

1. Convert engine input into `FrameInput`.
2. Call `Update(dt, input)`.
3. Apply `BodyVelocity` through your engine character movement.
4. Sync the post-collision body position with `SyncBodyPosition`.
5. Apply `CameraRotation` and `CameraLocalOffset` to your camera rig.

See `samples/Godot/PlayerGodotAdapter.cs` and `samples/Unity/PlayerUnityAdapter.cs` for minimal adapters.

## Settings

`WalkSimSettings` groups the tuning surface:

- `Movement`: walk speed and velocity smoothing.
- `Mouse`: sensitivity, pitch clamps, and look lag.
- `HeadBob`: positional and rotational step motion.
- `StrafeTilt`: roll while strafing.
- `TurnSway`: roll response while turning.
- `Breathing`: idle and walking breathing motion.
- `Bodycam`: global comfort/intensity limits.

You can pass settings into the constructor:

```csharp
var settings = new WalkSimSettings();
settings.Movement.WalkSpeed = 4.5f;
settings.Mouse.Sensitivity = 0.08f;
settings.Bodycam.MotionIntensity = 0.7f;

var controller = new WalkSimController.WalkSimController(Vector3.Zero, settings);
```

## Build and Test

```bash
dotnet build
dotnet test
```

## License

MIT. See `LICENSE`.
