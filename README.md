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

## Player Rig Schematic

Use a small transform hierarchy where engine physics owns the body position, the root body owns yaw, the camera rig owns bodycam rotation, and the camera owns local bob/breathing offset.

```text
PlayerBody                         engine character body
|                                  moves with BodyVelocity through engine physics
|                                  rotates only around Y with RawCameraYaw
|
+-- CameraRig / HeadPivot           local child of PlayerBody
    |                               rotates with CameraRotation pitch/yaw-offset/roll
    |                               do not move this with physics
    |
    +-- Camera                      local child of CameraRig
                                    moves locally with CameraLocalOffset
```

Frame ownership:

```text
FrameInput
  MoveAxis + MouseDelta
      |
      v
WalkSimController.Update(dt, input)
      |
      v
WalkSimState
  BodyVelocity       -> engine character movement
  RawCameraYaw       -> PlayerBody yaw
  CameraRotation     -> CameraRig local rotation
  CameraLocalOffset  -> Camera local position
```

After the engine moves the character and resolves collisions, call `SyncBodyPosition` with the real body position. The controller predicts desired motion, but the engine remains the source of truth for collision-corrected body position.

Unity mapping:

```text
GameObject with CharacterController + PlayerUnityAdapter
|
+-- CameraRig Transform
    |
    +-- Camera
```

Godot mapping:

```text
CharacterBody3D with PlayerGodotAdapter
|
+-- CameraRig Node3D
    |
    +-- Camera3D
```

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

## Compatibility Policy

The core library targets `netstandard2.1` and intentionally uses C# 9-compatible syntax. This keeps the source usable in Unity and Godot projects that compile copied source files with an older engine-managed C# compiler.

Do not require newer language features such as file-scoped namespaces, primary constructors, required members, collection expressions, or global using directives in `src/WalkSimController`. Engine compiler support can lag behind the installed local .NET SDK, so a local build with `LangVersion=latest` is not enough.

Before raising the language version, verify the lowest supported Unity and Godot versions compile the source directly. The minimum compatibility check is:

```bash
dotnet build src/WalkSimController/WalkSimController.csproj /p:LangVersion=9.0
dotnet test WalkSimController.sln
```

## License

MIT. See `LICENSE`.
