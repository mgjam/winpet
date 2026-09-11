# Windows Desktop Pet

A quiet Windows companion for moments between work, especially outside VS Code. It wanders through exposed desktop space, treats application windows as solid obstacles and platforms, and responds to clicks and drag-and-drop. No progression, notifications, or chores.

- [Product intent, experience, and v1 scope](PRODUCT.md)
- [Proposed implementation and prototype checks](ARCHITECTURE.md)
- [Visual reference: states, activities, and extras](previews/README.md) · [Animated gallery](previews/index.html)

## Status

A runnable first prototype with Cacti, a little potted cactus: walking, looking around, resting, sleeping, reading, thinking, silently singing, playing violin, click reactions, and drag/drop with a modest flick. Other pets can later implement the small `IPet` artwork interface without changing desktop physics. Only Cacti is included; there is no selector yet.

Cacti occasionally settles into a book with curved pages and a bookmark, ponders with a small thought bubble, or whistles with puckered lips and drifting musical notes (no audio). Settled reading lasts 19.5–28.6 seconds, thinking 18.2–23.4 seconds, and whistling 11.7–15.6 seconds. Activities additionally ease in over one second and out over 1.2 seconds, followed by 2.5 seconds in idle. Clicks, dragging, and falling interrupt immediately. Pause freezes animation and routine timing. A seeded long-run simulation spends about 85% of autonomous time walking, idling, or sleeping, 11% in special activities, and the rest looking around or sitting. Future pets can override `IPet.Routines` with their own weighted activity choices and duration ranges, and draw the resolved `PetPose` channels in `Paint`; `ComposePose` can customize their animation style; the default profile retains the basic walking and resting behaviors.

## Build and run

Use Windows and the .NET 10 SDK. From this folder:

```powershell
dotnet restore
dotnet build
dotnet run
```

Or open `bin\Debug\net10.0-windows\WinPet.exe` after building. A second launch does not create a duplicate pet. Nothing is installed or added to startup.

## Visual review workflow

Every normal build regenerates **`previews/` in the source tree**: a transparent PNG and looping GIF for each catalog entry, an overview sheet, and a self-contained HTML gallery. Open `previews/index.html` in a browser to switch between stills and animations. Keep these generated artifacts in source control alongside the code that changes their appearance.

Run `tools\generate-previews.cmd` to build and regenerate, or simply run `dotnet build`. Quit the running pet first if its executable is locked. For a quick code-only build, explicitly opt out with `dotnet build -p:SkipPetPreviews=true`. To render without rebuilding, use `dotnet bin\Debug\net10.0-windows\WinPet.dll --generate-previews previews`.

The catalog uses these terms:

| Category | Contents |
| --- | --- |
| Physical states | On ground, in air, being held |
| Activities | Standing, walking, sleeping, reading, thinking, singing/whistling, playing violin, being poked, looking around, sitting |
| Shared extras | Blinking and cursor tracking |
| Combinations | Thinking, reading, whistling, and violin with cursor attention and blinking |

The current scheduler calls its action enum `Mood`; the gallery calls these **activities**. Physical interaction takes priority over the chosen activity. Eyes and cursor attention remain independent channels. The gallery shows poses in place, not a collision simulation. Its eight-second transition clips shorten the settled activity duration so entrances and exits are easy to review; the actual pet keeps the durations described above.

`PreviewGenerator.cs` owns the scene catalog, sample times, cursor paths, and gallery layout. It calls the same `PetRenderer` → `IPet.ComposePose` → `IPet.Paint` path as the live pet. `GifWriter.cs` handles GIF palettes and changed-frame regions using built-in .NET/Windows drawing APIs. `PreviewChecks.cs` validates dimensions, frame counts, timing, looping, and decoding during generation; a failure fails the build. There are no external image tools or Python/PowerShell dependencies. Self-tests assert behavior; the build owns all visual artifacts.

Click the cactus for a reaction. Hold the left mouse button and drag to pick it up; release to drop or gently throw it. Airborne throws bounce softly off the left/right sides of windows and screen edges. Click Cacti in midair to bump him upward while keeping his sideways momentum; timed taps can keep him aloft. Holding and dragging still catches him, and grounded clicks keep the blush reaction. Windows block dragging as well as autonomous movement. Right-click the WinPet system-tray icon (possibly under the hidden-icons arrow) to pause/resume or quit.

Cacti follows the active Windows workspace and uses only its visible application windows as obstacles. Fully invisible overlays (such as NVIDIA's idle overlay) are ignored. Cacti hides only if application windows cover all usable desktop space. Restore or resize a window to expose space, and he returns automatically. He can stand above a window only if his whole body fits there.

## Source layout

- `Pet.cs`: shared pet contract and action names.
- `CactusPet.cs`: Cacti's routine choices and base body artwork.
- `CactusFace.cs`: draws the resolved eyes, mouth, and blush.
- `CactusReading.cs`, `CactusThinking.cs`, `CactusSinging.cs`, `CactusViolin.cs`: each activity's artwork and local movement.
- `CactusActivityRenderer.cs`: dispatches to the activity artist and applies the shared transparent layer/fade.
- `PetAnimation.cs`: resolves independent animation channels and attention priorities; `PetWindow.cs` handles desktop interaction and movement.
- `PreviewGenerator.cs`: generates the shared visual gallery from that same rendering path.

## Cursor attention

Cacti notices the cursor within about 240 physical pixels of his face and gently looks toward it in any direction. He returns to his activity gaze when it moves away. Thinking and whistling follow the cursor fully; reading glances up while retaining a slight bookward bias. Eyelids blink independently during every awake action, including sitting and being held; sleeping keeps them closed. Cursor awareness is shared behavior that future pet artwork can also use.

## Checks and prototype limits

`dotnet run -- --self-test` runs collision, window-filtering, and animation-combination checks and writes `self-test-results.txt` beside the executable. Visual references are generated by the build into `previews/`. `dotnet run -- --diagnose` writes `desktop-check.txt` there with monitor counts, space availability, window classes/process names, bounds, transparency, and workspace membership; it does not collect window titles. Run desktop diagnostics normally on your Windows desktop: an isolated execution environment may not see your real windows.

`dotnet run -- --workspace-check` checks a temporary pet window's workspace assignment. When Windows leaves the tool window unassigned to an individual workspace, it reports that fact; it cannot substitute for checking real workspace switches.

The prototype uses a Windows Forms layered window instead of WPF. The artwork supplies per-pixel transparency for smooth edges; fully transparent pixels pass mouse input through to the desktop. The pet has a fixed 76 × 92 physical-pixel footprint, including on mixed-DPI displays; automatic travel across monitor seams is deferred. Window geometry refreshes every 80 ms, so a rapidly moving window can briefly overlap the pet before correction. Transparent/shaped app windows are treated conservatively as solid rectangles. Auto-hidden taskbars follow Windows' reported work area. Real multi-monitor, fullscreen, and cross-app interaction still need hands-on validation.



