using System.Diagnostics;
using WinPet.Domain;
using WinPet.Rendering;

namespace WinPet.Desktop;

// Adapts Windows events and desktop snapshots to a live pet. Domain behavior lives in PetSession.
internal sealed class PetWindow : Form
{
    private const double WorldRefreshSeconds = .08;
    private readonly IPetVisual pet;
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 16 };
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly NotifyIcon tray;
    private readonly Icon appIcon;
    private readonly PetMenu petMenu;
    private double nextWorld;
    internal PetSession Session { get; }

    public PetWindow(IPetVisual pet)
    {
        this.pet = pet;
        using (var stream = typeof(PetWindow).Assembly.GetManifestResourceStream("WinPet.AppIcon")!)
        using (var icon = new Icon(stream, new Size(32, 32)))
            appIcon = (Icon)icon.Clone();
        Icon = appIcon;
        Text = $"WinPet — {pet.Name}";
        FormBorderStyle = FormBorderStyle.None;
        AutoScaleMode = AutoScaleMode.None;
        ClientSize = pet.Size;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        var area = Screen.PrimaryScreen!.WorkingArea;
        Session = new PetSession(pet, new(area.Right - pet.Size.Width - 32, area.Bottom - pet.Size.Height), Random.Shared);
        Location = Point.Round(Session.Position);
        petMenu = new PetMenu(pet, Session);
        ContextMenuStrip = petMenu;
        petMenu.Opening += (_, e) =>
        {
            RefreshWorld();
            if (petMenu.SourceControl == this && !Session.Available) e.Cancel = true;
            petMenu.RefreshItems();
        };
        petMenu.Closed += (_, _) => nextWorld = 0;
        petMenu.ActionRequested += action =>
        {
            RefreshWorld();
            if (Session.TryStart(action)) Place();
        };
        petMenu.SpaceRequested += () => nextWorld = 0;
        petMenu.QuitRequested += Close;
        tray = new NotifyIcon { Text = $"WinPet · {pet.Name}", Icon = appIcon, ContextMenuStrip = petMenu, Visible = true };
        timer.Tick += (_, _) => TickPet();
        Shown += (_, _) => { RefreshWorld(); timer.Start(); };
    }

    protected override bool ShowWithoutActivation => true;
    protected override CreateParams CreateParams
    {
        get
        {
            var parameters = base.CreateParams;
            parameters.ExStyle |= Native.NoActivateStyle | Native.ToolWindowStyle | Native.LayeredStyle;
            return parameters;
        }
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == Native.MouseActivateMessage) { message.Result = Native.NoActivateResult; return; }
        base.WndProc(ref message);
    }

    internal void StopAnimation() => timer.Stop();

    internal void RefreshWorld()
    {
        if (VirtualDesktops.IsCurrent(Handle) == false)
        {
            petMenu.Close();
            Session.LeaveWorkspace();
            Capture = false;
            Hide();
            // An unassigned tool window may need a fresh handle to follow an empty workspace.
            if (!VirtualDesktops.FollowCurrent(Handle)) RecreateHandle();
        }
        Session.Observe(DesktopWorld.Read(Handle, ignoredWindow: petMenu.IsHandleCreated ? petMenu.Handle : 0));
        if (Session.Available)
        {
            Place();
            if (!Visible) Show();
        }
        else
        {
            if (petMenu.SourceControl == this) petMenu.Close();
            Capture = false;
            Hide();
        }
    }

    private void TickPet()
    {
        double now = clock.Elapsed.TotalSeconds;
        if (now >= nextWorld) { RefreshWorld(); nextWorld = now + WorldRefreshSeconds; }
        Session.Advance(now, Cursor.Position, petMenu.Visible);
        if (Session.Available && !petMenu.Visible) Place();
    }

    private void Place()
    {
        using var frame = PetRenderer.Frame(pet, Session.Frame);
        var position = new Point((int)Math.Floor(Session.Position.X), (int)Math.Floor(Session.Position.Y));
        LayeredWindow.Draw(Handle, frame, position);
        if (!petMenu.Visible) Native.KeepTopmost(Handle);
    }

    protected override void OnPaint(PaintEventArgs e) { } // LayeredWindow supplies the whole frame.
    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && Session.Press(Cursor.Position, clock.Elapsed.TotalSeconds)) Capture = true;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left || !Session.Held) return;
        Session.Release(clock.Elapsed.TotalSeconds);
        Capture = false;
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);
        if (!Capture) Session.CancelHold();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            timer.Dispose();
            tray.Visible = false;
            petMenu.Dispose();
            tray.Dispose();
            appIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}
