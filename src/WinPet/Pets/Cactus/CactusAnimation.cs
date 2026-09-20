using WinPet.Domain;

namespace WinPet.Pets.Cactus;

// Resolve intent once; artwork only draws the resulting independent channels.
// Priority: sleep closes the lids and ignores attention; otherwise blink overlays
// every gaze. Nearby cursor attention blends over the activity's default gaze.
internal static class CactusAnimation
{
    public static CactusPose Compose(PetIntent intent, double seconds, int facing, CursorAttention cursor = default,
        RoutinePose? routine = null, CactusActivity? definition = null)
    {
        if (intent is PetIntent.Falling or PetIntent.Dragged or PetIntent.React or PetIntent.Dormant) definition = null;
        var activity = new ActivityPose(definition, routine?.Seconds ?? seconds,
            definition is null ? 0 : routine?.Amount ?? 1);
        float baseX = intent == PetIntent.Observe ? (float)Math.Sin(seconds * 1.4) * .8f : facing * .28f;
        var activityGaze = definition?.GazeAt(activity.Seconds) ?? default;
        float x = Lerp(baseX, activityGaze.X, activity.Amount);
        float y = activityGaze.Y * activity.Amount;
        bool sleeping = intent == PetIntent.Dormant;
        float attention = sleeping ? 0 : Math.Clamp(cursor.Attention, 0, 1) * Lerp(1, definition?.CursorWeight ?? 1, activity.Amount);
        x = Lerp(x, cursor.X, attention);
        y = Lerp(y, cursor.Y, attention);
        var eyes = new EyePose(x * 2.5f, y * 2, sleeping ? 0 : BlinkOpenness(seconds), intent == PetIntent.Dragged ? 6 : 4);
        var mouth = intent is PetIntent.Falling or PetIntent.Dragged ? MouthShape.Surprised : definition?.Mouth ?? MouthShape.Smile;
        var face = new FacePose(eyes, mouth, activity.Amount, intent is PetIntent.React or PetIntent.Dragged);
        return new(intent == PetIntent.Move ? (float)Math.Sin(seconds * 12) * 2 : 0,
            face, activity, intent == PetIntent.React);
    }

    // A continuous, independent clock: changing activities never restarts a blink.
    // Close, hold briefly, reopen. All awake poses, including sitting/dragging, use it.
    internal static float BlinkOpenness(double seconds)
    {
        double phase = (seconds + 2) % 4.7;
        if (phase < .08) return 1 - Smooth((float)(phase / .08));
        if (phase < .13) return 0;
        if (phase < .25) return Smooth((float)((phase - .13) / .12));
        return 1;
    }

    private static float Lerp(float from, float to, float amount) => from + (to - from) * amount;
    private static float Smooth(float t) => t * t * (3 - 2 * t);
}
