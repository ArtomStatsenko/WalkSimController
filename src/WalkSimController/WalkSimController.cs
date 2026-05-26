// WalkSimController.cs — v3.1
// Update order: MouseLook → Movement → HeadBob → StrafeTilt → TurnSway → Breathing → Compose
//
//   BodyPosition
//     + HeadAnchorLocal
//     + CameraLocalOffset
//     → apply CameraRotation

using System;
using System.Numerics;

namespace WalkSimController
{
    public sealed class WalkSimController
    {
        private const float StepAngleSeedFactor = 0.28f;
        private const float StepAngularVelocityFactor = 46f;
        private const float SecondaryStepImpulseScale = 0.45f;
        private const float Tau = MathF.PI * 2f;

        public WalkSimSettings Settings { get; }

        // ── Mouse ────────────────────────────────────────────────────────────
        private float _rawYaw;
        private float _rawPitch;
        private float _prevRawYaw;
        private float _smoothYaw;
        private float _smoothPitch;
        private float _lookVelYaw;
        private float _lookVelPitch;

        // ── Movement ─────────────────────────────────────────────────────────
        private Vector3 _bodyPos;
        private Vector3 _bodyVel;
        private Vector3 _velSpring;
        private float _speedRatio;
        private float _bodyYaw;
        private float _movementYaw;

        // ── Head bob ─────────────────────────────────────────────────────────
        private float _bobTimer;
        private Vector3 _bobOffset;
        private Vector3 _bobOffsetVel;
        private float _bobVariationPhase;

        // Rotational parts of the head bob bodycam feel.
        private float _bobPitchAngle;
        private float _bobPitchVel;
        private float _bobYawAngle;
        private float _bobYawVel;

        // ── Strafe tilt ──────────────────────────────────────────────────────
        private float _strafeTiltRoll;
        private float _strafeTiltVel;

        // ── Turn sway ────────────────────────────────────────────────────────
        private float _turnSwayRoll;
        private float _turnSwayVel;

        // ── Breathing ────────────────────────────────────────────────────────
        private float _breathTimer;
        private float _breathBlend;
        private float _breathOffsetY;
        private float _breathOffsetYVel;
        private float _breathRoll;
        private float _breathRollVel;

        // ─────────────────────────────────────────────────────────────────────

        public WalkSimController(Vector3 startPosition, WalkSimSettings? settings = null)
        {
            _bodyPos = startPosition;
            Settings = settings ?? new WalkSimSettings();
        }

        // Sync after collision resolution. Only the position changes; spring state is preserved.
        public void SyncBodyPosition(Vector3 realPosition) => _bodyPos = realPosition;

        /// <summary>
        /// Resets all internal state for respawn, load, or teleport.
        /// Clears spring velocity, bob, and breathing so old state does not leak into the new pose.
        /// </summary>
        public void Reset(Vector3 position, float yaw = 0f, float pitch = 0f)
        {
            _bodyPos = position;
            _bodyVel = Vector3.Zero;
            _velSpring = Vector3.Zero;
            _speedRatio = 0f;
            _bodyYaw = yaw;
            _movementYaw = yaw;

            _rawYaw = yaw;
            _rawPitch = pitch;
            _prevRawYaw = yaw;
            _smoothYaw = yaw;
            _smoothPitch = pitch;
            _lookVelYaw = 0f;
            _lookVelPitch = 0f;

            _bobTimer = 0f;
            _bobOffset = Vector3.Zero;
            _bobOffsetVel = Vector3.Zero;
            _bobVariationPhase = 0f;
            _bobPitchAngle = 0f;
            _bobPitchVel = 0f;
            _bobYawAngle = 0f;
            _bobYawVel = 0f;

            _strafeTiltRoll = 0f;
            _strafeTiltVel = 0f;
            _turnSwayRoll = 0f;
            _turnSwayVel = 0f;

            _breathTimer = 0f;
            _breathBlend = 0f;
            _breathOffsetY = 0f;
            _breathOffsetYVel = 0f;
            _breathRoll = 0f;
            _breathRollVel = 0f;
        }

        /// <summary>
        /// Updates the controller for one frame.
        /// Returns an atomic snapshot; read only the returned state.
        /// dt must be greater than 0 and finite. Invalid dt values skip the frame.
        /// </summary>
        public WalkSimState Update(float dt, FrameInput input)
        {
            if (!float.IsFinite(dt) || dt <= 0f)
                return BuildState();

            dt = MathF.Min(dt, Settings.MaxDeltaTime);

            // Order matters: mouse, movement, effects, compose.
            UpdateMouseLook(dt, input.MouseDelta);
            UpdateMovement(dt, input.MoveAxis);
            UpdateHeadBob(dt);
            UpdateStrafeTilt(dt);
            UpdateTurnSway(dt);
            UpdateBreathing(dt);

            return BuildState();
        }

        // ─── 1. MouseLook ─────────────────────────────────────────────────────

        private void UpdateMouseLook(float dt, Vector2 mouse)
        {
            var s = Settings.Mouse;

            _prevRawYaw = _rawYaw;

            if (float.IsFinite(mouse.X) && float.IsFinite(mouse.Y))
            {
                _rawYaw = Math.WrapAngle(_rawYaw + mouse.X * s.Sensitivity);
                _rawPitch = Math.Clamp(
                    _rawPitch - mouse.Y * s.Sensitivity,
                    -s.PitchClampDown,
                    s.PitchClampUp
                );
            }

            if (s.LookLagSmoothTime > 1e-6f)
            {
                // SpringAngle follows the shortest path and stays stable at +/-180 degrees.
                _smoothYaw = Math.SpringAngle(
                    _smoothYaw,
                    _rawYaw,
                    ref _lookVelYaw,
                    s.LookLagSmoothTime,
                    dt
                );
                _smoothPitch = Math.Spring(
                    _smoothPitch,
                    _rawPitch,
                    ref _lookVelPitch,
                    s.LookLagSmoothTime,
                    dt
                );
            }
            else
            {
                _smoothYaw = _rawYaw;
                _smoothPitch = _rawPitch;
            }
        }

        // ─── 2. Movement ──────────────────────────────────────────────────────

        private void UpdateMovement(float dt, Vector2 moveAxis)
        {
            var s = Settings.Movement;
            var dir = Math.ClampMagnitude1(moveAxis);

            _movementYaw = _rawYaw; // Walking follows immediate camera control.
            var yawRad = _movementYaw * (MathF.PI / 180f);
            var cos = MathF.Cos(yawRad);
            var sin = MathF.Sin(yawRad);

            Vector3 target =
                new(
                    (dir.X * cos + dir.Y * sin) * s.WalkSpeed,
                    0f,
                    (dir.Y * cos - dir.X * sin) * s.WalkSpeed
                );

            _bodyVel = Math.Spring3(_bodyVel, target, ref _velSpring, s.SmoothTime, dt);

            _bodyPos = new Vector3(
                _bodyPos.X + _bodyVel.X * dt,
                _bodyPos.Y,
                _bodyPos.Z + _bodyVel.Z * dt
            );

            var horizSpeed = MathF.Sqrt(_bodyVel.X * _bodyVel.X + _bodyVel.Z * _bodyVel.Z);
            _speedRatio = Math.Clamp01(horizSpeed / s.WalkSpeed);
            UpdateBodyYaw();
        }

        // ─── 3. Head bob ──────────────────────────────────────────────────────
        //
        //   Positional: X = A*sin(2*pi*f*t), Y = B*sin(4*pi*f*t), Lissajous figure-8.
        //   Angular: per-step pitch impulse and yaw drift for bodycam feel.

        private void UpdateHeadBob(float dt)
        {
            var s = Settings.HeadBob;
            var b = Settings.Bodycam;
            var moving = _speedRatio > 0.05f;

            var prevTimer = _bobTimer;

            if (moving)
            {
                _bobVariationPhase += dt * Tau * 0.17f;
                _bobVariationPhase = Math.WrapCycle(_bobVariationPhase, Tau * 1000f);
                var variation =
                    1f + (Math.SmoothNoise01(_bobVariationPhase) - 0.5f) * 0.16f * b.MicroVariation;
                _bobTimer +=
                    dt * Tau * s.FrequencyHz * (0.7f + _speedRatio * 0.3f) * variation;
                if (_bobTimer > Tau * 1000f)
                {
                    var reduced = Math.WrapCycle(_bobTimer, Tau);
                    prevTimer -= _bobTimer - reduced;
                    _bobTimer = reduced;
                }
            }
            else
                _bobTimer = Math.SafeLerp(_bobTimer, 0f, dt, s.PhaseDecay);

            var ampVariation = moving
                ? 1f
                    + (Math.SmoothNoise01(_bobVariationPhase + 1.91f) - 0.5f)
                        * 0.22f
                        * b.MicroVariation
                : 1f;

            // Positional bob.
            var bobTarget = moving
                ? new Vector3(
                    MathF.Sin(_bobTimer) * s.AmplitudeX * ampVariation,
                    MathF.Sin(_bobTimer * 2f) * s.AmplitudeY * ampVariation,
                    0f
                )
                : Vector3.Zero;

            var decayST = moving ? 0.04f : 1f / s.DecaySpeed;
            _bobOffset = Math.Spring3(_bobOffset, bobTarget, ref _bobOffsetVel, decayST, dt);

            var pitchTarget = 0f;
            var yawTarget = 0f;

            var stepEvents = moving ? CountDownwardZeroCrossings(prevTimer, _bobTimer) : 0;
            if (stepEvents > 0)
            {
                var cappedEvents = System
                    .Math
                    .Min(stepEvents, (int)MathF.Ceiling(b.MaxStepImpulseDebt));
                for (var i = 0; i < cappedEvents; i++)
                {
                    var impulseScale = i == 0 ? 1f : SecondaryStepImpulseScale;
                    var yawSide = MathF.Sin((_bobTimer - i * MathF.PI) * 0.5f) > 0f ? 1f : -1f;

                    // Impulse split: a small angle seed plus spring velocity. The constants are
                    // tuned for the default 0.12s/0.18s decay so lag spikes still leave a visible step.
                    _bobPitchAngle += -s.PitchImpulse * StepAngleSeedFactor * impulseScale;
                    _bobYawAngle += yawSide * s.YawDrift * StepAngleSeedFactor * impulseScale;
                    _bobPitchVel += -s.PitchImpulse * StepAngularVelocityFactor * impulseScale;
                    _bobYawVel += yawSide * s.YawDrift * StepAngularVelocityFactor * impulseScale;
                }
            }

            _bobPitchAngle = Math.Spring(
                _bobPitchAngle,
                pitchTarget,
                ref _bobPitchVel,
                moving ? 0.12f : 0.08f,
                dt
            );
            _bobYawAngle = Math.Spring(
                _bobYawAngle,
                yawTarget,
                ref _bobYawVel,
                moving ? 0.18f : 0.10f,
                dt
            );
        }

        private static int CountDownwardZeroCrossings(float fromPhase, float toPhase)
        {
            if (toPhase <= fromPhase)
                return 0;

            var first = MathF.PI;
            var fromIndex = (int)MathF.Floor((fromPhase - first) / Tau);
            var toIndex = (int)MathF.Floor((toPhase - first) / Tau);

            var count = 0;
            for (var i = fromIndex; i <= toIndex; i++)
            {
                var crossing = first + i * Tau;
                if (crossing > fromPhase && crossing <= toPhase)
                    count++;
            }

            return count;
        }

        // ─── 4. Strafe tilt ───────────────────────────────────────────────────
        // Roll from real body velocity, synced with movement rather than raw input.

        private void UpdateStrafeTilt(float dt)
        {
            var s = Settings.StrafeTilt;

            var yawRad = _rawYaw * (MathF.PI / 180f);
            var lateralSpeed = _bodyVel.X * MathF.Cos(yawRad) + _bodyVel.Z * (-MathF.Sin(yawRad));

            var factor = Math.Clamp(lateralSpeed / Settings.Movement.WalkSpeed, -1f, 1f);
            _strafeTiltRoll = Math.Spring(
                _strafeTiltRoll,
                -factor * s.MaxRollDegrees,
                ref _strafeTiltVel,
                s.SmoothTime,
                dt
            );
        }

        // ─── 5. Turn sway ─────────────────────────────────────────────────────
        // DeltaAngle is safe across the +/-180 degree wrap.
        // yawSpeed is in deg/s, so the effect is frame-rate independent.

        private void UpdateTurnSway(float dt)
        {
            var s = Settings.TurnSway;

            var yawDelta = Math.DeltaAngle(_prevRawYaw, _rawYaw);
            var yawSpeed = yawDelta / dt; // deg/s

            var target = Math.Clamp(-yawSpeed * s.Amount, -s.MaxRollDegrees, s.MaxRollDegrees);

            _turnSwayRoll = Math.Spring(_turnSwayRoll, target, ref _turnSwayVel, s.SmoothTime, dt);
        }

        // ─── 6. Breathing ─────────────────────────────────────────────────────

        private void UpdateBreathing(float dt)
        {
            var s = Settings.Breathing;

            _breathTimer += dt;
            _breathTimer = Math.WrapCycle(_breathTimer, 1000f);

            var breathTarget = _speedRatio < 0.05f ? 1f : s.WalkBlend;
            _breathBlend = Math.SafeLerp(_breathBlend, breathTarget, dt, s.BlendSpeed);

            var breathY =
                MathF.Sin(_breathTimer * Tau * s.FrequencyHz) * s.AmplitudeY * _breathBlend;
            var breathRoll =
                MathF.Sin(_breathTimer * Tau * s.FrequencyHz * s.SecondaryFreqMultiplier)
                * s.AmplitudeRoll
                * _breathBlend;

            _breathOffsetY = Math.Spring(_breathOffsetY, breathY, ref _breathOffsetYVel, 0.1f, dt);
            _breathRoll = Math.Spring(_breathRoll, breathRoll, ref _breathRollVel, 0.1f, dt);
        }

        // ─── 7. Compose + snapshot ────────────────────────────────────────────

        private WalkSimState BuildState()
        {
            var motion = Settings.Bodycam.MotionIntensity;
            var angular = Settings.Bodycam.AngularIntensity * motion;

            // MotionIntensity is the master comfort multiplier; AngularIntensity is subordinate to it.
            var cameraYaw =
                _rawYaw + Math.DeltaAngle(_rawYaw, _smoothYaw) * angular + _bobYawAngle * angular;

            // LookLagSmoothTime computes the lag target; Bodycam intensities blend it into the visual camera.
            var rawPitchAdditive = (_smoothPitch - _rawPitch) * angular + _bobPitchAngle * angular;
            var pitchAdditive = Math.Clamp(
                rawPitchAdditive,
                -Settings.Bodycam.MaxPitchAdditiveDegrees,
                Settings.Bodycam.MaxPitchAdditiveDegrees
            );
            var cameraPitch = Math.Clamp(
                _rawPitch + pitchAdditive,
                -Settings.Mouse.PitchClampDown,
                Settings.Mouse.PitchClampUp
            );
            var roll = (_strafeTiltRoll + _turnSwayRoll + _breathRoll) * angular;
            roll = Math.Clamp(
                roll,
                -Settings.Bodycam.MaxTotalRollDegrees,
                Settings.Bodycam.MaxTotalRollDegrees
            );

            var rot = new CameraAngles
            {
                Yaw = Math.WrapAngle(cameraYaw),
                Pitch = cameraPitch,
                Roll = roll
            };

            var offset = new Vector3(
                _bobOffset.X * motion,
                (_bobOffset.Y + _breathOffsetY) * motion,
                0f
            );

            return new WalkSimState(
                _bodyPos,
                _bodyVel,
                _bodyYaw,
                _movementYaw,
                _rawYaw,
                rot,
                offset,
                _speedRatio
            );
        }

        private void UpdateBodyYaw()
        {
            var horizSpeedSq = _bodyVel.X * _bodyVel.X + _bodyVel.Z * _bodyVel.Z;
            if (horizSpeedSq > 0.0001f)
                _bodyYaw = Math.WrapAngle(MathF.Atan2(_bodyVel.X, _bodyVel.Z) * (180f / MathF.PI));
            else
                _bodyYaw = _rawYaw;
        }
    }
}
