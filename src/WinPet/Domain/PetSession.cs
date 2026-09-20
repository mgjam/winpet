namespace WinPet.Domain;

// A live pet owns its motion, interaction, attention, and behavior. The desktop
// supplies world snapshots, cursor positions and elapsed time; no window is needed.
internal sealed class PetSession
{
    private const float MaxStepSeconds = .035f;
    private const float Gravity = 1100;
    private const float MaxFallSpeed = 900;
    private const float MoveSpeed = 25;
    private const float MaxThrowSpeed = 650;
    private const float DragThreshold = 5;
    private const double ThrowReleaseWindow = .1;
    private const double ReactionDuration = 1.8;
    private readonly IPet pet;
    private readonly Random random;
    private readonly PetBehavior behavior;
    private readonly PetAnimationClock animation = new();
    private World world = new([], []);
    private float vx, vy;
    private double lastTick, lastDragTime;
    private PointF pressCursor, lastCursor, grabOffset, velocityBeforePress;
    private CursorAttention attention;

    public PetSession(IPet pet, PointF position, Random random)
    {
        this.pet = pet;
        this.random = random;
        behavior = new PetBehavior(pet);
        Position = position;
    }

    public PointF Position { get; private set; }
    public PointF Velocity => new(vx, vy);
    public int Facing { get; private set; } = 1;
    public bool Available { get; private set; }
    public bool Paused { get; set; }
    public bool Held { get; private set; }
    public bool Dragging { get; private set; }
    public PetIntent Intent => behavior.Intent;
    public PetAction? ActiveAction => behavior.ActiveAction;
    public double AnimationSeconds => animation.Seconds;
    public double RoutineSeconds => animation.RoutineSeconds;
    public PetActionContext ActionContext => new(Paused, Available, Held, world.Supported(Position, pet.Size) && vy >= 0);
    public PetFrame Frame => new(Intent, animation.Seconds, Facing, attention,
        behavior.PoseAt(animation.RoutineSeconds), behavior.Activity);

    public void Observe(World snapshot)
    {
        world = snapshot;
        var safe = world.FindSpace(Position, pet.Size);
        Available = safe.HasValue;
        if (safe is { } point)
        {
            if (point != Position) { Position = point; vx = vy = 0; }
        }
        else CancelHold();
    }

    public void LeaveWorkspace()
    {
        CancelHold();
        vx = vy = 0;
    }

    public bool CanStart(PetAction action) => behavior.CanStart(action, ActionContext);

    public bool TryStart(PetAction action)
    {
        if (!behavior.TryStart(action, ActionContext, animation.RoutineSeconds)) return false;
        vx = vy = 0;
        return true;
    }

    public void Advance(double now, PointF cursor, bool menuOpen = false)
    {
        float dt = (float)Math.Clamp(now - lastTick, 0, MaxStepSeconds);
        lastTick = now;
        if (menuOpen) return;
        animation.Advance(dt, Paused, Available, Held);
        if (!Available) return;
        if (Held) MoveHeld(cursor, now);
        else if (!Paused) MoveFreely(dt);
        if (!Paused)
        {
            var origin = new PointF(Position.X + pet.AttentionOrigin.X, Position.Y + pet.AttentionOrigin.Y);
            attention = attention.Approach(CursorAttention.Toward(origin, cursor), dt);
        }
    }

    public bool Press(PointF cursor, double now)
    {
        if (!Available || Held) return false;
        Held = true;
        Dragging = false;
        pressCursor = lastCursor = cursor;
        grabOffset = new(cursor.X - Position.X, cursor.Y - Position.Y);
        lastDragTime = now;
        velocityBeforePress = Velocity;
        vx = vy = 0;
        return true;
    }

    public void Release(double now)
    {
        if (!Held) return;
        Held = false;
        if (Dragging)
        {
            behavior.Interrupt(PetIntent.Falling, animation.RoutineSeconds);
            if (now - lastDragTime > ThrowReleaseWindow) vx = vy = 0;
        }
        else if (!Paused && (!world.Supported(Position, pet.Size) || velocityBeforePress.Y < 0))
        {
            var bumped = PetMotion.Bump(velocityBeforePress);
            vx = bumped.X;
            vy = bumped.Y;
            behavior.Interrupt(PetIntent.Falling, animation.RoutineSeconds);
        }
        else
        {
            behavior.Interrupt(PetIntent.React, animation.RoutineSeconds, ReactionDuration);
            vx = vy = 0;
        }
        Dragging = false;
    }

    public void CancelHold()
    {
        if (!Held) return;
        Held = Dragging = false;
        vx = vy = 0;
        behavior.Interrupt(PetIntent.Falling, animation.RoutineSeconds);
    }

    private void MoveHeld(PointF cursor, double now)
    {
        if (!Dragging && Math.Abs(cursor.X - pressCursor.X) + Math.Abs(cursor.Y - pressCursor.Y) > DragThreshold)
            Dragging = true;
        if (!Dragging) return;
        behavior.Interrupt(PetIntent.Dragged, animation.RoutineSeconds);
        var wanted = new PointF(cursor.X - grabOffset.X, cursor.Y - grabOffset.Y);
        Position = world.Move(Position, pet.Size, wanted.X - Position.X, wanted.Y - Position.Y, out _, out _);
        double elapsed = Math.Max(.008, now - lastDragTime);
        vx = Math.Clamp((float)((cursor.X - lastCursor.X) / elapsed), -MaxThrowSpeed, MaxThrowSpeed);
        vy = Math.Clamp((float)((cursor.Y - lastCursor.Y) / elapsed), -MaxThrowSpeed, MaxThrowSpeed);
        lastCursor = cursor;
        lastDragTime = now;
    }

    private void MoveFreely(float dt)
    {
        bool airborne = !world.Supported(Position, pet.Size) || vy < 0;
        if (airborne)
        {
            behavior.Interrupt(PetIntent.Falling, animation.RoutineSeconds);
            vy = Math.Min(MaxFallSpeed, vy + Gravity * dt);
        }
        else
        {
            vy = 0;
            if (behavior.UpdateGrounded(animation.RoutineSeconds, random)) Facing = random.Next(2) == 0 ? -1 : 1;
            vx = Intent == PetIntent.Move ? Facing * MoveSpeed : 0;
        }
        Position = PetMotion.Move(world, Position, pet.Size, ref vx, ref vy, dt, airborne, out bool wall);
        if (wall) Facing = vx != 0 ? Math.Sign(vx) : -Facing;
    }
}
