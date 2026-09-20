namespace WinPet.Domain;

// Local activity time drives both the prop and facial animation. Ends return to idle.
internal readonly record struct RoutinePose(double Seconds, double Duration)
{
    public const double Entrance = 1.0;
    public const double Exit = 1.2;
    public float Amount
    {
        get
        {
            double t = Math.Clamp(Math.Min(Seconds / Entrance, (Duration - Seconds) / Exit), 0, 1);
            return (float)(t * t * (3 - 2 * t));
        }
    }
}
