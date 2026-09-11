using System.Diagnostics;

namespace WinPet;

internal sealed class PetWindow : Form
{
    private readonly IPet pet;
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 16 };
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly NotifyIcon tray;
    private readonly Icon appIcon;
    private World world = new([], []);
    private PointF position;
    private float vx, vy;
    private int facing = 1;
    private Mood mood = Mood.Idle;
    private double lastTick, nextWorld, nextBehavior = 2;
    private readonly PetAnimationClock animation = new();
    private double routineStarted, routineDuration;
    private bool paused, available, pressed, dragging;
    private Point pressCursor, lastCursor;
    private PointF grabOffset;
    private double lastDragTime;
    private PointF velocityBeforePress;
    private PetGaze gaze;

    public PetWindow(IPet pet)
    {
        this.pet = pet;
        using (var iconStream = typeof(PetWindow).Assembly.GetManifestResourceStream("WinPet.AppIcon")!)
        using (var loadedIcon = new Icon(iconStream, new Size(32, 32)))
            appIcon = (Icon)loadedIcon.Clone();
        Icon = appIcon;
        Text = $"WinPet — {pet.Name}";
        FormBorderStyle = FormBorderStyle.None;
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = pet.Size;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        var area = Screen.PrimaryScreen!.WorkingArea;
        position = new PointF(area.Right - pet.Size.Width - 32, area.Bottom - pet.Size.Height);
        Location = Point.Round(position);
        var menu = new ContextMenuStrip();
        menu.Items.Add(pet.Name, null, (_, _) => { }).Enabled = false;
        var pause = new ToolStripMenuItem("Pause") { CheckOnClick = true };
        pause.CheckedChanged += (_, _) => { paused = pause.Checked; pause.Text = paused ? "Resume" : "Pause"; };
        menu.Items.Add(pause);
        menu.Items.Add("Find desktop space", null, (_, _) => { available = false; nextWorld = 0; });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, _) => Close());
        tray = new NotifyIcon { Text = $"WinPet · {pet.Name}", Icon = appIcon, ContextMenuStrip = menu, Visible = true };
        timer.Tick += (_, _) => TickPet();
        Shown += (_, _) => { RefreshWorld(); timer.Start(); };
    }

    protected override bool ShowWithoutActivation => true;
    protected override CreateParams CreateParams
    {
        get { var p = base.CreateParams; p.ExStyle |= 0x08000000 | 0x00000080 | 0x00080000; return p; }
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == 0x21) { message.Result = 3; return; } // MA_NOACTIVATE
        base.WndProc(ref message);
    }

    private void RefreshWorld()
    {
        if (VirtualDesktops.IsCurrent(Handle) == false)
        {
            // Hide before moving so the old coordinates cannot flash over a new app.
            pressed = dragging = false;
            Capture = false;
            Hide();
            if (!VirtualDesktops.FollowCurrent(Handle))
            {
                // An empty workspace may have no app to supply a desktop ID.
                // A fresh unowned HWND is created on the active workspace.
                RecreateHandle();
            }
            vx = vy = 0;
        }
        world = Native.ReadWorld(Handle);
        var safe = world.FindSpace(position, pet.Size);
        available = safe.HasValue;
        if (safe is { } point)
        {
            if (point != position) { position = point; vx = vy = 0; }
            // Place before showing, so a recovered pet never flashes at its old location.
            Place();
            if (!Visible) Show();
        }
        else
        {
            pressed = dragging = false;
            Capture = false;
            Hide();
        }
    }

    internal async Task<string> CheckWorkspaceFollowing()
    {
        timer.Stop();
        RefreshWorld();
        await Task.Delay(100);
        Guid origin = VirtualDesktops.DesktopOf(Handle);
        Guid other = Guid.Empty;
        Native.EnumWindows((window, _) =>
        {
            Guid desktop = VirtualDesktops.DesktopOf(window);
            if (VirtualDesktops.IsCurrent(window) == false && desktop != Guid.Empty && desktop != origin) other = desktop;
            return other == Guid.Empty;
        }, 0);
        var lines = new List<string> { $"Window has workspace ID: {origin != Guid.Empty}", $"Initial current workspace: {VirtualDesktops.IsCurrent(Handle)}", $"Initial visible: {Visible}", $"Initial valid space: {available}" };
        if (available)
        {
            var corner = new Point((int)Math.Floor(position.X), (int)Math.Floor(position.Y));
            lines.Add($"Transparent corner passes native hit testing: {Native.WindowFromPoint(corner) != Handle}");
            lines.Add($"Cacti body receives native hit testing: {Native.WindowFromPoint(new Point(corner.X + 38, corner.Y + 30)) == Handle}");
        }
        if (origin == Guid.Empty)
        {
            lines.Add("Tool window is not assigned to an individual workspace; explicit move test is inapplicable.");
            lines.Add("Switching workspaces should be checked interactively.");
            return string.Join("\n", lines);
        }
        if (other != Guid.Empty)
        {
            lines.Add($"Moved own test window to inactive workspace: {VirtualDesktops.Move(Handle, other)}");
            await Task.Delay(300);
            lines.Add($"Window desktop matches inactive target: {VirtualDesktops.DesktopOf(Handle) == other}");
            lines.Add($"Current before recovery: {VirtualDesktops.IsCurrent(Handle)}");
            RefreshWorld();
            lines.Add($"Current after recovery: {VirtualDesktops.IsCurrent(Handle)}");
            lines.Add($"Visible after recovery: {Visible}");
            lines.Add($"Valid position after recovery: {world.Fits(position, pet.Size)}");
        }
        else lines.Add("No inactive workspace window available for recovery check.");
        return string.Join("\n", lines);
    }

    private void TickPet()
    {
        double now = clock.Elapsed.TotalSeconds;
        float dt = (float)Math.Clamp(now - lastTick, 0, 0.035);
        lastTick = now;
        animation.Advance(dt, paused, available, pressed);
        if (now >= nextWorld) { RefreshWorld(); nextWorld = now + 0.08; }
        if (!available) return;
        if (pressed)
        {
            var cursor = Cursor.Position;
            if (!dragging && Math.Abs(cursor.X - pressCursor.X) + Math.Abs(cursor.Y - pressCursor.Y) > 5) dragging = true;
            if (dragging)
            {
                mood = Mood.Dragged;
                var wanted = new PointF(cursor.X - grabOffset.X, cursor.Y - grabOffset.Y);
                position = world.Move(position, pet.Size, wanted.X - position.X, wanted.Y - position.Y, out _, out _);
                double elapsed = Math.Max(0.008, now - lastDragTime);
                vx = Math.Clamp((float)((cursor.X - lastCursor.X) / elapsed), -650, 650);
                vy = Math.Clamp((float)((cursor.Y - lastCursor.Y) / elapsed), -650, 650);
                lastCursor = cursor; lastDragTime = now;
            }
        }
        else if (!paused)
        {
            bool grounded = world.Supported(position, pet.Size);
            bool airborne = !grounded || vy < 0;
            if (airborne)
            {
                mood = Mood.Falling;
                vy = Math.Min(900, vy + 1100 * dt);
            }
            else
            {
                vy = 0;
                if (mood is Mood.Falling or Mood.Dragged) { mood = Mood.Sit; nextBehavior = animation.RoutineSeconds + 1.4; }
                if (animation.RoutineSeconds >= nextBehavior)
                {
                    // Put the prop away, then breathe in idle before choosing another activity.
                    var routine = RoutinePose.IsActivity(mood)
                        ? (Mood: Mood.Idle, Duration: 2.5)
                        : PetRoutine.Choose(pet.Routines, mood, Random.Shared);
                    mood = routine.Mood;
                    routineStarted = animation.RoutineSeconds;
                    routineDuration = routine.Duration;
                    facing = Random.Shared.Next(2) == 0 ? -1 : 1;
                    nextBehavior = animation.RoutineSeconds + routine.Duration;
                }
                vx = mood == Mood.Walk ? facing * 25 : 0;
            }
            position = PetMotion.Move(world, position, pet.Size, ref vx, ref vy, dt, airborne, out bool wall);
            if (wall) facing = vx != 0 ? Math.Sign(vx) : -facing;
        }
        if (!paused)
        {
            var eyes = new PointF(position.X + pet.GazeOrigin.X, position.Y + pet.GazeOrigin.Y);
            gaze = gaze.Approach(PetGaze.Toward(eyes, Cursor.Position, mood), dt);
        }
        Place();
    }

    private void Place()
    {
        PetRenderer.Draw(Handle, pet, mood, animation.Seconds, facing,
            new Point((int)Math.Floor(position.X), (int)Math.Floor(position.Y)), gaze,
            RoutinePose.IsActivity(mood) ? new RoutinePose(animation.RoutineSeconds - routineStarted, routineDuration) : null);
        Native.SetWindowPos(Handle, -1, 0, 0, 0, 0, 0x13); // Keep topmost without activating or changing bounds.
    }

    protected override void OnPaint(PaintEventArgs e) { } // UpdateLayeredWindow supplies the whole frame.
    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        pressed = true; dragging = false;
        pressCursor = lastCursor = Cursor.Position;
        grabOffset = new PointF(pressCursor.X - position.X, pressCursor.Y - position.Y);
        lastDragTime = clock.Elapsed.TotalSeconds;
        velocityBeforePress = new PointF(vx, vy);
        vx = vy = 0;
        Capture = true;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left || !pressed) return;
        pressed = false;
        if (dragging) { mood = Mood.Falling; if (clock.Elapsed.TotalSeconds - lastDragTime > .1) vx = vy = 0; }
        else if (!paused && (!world.Supported(position, pet.Size) || velocityBeforePress.Y < 0))
        {
            // A tap keeps sideways momentum; a drag still catches and throws normally.
            var bumped = PetMotion.Bump(velocityBeforePress);
            vx = bumped.X; vy = bumped.Y;
            mood = Mood.Falling;
        }
        else { mood = Mood.React; nextBehavior = animation.RoutineSeconds + 1.8; vx = vy = 0; }
        dragging = false;
        Capture = false;
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);
        if (!Capture && pressed) { pressed = dragging = false; vx = vy = 0; mood = Mood.Falling; }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { timer.Dispose(); tray.Visible = false; tray.ContextMenuStrip?.Dispose(); tray.Dispose(); appIcon.Dispose(); Region?.Dispose(); }
        base.Dispose(disposing);
    }
}
