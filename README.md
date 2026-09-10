# Windows Desktop Pet

A quiet Windows companion for moments between work, especially outside VS Code. It wanders through exposed desktop space, treats application windows as solid obstacles and platforms, and responds to clicks and drag-and-drop. No progression, notifications, or chores.

- [Product intent, experience, and v1 scope](PRODUCT.md)
- [Proposed implementation and prototype checks](ARCHITECTURE.md)

## Status

A runnable first prototype with Cacti, a little potted cactus: walking, looking around, resting, sleeping, click reactions, and drag/drop with a modest flick. Other pets can later implement the small `IPet` artwork interface without changing desktop physics. Only Cacti is included; there is no selector yet.

## Build and run

Use Windows and the .NET 10 SDK. From this folder:

```powershell
dotnet restore
dotnet build
dotnet run
```

Or open `bin\Debug\net10.0-windows\WinPet.exe` after building. A second launch does not create a duplicate pet. Nothing is installed or added to startup.

Click the cactus for a reaction. Hold the left mouse button and drag to pick it up; release to drop or gently throw it. Windows block dragging as well as autonomous movement. Right-click the WinPet system-tray icon (possibly under the hidden-icons arrow) to pause/resume or quit.

Cacti follows the active Windows workspace and uses only its visible application windows as obstacles. Fully invisible overlays (such as NVIDIA's idle overlay) are ignored. Cacti hides only if application windows cover all usable desktop space. Restore or resize a window to expose space, and he returns automatically. He can stand above a window only if his whole body fits there.

## Checks and prototype limits

`dotnet run -- --self-test` runs collision and window-filtering checks and writes `self-test-results.txt` and an artwork preview beside the executable. `dotnet run -- --diagnose` writes `desktop-check.txt` there with monitor counts, space availability, window classes/process names, bounds, transparency, and workspace membership; it does not collect window titles. Run desktop diagnostics normally on your Windows desktop: an isolated execution environment may not see your real windows.

`dotnet run -- --workspace-check` checks a temporary pet window's workspace assignment. When Windows leaves the tool window unassigned to an individual workspace, it reports that fact; it cannot substitute for checking real workspace switches.

The prototype uses a native shaped Windows Forms window instead of WPF. Empty space outside its silhouette has no window to intercept input. The pet has a fixed 76 × 92 physical-pixel footprint, including on mixed-DPI displays; automatic travel across monitor seams is deferred. Window geometry refreshes every 80 ms, so a rapidly moving window can briefly overlap the pet before correction. Transparent/shaped app windows are treated conservatively as solid rectangles. Auto-hidden taskbars follow Windows' reported work area. Real multi-monitor, fullscreen, and cross-app interaction still need hands-on validation.
