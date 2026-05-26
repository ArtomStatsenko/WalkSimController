using System;
using System.Numerics;

namespace WalkSimController;

internal static class Math
{
    /// <summary>
    /// Critically damped spring, similar to Unity SmoothDamp.
    /// Stable for any dt; when dt is much larger than smoothTime the result
    /// remains finite, though some overshoot is possible.
    /// </summary>
    public static float Spring(float cur, float tgt, ref float vel, float smoothTime, float dt)
    {
        if (smoothTime < 1e-6f)
        {
            vel = 0f;
            return tgt;
        }
        var omega = 2f / smoothTime;
        var x = omega * dt;
        var exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
        var delta = cur - tgt;
        var temp = (vel + omega * delta) * dt;
        vel = (vel - omega * temp) * exp;
        var result = tgt + (delta + temp) * exp;
        if ((tgt - cur > 0f) == (result > tgt))
        {
            result = tgt;
            vel = 0f;
        }
        return result;
    }

    public static Vector3 Spring3(Vector3 cur, Vector3 tgt, ref Vector3 vel, float st, float dt)
    {
        return new Vector3(
            Spring(cur.X, tgt.X, ref vel.X, st, dt),
            Spring(cur.Y, tgt.Y, ref vel.Y, st, dt),
            Spring(cur.Z, tgt.Z, ref vel.Z, st, dt)
        );
    }

    public static Vector2 ClampMagnitude1(Vector2 @this)
    {
        if (!float.IsFinite(@this.X) || !float.IsFinite(@this.Y))
            return default;
        var len = @this.Length();
        return len > 1f ? new Vector2(@this.X / len, @this.Y / len) : @this;
    }

    /// <summary>
    /// Angle-safe spring: moves along the shortest path between angles,
    /// avoiding the long route through +/-180 degrees.
    /// </summary>
    public static float SpringAngle(float cur, float tgt, ref float vel, float smoothTime, float dt)
    {
        var delta = DeltaAngle(cur, tgt);
        var target = cur + delta;
        var result = Spring(cur, target, ref vel, smoothTime, dt);
        return WrapAngle(result);
    }

    /// <summary>Wraps an angle to (-180, 180].</summary>
    public static float WrapAngle(float deg)
    {
        deg %= 360f;
        if (deg > 180f)
            deg -= 360f;
        if (deg <= -180f)
            deg += 360f;
        return deg;
    }

    /// <summary>Shortest signed difference between two angles, in the -180..180 range.</summary>
    public static float DeltaAngle(float from, float to) => WrapAngle(to - from);

    public static float Clamp(float v, float lo, float hi) =>
        v < lo
            ? lo
            : v > hi
                ? hi
                : v;

    public static float Clamp01(float v) => Clamp(v, 0f, 1f);

    public static float Lerp(float a, float b, float t) => a + (b - a) * t;

    /// <summary>Lerp protected against t > 1 during frame spikes.</summary>
    public static float SafeLerp(float a, float b, float dt, float speed) =>
        Lerp(a, b, Clamp01(dt * speed));

    public static float SmoothNoise01(float phase) =>
        Clamp01(0.5f + 0.25f * MathF.Sin(phase) + 0.25f * MathF.Sin(phase * MathF.E + 1.3f));

    public static float WrapCycle(float value, float cycle)
    {
        if (!float.IsFinite(value) || cycle <= 0f)
            return 0f;
        value %= cycle;
        return value < 0f ? value + cycle : value;
    }
}
