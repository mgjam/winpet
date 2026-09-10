namespace WinPet;

internal static class PetMotion
{
    private const float SideRestitution = 0.72f;
    private const float BumpSpeed = 400;

    public static PointF Bump(PointF velocity) => new(velocity.X, -BumpSpeed);

    public static PointF Move(World world, PointF position, SizeF size, ref float vx, ref float vy,
        float dt, bool airborne, out bool hitSide)
    {
        position = world.Move(position, size, vx * dt, vy * dt, out hitSide, out bool hitVertical);
        if (hitSide) vx = airborne ? -vx * SideRestitution : 0;
        // Tops remain landing surfaces; ceilings stop upward motion without adding energy.
        if (hitVertical) vy = 0;
        return position;
    }
}
