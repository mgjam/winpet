namespace WinPet.Domain;

// A held pet pauses its routine, but visual time continues. Explicit pause and
// unavailable desktop space freeze both clocks without accumulating catch-up time.
internal sealed class PetAnimationClock
{
    public double Seconds { get; private set; }
    public double RoutineSeconds { get; private set; }

    public void Advance(float dt, bool paused, bool visible, bool held)
    {
        if (paused || !visible) return;
        Seconds += Math.Max(0, dt);
        if (!held) RoutineSeconds += Math.Max(0, dt);
    }
}
