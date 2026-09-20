using System.Drawing.Drawing2D;
using WinPet.Domain;
using WinPet.Rendering;

namespace WinPet.Pets.Cactus;

internal sealed class CactusPet : IPetVisual
{
    public string Name => "Cacti";
    public Size Size => new(76, 92);
    public PointF AttentionOrigin => new(38, 43);
    public IReadOnlyList<PetAction> Actions { get; } = Array.AsReadOnly<PetAction>([
        CactusWatering.Action
    ]);
    private const double TransitionSeconds = RoutinePose.Entrance + RoutinePose.Exit;
    public IReadOnlyList<PetRoutine> Routines { get; } = Array.AsReadOnly<PetRoutine>([
        new(PetIntent.Move, 70, 5, 10), new(PetIntent.Idle, 53, 6, 12), new(PetIntent.Dormant, 33, 16, 28),
        new(PetIntent.Observe, 4, 4, 7), new(PetIntent.Rest, 3, 4, 8),
        // Give the settled activity its full duration, in addition to opening/putting away.
        new(PetIntent.Idle, 2, 19.5 + TransitionSeconds, 28.6 + TransitionSeconds, CactusActivities.Read),
        new(PetIntent.Idle, 2, 18.2 + TransitionSeconds, 23.4 + TransitionSeconds, CactusActivities.Think),
        new(PetIntent.Idle, 1, 11.7 + TransitionSeconds, 15.6 + TransitionSeconds, CactusActivities.Sing),
        new(PetIntent.Idle, 1, 18 + TransitionSeconds, 25 + TransitionSeconds, CactusActivities.Violin),
        new(PetIntent.Idle, 1, 20 + TransitionSeconds, 28 + TransitionSeconds, CactusActivities.Laptop)
    ]);

    public CactusPose ComposePose(PetIntent intent, double seconds, int facing, CursorAttention attention,
        RoutinePose? routine, PetActivity? activity = null)
        => CactusAnimation.Compose(intent, seconds, facing, attention, routine, activity as CactusActivity);

    public void Paint(Graphics g, PetFrame frame)
    {
        var pose = ComposePose(frame.Intent, frame.Seconds, frame.Facing, frame.Attention, frame.Routine, frame.Activity);
        double activitySeconds = pose.Activity.Seconds;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var outline = new Pen(CactusPalette.Edge, 2);
        using var green = new SolidBrush(CactusPalette.Green);
        using var light = new Pen(Color.FromArgb(164, 209, 139), 2);
        using var pot = new SolidBrush(Color.FromArgb(206, 127, 90));
        using var rim = new SolidBrush(Color.FromArgb(233, 163, 118));
        using var dark = new SolidBrush(Color.FromArgb(44, 65, 51));
        using var pink = new SolidBrush(Color.FromArgb(239, 145, 160));
        using var feet = new SolidBrush(Color.FromArgb(91, 75, 57));
        float stride = pose.Stride;
        g.FillEllipse(feet, 27, 84 + Math.Max(0, stride), 10, 5);
        g.FillEllipse(feet, 42, 84 + Math.Max(0, -stride), 10, 5);
        using var body = new GraphicsPath(FillMode.Winding);
        body.AddBezier(25, 24, 25, 6, 52, 6, 52, 24);
        body.AddLine(52, 24, 52, 42); body.AddLine(52, 42, 58, 42);
        body.AddLine(58, 42, 58, 28); body.AddBezier(58, 28, 58, 21, 67, 21, 67, 28);
        body.AddLine(67, 28, 67, 46); body.AddBezier(67, 46, 67, 51, 63, 51, 60, 51);
        body.AddLine(60, 51, 52, 51); body.AddLine(52, 51, 52, 65);
        body.AddLine(52, 65, 25, 65); body.AddLine(25, 65, 25, 58);
        body.AddLine(25, 58, 16, 58); body.AddBezier(16, 58, 11, 58, 11, 54, 11, 50);
        body.AddLine(11, 50, 11, 36); body.AddBezier(11, 36, 11, 29, 21, 29, 21, 36);
        body.AddLine(21, 36, 21, 48); body.AddLine(21, 48, 25, 48); body.CloseFigure();
        g.FillPath(green, body);
        g.DrawPath(outline, body);
        g.DrawLine(light, 31, 21, 31, 38);
        g.DrawLine(light, 46, 20, 46, 37);
        g.DrawLine(light, 32, 54, 32, 62);
        g.DrawLine(light, 45, 54, 45, 62);
        g.FillEllipse(pink, 40, 5, 7, 7);
        g.FillEllipse(pink, 46, 5, 7, 7);
        g.FillEllipse(pink, 43, 9, 7, 7);
        g.FillEllipse(Brushes.Wheat, 44, 8, 4, 4);
        CactusFace.Paint(g, pose.Face, activitySeconds, outline, dark, pink);
        PointF[] potShape = [new(24, 69), new(53, 69), new(48, 86), new(29, 86)];
        g.FillPolygon(pot, potShape); g.DrawPolygon(outline, potShape);
        g.FillRectangle(rim, 21, 63, 35, 7); g.DrawRectangle(outline, 21, 63, 35, 7);
        using var detail = new Pen(Color.FromArgb(243, 181, 139), 2);
        g.DrawLine(detail, 29, 74, 31, 82);
        if (pose.Reacting) g.DrawArc(detail, 35, 73, 10, 8, 0, 180);
        if (pose.Activity.Definition is { } activity)
            PetActivityRenderer.Paint(g, Size, activity.Paint, pose.Activity.Seconds, pose.Activity.Amount);
    }
}
