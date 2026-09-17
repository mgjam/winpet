namespace WinPet;

internal enum Mood { Idle, Walk, Look, Sit, Sleep, React, Dragged, Falling, Read, Think, Sing, Violin, Laptop }

// Pets draw resolved channels. They may customize composition without changing
// desktop physics, or use the shared blink/attention/interaction priorities.
internal interface IPet
{
    string Name { get; }
    Size Size { get; }
    IReadOnlyList<PetRoutine> Routines => PetRoutine.Default;
    PointF GazeOrigin => new(Size.Width / 2f, Size.Height / 2f);
    PetPose ComposePose(Mood intent, double seconds, int facing, PetGaze gaze, RoutinePose? routine)
        => PetAnimation.Compose(intent, seconds, facing, gaze, routine);
    void Paint(Graphics graphics, PetPose pose);
}
