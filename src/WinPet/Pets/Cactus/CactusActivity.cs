using WinPet.Domain;

namespace WinPet.Pets.Cactus;

internal sealed class CactusActivity : PetActivity
{
    private readonly Action<Graphics, double, float> paint;
    private readonly Func<double, PointF>? gaze;
    public float CursorWeight { get; }
    public MouthShape Mouth { get; }

    public CactusActivity(string id, Action<Graphics, double, float> paint, Func<double, PointF>? gaze = null,
        float cursorWeight = 1, MouthShape mouth = MouthShape.Smile) : base(id)
    {
        ArgumentNullException.ThrowIfNull(paint);
        if (!float.IsFinite(cursorWeight) || cursorWeight is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(cursorWeight));
        (this.paint, this.gaze, CursorWeight, Mouth) = (paint, gaze, cursorWeight, mouth);
    }

    public PointF GazeAt(double seconds) => gaze?.Invoke(seconds) ?? default;
    public void Paint(Graphics graphics, double seconds, float amount) => paint(graphics, seconds, amount);
}
