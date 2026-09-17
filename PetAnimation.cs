namespace WinPet;

internal enum PetActivity { None, Read, Think, Sing, Violin, Laptop }
internal enum MouthShape { Smile, Surprised, Whistle }
internal readonly record struct EyePose(float X, float Y, float Openness, float Height);
internal readonly record struct FacePose(EyePose Eyes, MouthShape Mouth, float MouthAmount, bool Blush);
internal readonly record struct ActivityPose(PetActivity Kind, double Seconds, float Amount);
internal readonly record struct PetPose(float Stride, FacePose Face, ActivityPose Activity, bool Reacting);

// Resolve intent once; artwork only draws the resulting independent channels.
// Priority: sleep closes the lids and ignores attention; otherwise blink overlays
// every gaze. Nearby cursor attention blends over the activity's default gaze.
internal static class PetAnimation
{
    private readonly record struct ActivityStyle(PointF Gaze, float CursorWeight, MouthShape Mouth);
    private static readonly IReadOnlyDictionary<PetActivity, ActivityStyle> Styles =
        new Dictionary<PetActivity, ActivityStyle>
        {
            [PetActivity.None] = new(default, 1, MouthShape.Smile),
            [PetActivity.Read] = new(new(0, 1), .85f, MouthShape.Smile),
            [PetActivity.Think] = new(new(.6f, -.75f), 1, MouthShape.Smile),
            [PetActivity.Sing] = new(default, 1, MouthShape.Whistle),
            [PetActivity.Violin] = new(new(.25f, .35f), 1, MouthShape.Smile),
            [PetActivity.Laptop] = new(new(0, .8f), .85f, MouthShape.Smile)
        };

    public static PetPose Compose(Mood intent, double seconds, int facing, PetGaze cursor = default, RoutinePose? routine = null)
    {
        var kind = intent switch
        {
            Mood.Read => PetActivity.Read, Mood.Think => PetActivity.Think,
            Mood.Sing => PetActivity.Sing, Mood.Violin => PetActivity.Violin, Mood.Laptop => PetActivity.Laptop, _ => PetActivity.None
        };
        var activity = new ActivityPose(kind, routine?.Seconds ?? seconds,
            kind == PetActivity.None ? 0 : routine?.Amount ?? 1);
        var style = Styles[kind];
        float baseX = intent == Mood.Look ? (float)Math.Sin(seconds * 1.4) * .8f : facing * .28f;
        var activityGaze = style.Gaze;
        if (kind == PetActivity.Read) activityGaze.X = (float)Math.Sin(activity.Seconds * 1.8) * .48f;
        float x = Lerp(baseX, activityGaze.X, activity.Amount);
        float y = activityGaze.Y * activity.Amount;
        bool sleeping = intent == Mood.Sleep;
        float attention = sleeping ? 0 : Math.Clamp(cursor.Attention, 0, 1) * Lerp(1, style.CursorWeight, activity.Amount);
        x = Lerp(x, cursor.X, attention);
        y = Lerp(y, cursor.Y, attention);
        var eyes = new EyePose(x * 2.5f, y * 2, sleeping ? 0 : BlinkOpenness(seconds), intent == Mood.Dragged ? 6 : 4);
        var mouth = intent is Mood.Falling or Mood.Dragged ? MouthShape.Surprised : style.Mouth;
        var face = new FacePose(eyes, mouth, activity.Amount, intent is Mood.React or Mood.Dragged);
        return new(intent == Mood.Walk ? (float)Math.Sin(seconds * 12) * 2 : 0,
            face, activity, intent == Mood.React);
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
