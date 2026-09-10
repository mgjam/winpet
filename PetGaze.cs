namespace WinPet;

// Shared attention behavior; each pet decides how its face expresses the direction.
internal readonly record struct PetGaze(float X, float Y, float Attention)
{
    public const float Radius = 240;

    public static PetGaze Toward(PointF eyes, PointF cursor, Mood mood)
    {
        if (mood == Mood.Sleep) return default;
        float dx = cursor.X - eyes.X, dy = cursor.Y - eyes.Y;
        float distance = MathF.Sqrt(dx * dx + dy * dy);
        if (distance >= Radius) return default;
        float divisor = Math.Max(32, distance);
        // Ease out near the boundary; avoid a sudden flip as the cursor crosses the face.
        return new(dx / divisor, dy / divisor, Math.Clamp((Radius - distance) / 48, 0, 1));
    }

    public PetGaze Approach(PetGaze target, float dt)
    {
        float blend = 1 - MathF.Exp(-Math.Max(0, dt) / 0.09f);
        return new(X + (target.X - X) * blend, Y + (target.Y - Y) * blend,
            Attention + (target.Attention - Attention) * blend);
    }
}
