namespace WinPet.Domain;

// Smoothed cursor direction and proximity; visuals decide whether and how to react.
internal readonly record struct CursorAttention(float X, float Y, float Attention)
{
    public const float Radius = 240;

    public static CursorAttention Toward(PointF origin, PointF cursor)
    {
        float dx = cursor.X - origin.X, dy = cursor.Y - origin.Y;
        float distance = MathF.Sqrt(dx * dx + dy * dy);
        if (distance >= Radius) return default;
        float divisor = Math.Max(32, distance);
        // Ease out near the boundary; avoid a sudden flip as the cursor crosses the origin.
        return new(dx / divisor, dy / divisor, Math.Clamp((Radius - distance) / 48, 0, 1));
    }

    public CursorAttention Approach(CursorAttention target, float dt)
    {
        float blend = 1 - MathF.Exp(-Math.Max(0, dt) / 0.09f);
        return new(X + (target.X - X) * blend, Y + (target.Y - Y) * blend,
            Attention + (target.Attention - Attention) * blend);
    }
}
