# Windows Desktop Pet

A quiet Windows companion for moments between work, especially outside VS Code. It wanders through exposed desktop space, treats application windows as solid obstacles and platforms, and responds to clicks and drag-and-drop. No progression, notifications, or chores.

- [Product intent, experience, and v1 scope](PRODUCT.md)
- [Architecture and extension points](ARCHITECTURE.md)
- [Development principles](CONTRIBUTING.md)
- [Visual reference: states, activities, and extras](previews/README.md) · [Animated gallery](previews/index.html)

## Status

A runnable first prototype with Cacti, a little potted cactus: walking, looking around, resting, sleeping, reading, thinking, silently singing, playing violin, writing on a laptop, click reactions, and drag/drop with a modest flick. Other pets can later implement the `IPetVisual` rendering interface without changing desktop physics. Only Cacti is included; there is no selector yet.

Cacti occasionally settles into a book with curved pages and a bookmark, ponders with a small thought bubble, or whistles with puckered lips and drifting musical notes (no audio). Settled reading lasts 19.5–28.6 seconds, thinking 18.2–23.4 seconds, and whistling 11.7–15.6 seconds. Activities additionally ease in over one second and out over 1.2 seconds, followed by 2.5 seconds in idle. Clicks, dragging, and falling interrupt immediately. Pause freezes animation and routine timing. Walking, idling, and sleeping remain the dominant routine choices. Laptop writing lasts 20–28 settled seconds, with the screen facing Cacti, alternating typing taps, and short thinking pauses. Future pets can override `IPet.Routines` with their own weighted activity choices and duration ranges, and interpret a neutral `PetFrame` in `IPetVisual.Paint`. The default profile only idles; Cacti explicitly opts into its walking and resting behaviors.

## Build and run

Use Windows and the .NET 10 SDK. From this folder:

```powershell
dotnet restore src/WinPet.sln
dotnet build src/WinPet.sln
dotnet run --project src/WinPet
```

Or open `src\WinPet\bin\Debug\net10.0-windows\WinPet.exe` after building. A second launch does not create a duplicate pet. Nothing is installed or added to startup.

## Visual review workflow

Every normal build regenerates **`previews/` in the source tree**: a transparent PNG and looping GIF for each catalog entry, an overview sheet, and a self-contained HTML gallery. Open `previews/index.html` in a browser to switch between stills and animations. Keep these generated artifacts in source control alongside the code that changes their appearance.

Run `tools\generate-previews.cmd` to build and regenerate, or simply run `dotnet build src/WinPet.sln`. Quit the running pet first if its executable is locked. For a quick code-only build, explicitly opt out with `dotnet build src/WinPet.sln -p:SkipPetPreviews=true`. To render without rebuilding, use `dotnet src\WinPet\bin\Debug\net10.0-windows\WinPet.dll --generate-previews previews`.

The catalog uses these terms:

| Category | Contents |
| --- | --- |
| Physical states | On ground, in air, being held |
| Activities | Standing, walking, sleeping, reading, thinking, singing/whistling, playing violin, writing on a laptop, being poked, looking around, sitting |
| Shared extras | Blinking and cursor tracking |
| Combinations | Thinking, reading, whistling, violin, and laptop writing with cursor attention and blinking |

`PetIntent` describes shared behavior and interaction states such as moving, dormancy, and falling. Each pet chooses how these look; movement can mean walking, rolling, or driving. Pet-specific **activities** are definitions carried directly by routines and menu actions. Physical interaction takes priority over the chosen activity. Cacti resolves its eyes and cursor attention independently; the shared model requires no eyes, mouth, face, or body. The gallery shows poses in place, not a collision simulation. Its eight-second transition clips shorten the settled activity duration so entrances and exits are easy to review; the actual pet keeps the durations described above.

`src/WinPet/Previewing/CactusPreviewCatalog.cs` supplies Cacti's review scenes. `src/WinPet/Previewing/PreviewGenerator.cs` owns sample times and cursor paths, and `src/WinPet/Previewing/PreviewGallery.cs` writes the HTML/Markdown reference. Rendering uses the same `PetFrame` → `PetRenderer` → `IPetVisual.Paint` path as the live pet. `src/WinPet/Previewing/GifWriter.cs` handles GIF palettes and changed-frame regions using built-in .NET/Windows drawing APIs. `src/WinPet.Tests/Previewing/PreviewChecks.cs` validates dimensions, frame counts, timing, looping, and decoding through the separate test runner. There are no external image tools or Python/PowerShell dependencies. Self-tests assert behavior; the build owns all visual artifacts.

Click the cactus for a reaction. Hold the left mouse button and drag to pick it up; release to drop or gently throw it. Airborne throws bounce softly off the left/right sides of windows and screen edges. Click Cacti in midair to bump him upward while keeping his sideways momentum; timed taps can keep him aloft. Holding and dragging still catches him, and grounded clicks keep the blush reaction. Windows block dragging as well as autonomous movement. Right-click the pet or the WinPet system-tray icon (possibly under the hidden-icons arrow) for its Windows context menu: Water, pause/resume, find desktop space, or quit. The pet stays still while the menu is open. Water is available while unpaused and grounded, plays a seven-second watering animation, then returns to idle. Clicks, dragging, and falling can interrupt it; there is no thirst meter or watering obligation.

Pets optionally expose `IPet.Actions`: validated `PetAction` definitions with a stable ID, menu label, activity, duration, and optional availability rule. The default list is empty; standard controls remain available. `PetBehavior` owns the shared routine/action timeline, completion, and interruptions. The window checks availability again when a menu choice executes. Desktop safety checks continue while the menu is open, and the popup is excluded from obstacles.

`IPet` contains behavior and footprint information only; `IPetVisual` adds drawing. The shared `PetActivity` carries an identity, while its visual meaning belongs to the pet. Cacti uses `CactusActivity` to attach artwork and facial defaults. A faceless object can use plain activities to change color or lights without constructing any facial pose. Adding a pet-specific menu action or autonomous activity requires no shared enum, animation switch, or renderer edits. For example, define `new PetAction("water", "Water", new PetActivity("water"), 7)` in that pet's action list, or supply an activity to `new PetRoutine(PetIntent.Idle, 2, 10, 20, activity)`. Routine definitions validate their weights, durations, and intent on construction. The pet interprets that activity in its own `Paint` method. `CactusPet.ComposePose` is a Cacti-specific helper, not an interface requirement. Cacti owns its activity definitions in `CactusActivities` and its watering definition and artwork in `CactusWatering`. Water is never selected autonomously.

Cacti follows the active Windows workspace and uses only its visible application windows as obstacles. Fully invisible overlays (such as NVIDIA's idle overlay) are ignored. Cacti hides only if application windows cover all usable desktop space. Restore or resize a window to expose space, and he returns automatically. He can stand above a window only if his whole body fits there.

## Source layout

Open `src/WinPet.sln` to load both projects:

```text
src/
  WinPet.sln
  WinPet/
    WinPet.csproj
  WinPet.Tests/
    WinPet.Tests.csproj
```

Each top-level type has its own matching filename, and namespaces match the folder structure.

| Folder | Namespace | Responsibility |
| --- | --- | --- |
| `src/WinPet/Domain/` | `WinPet.Domain` | `IPet`, behavior, actions, routines, physics, cursor attention, and neutral frame data |
| `src/WinPet/Desktop/` | `WinPet.Desktop` | Windows Forms views, desktop observation, native interop, and layered-window presentation |
| `src/WinPet/Rendering/` | `WinPet.Rendering` | Shared bitmap and activity-layer composition |
| `src/WinPet/Pets/Cactus/` | `WinPet.Pets.Cactus` | Cacti's definition, activities, artwork, and palette |
| `src/WinPet/Previewing/` | `WinPet.Previewing` | Preview scenes, generation, GIF encoding, and gallery output |
| `src/WinPet.Tests/` | `WinPet.Tests` and matching subnamespaces | Check runner, suites grouped like the application, and `TestDoubles/CheckPet.cs` |

The shared interface lives in `src/WinPet/Domain/IPet.cs`; `PetIntent`, `PetFrame`, and other supporting types have their own files. `src/WinPet/Program.cs` remains the application entry point. Generated images and gallery pages stay in `previews/`. Test sources are excluded from the application assembly.

## Cursor attention

Cacti notices the cursor within about 240 physical pixels of his face and gently looks toward it in any direction. He returns to his activity gaze when it moves away. Thinking and whistling follow the cursor fully; reading glances up while retaining a slight bookward bias. Eyelids blink independently during every awake action, including sitting and being held; sleeping keeps them closed. Cursor awareness is shared behavior that future pet artwork can also use.

## Checks and prototype limits

The test project keeps the existing dependency-free assertion runner (run it with `dotnet run`, rather than `dotnet test`). The application exposes its internal types only to the test assembly. Build the gallery with `dotnet build src/WinPet.sln`, then validate it with `dotnet run --project src/WinPet.Tests -p:SkipPetPreviews=true -- --preview-check previews`. Preview verification returns a nonzero exit code if an artifact is missing or invalid. The workspace check is opt-in because it opens a temporary desktop window.

`dotnet run --project src/WinPet.Tests -p:SkipPetPreviews=true` runs focused suites for physics, window filtering, routine selection, animation, artwork, actions, the live pet model, and menu bindings. `SelfChecks` writes `self-test-results.txt` beside the executable and returns a nonzero exit code on any failure, including unexpected suite exceptions. The model tests use deterministic randomness and supplied time/geometry to cover drag offsets, throwing, midair taps, lost capture, support removal, menu freezing, pause, hidden time, and recovery. Menu checks exercise bindings without creating a form or tray icon. Rendering checks cover watering containment and facial clearance across all 141 sampled frames. Visual references are generated by the build into `previews/`, including `water-sequence.png` for entry/pour/exit review. `dotnet run --project src/WinPet -- --diagnose` writes `desktop-check.txt` there with monitor counts, space availability, window classes/process names, bounds, transparency, and workspace membership; it does not collect window titles. Run desktop diagnostics normally on your Windows desktop: an isolated execution environment may not see your real windows.

`dotnet run --project src/WinPet.Tests -p:SkipPetPreviews=true -- --workspace-check` checks a temporary pet window's workspace assignment. When Windows leaves the tool window unassigned to an individual workspace, it reports that fact; it cannot substitute for checking real workspace switches.

The appearance contract supports arbitrary anatomy, including no face or body. The current motion model still uses gravity, surface movement, rectangular collision bounds, and grounded actions; custom flying or swimming mechanics are not implemented.

The prototype uses a Windows Forms layered window instead of WPF. The artwork supplies per-pixel transparency for smooth edges; fully transparent pixels pass mouse input through to the desktop. The pet has a fixed 76 × 92 physical-pixel footprint, including on mixed-DPI displays; automatic travel across monitor seams is deferred. Window geometry refreshes every 80 ms, so a rapidly moving window can briefly overlap the pet before correction. Transparent/shaped app windows are treated conservatively as solid rectangles. Auto-hidden taskbars follow Windows' reported work area. Real multi-monitor, fullscreen, and cross-app interaction still need hands-on validation.



