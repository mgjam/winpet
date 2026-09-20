# Windows Desktop Pet — Architecture

The application is a .NET 10 Windows Forms project, with a separate `src/WinPet.Tests` executable project for verification. The design follows [CONTRIBUTING.md](CONTRIBUTING.md): domain models own behavior, desktop adapters own Windows integration, and pet definitions own artwork and activities. No dependency-injection container, service framework, or plugin loader is required.

## Live pet model

Source folders and namespaces follow the same boundaries: `WinPet.Domain` for shared models and animation state, `WinPet.Desktop` for Windows integration, `WinPet.Rendering` for bitmap composition, `WinPet.Pets.Cactus` for Cacti's artwork, and `WinPet.Previewing` for gallery tooling. Tests mirror these folders under `WinPet.Tests`, with shared test doubles in `TestDoubles`. Every top-level type has a matching source filename; `IPet` lives in `src/WinPet/Domain/IPet.cs`.

`PetSession` is the live pet: position, velocity, facing, pause state, mouse interaction, cursor attention, and animation clocks. It receives immutable `World` snapshots, cursor coordinates, elapsed time, and an injected `Random`. It can be exercised without creating a form, enumerating native windows, reading the real cursor, or sleeping.

The model composes focused domain units:

- `World` owns valid desktop geometry, nearest-space recovery, support, and collision-safe movement. It copies input collections so a snapshot cannot change underneath a simulation.
- `PetMotion` applies collision responses, side rebounds, and midair bumps.
- `PetBehavior` owns the timeline for autonomous routines and requested actions: start, completion, rest, and interruption.
- `PetAnimationClock` separates visual time from routine time. Holding freezes routine time; pause, hidden space, and an open menu freeze both.
- `CursorAttention` calculates and smooths cursor direction and proximity. `PetFrame` exposes intent, visual time, facing, attention, activity identity, and local activity timing without prescribing anatomy.

The session is the authority for action eligibility and pause state. A menu reads those facts and requests an action; it cannot mutate behavior timing or velocity itself.

## Pet definitions and extension

`IPet` describes a name, footprint, attention origin, routine choices, and optional menu actions. It has no drawing method or pose composer. `IPetVisual` in `Rendering` extends it with `Paint(Graphics, PetFrame)` for desktop presentation; a simulation-only definition implements just `IPet`. The renderer forwards the neutral frame to the pet without a species or anatomy switch. A pet may be a blob, mouth, disembodied face, car, or any other shape.

`PetIntent` describes idle, move, observe, rest, dormant, react, dragged, and falling states. Cacti interprets move as walking and dormant as sleeping; a car can drive and turn off its engine. The default routine profile only idles. Each pet opts into other routines. Dormant routines may carry an activity, and cursor attention remains available in every state; how it is expressed is a visual decision.

`PetActivity` is an identity-bearing behavior definition with no drawing callbacks or facial fields. A pet can associate it with its own artwork or derive a pet-specific definition; Cacti uses `CactusActivity` for a drawing callback, gaze defaults, cursor weight, and mouth style. A `PetRoutine` carries an intent, weight, duration range, and optional activity. Definitions reject invalid weights, durations, and physical-interaction intents at construction. Weighted selection avoids repeating the same intent/activity combination when alternatives exist and falls back to idle for an empty profile.

`PetAction` carries a stable ID, menu label, finite duration, activity, and optional eligibility predicate. `PetActionContext` supplies interaction facts without Windows handles. Actions are optional; the default list is empty.

Adding a new autonomous activity or menu action requires a pet-owned definition and artist. It does not require changing shared enums, animation switches, or renderer dispatchers. Cacti's reusable activity definitions live in `CactusActivities`, and its watering definition and geometry live in `CactusWatering`. Water is not an autonomous routine.

A requested action replaces the current routine. Activity completion clears the artwork and rests in idle for 2.5 seconds. A click, drag, or fall interrupts the activity. Landing rests for 1.4 seconds. Time spent paused or unavailable does not consume an activity's duration.

The current simulation still deliberately uses rectangular collision bounds, gravity, surface movement, and grounded actions. These are shared desktop-physics rules, not anatomy requirements. Flying, swimming, and custom collision geometry would require a separate motion-policy extension; the visual contract does not claim to implement those mechanics.

## Desktop adapter and resource ownership

`PetWindow` translates Windows mouse/timer events into session operations and renders the resulting `PetFrame`. It owns the timer, tray icon, menu, icon, and form lifetime. It contains no drag physics, throw calculation, routine selection, or cursor-attention rules.

`PetMenu` displays commands, reads eligibility from the session, and raises desktop command requests. `DesktopWorld` reads native window facts and constructs a `World`; `Native` contains platform declarations and constants. `VirtualDesktops` uses the public virtual-desktop API to follow the current workspace without switching the user's workspace.

The form is a pet-sized, borderless, topmost layered window that avoids activation and taskbar presence. Bitmap alpha supplies native hit testing: transparent pixels pass mouse input through. `PetRenderer` composes bitmaps; `LayeredWindow` presents them and releases acquired GDI handles even if later acquisition or presentation fails. Pet artwork and preview generation do not depend on a window handle.

Desktop geometry is refreshed every 80 ms. While a context menu holds movement and animation still, geometry polling continues; the popup is excluded from obstacles. Unavailable space hides the pet and cancels a grab. The tray remains usable when the pet is hidden. The adapter refreshes desktop facts before executing a requested action.

## Animation and artwork

Cacti owns `CactusAnimation`, `CactusPose`, `FacePose`, `EyePose`, `MouthShape`, and `ActivityPose` in `Pets/Cactus`. They are not part of the shared domain or renderer contract. Its composition priorities are:

1. Intent and activity supply body stride, default gaze, and expression. Local entrance/exit timing blends activity gaze toward idle.
2. Smoothed cursor attention blends over the activity's gaze with a pet-defined weight.
3. Blinking uses the independent facial clock; sleep closes the eyes and ignores attention.
4. Physical interaction suppresses activity props and supplies its own expression.

`PetActivityRenderer` is an optional drawing helper that applies a transparent layer and fade to a supplied paint callback. It knows nothing about activity types or faces. Each artist owns its geometry and local movement. `CactusPalette` holds shared body/prop materials. Domain movement never changes artwork bounds, keeping collision geometry stable.

## Verification and previews

`SelfChecks` in `WinPet.Tests` runs focused suites and owns reporting and exit status. Each suite reports assertions; an unexpected suite exception is recorded as a failure and does not prevent subsequent suites from running. Tests cover geometry, native-window filtering policy, weighted routines, expression composition, action lifecycle, live-session interactions, and menu bindings. Model tests inject time, geometry, and randomness. Contract tests exercise faceless, mouth-only, eyes-only, face-only, and car implementations through real sessions and bitmap rendering, plus a definition with no painter at all. Menu tests create neither a form nor a tray icon.

`CactusPreviewCatalog` supplies scenes to the pet-independent `PreviewGenerator`. The generator supplies the same neutral frame to the bitmap renderer as the live session; Cacti composes its own facial pose inside `Paint`. `PreviewGallery` owns HTML/Markdown output; `GifWriter` owns palette encoding and changed-frame storage. `PreviewChecks` in the test project verifies dimensions, frame timing, looping, and decoded frames via an explicit `--preview-check` command. Every ordinary build regenerates the checked-in gallery.

`WorkspaceChecks` is an explicit Windows integration diagnostic in the test project. The application excludes all test sources and grants the test assembly access to its internals through `InternalsVisibleTo`. Automated model checks cannot replace hands-on validation of native focus, virtual-desktop switching, mixed-DPI monitors, and cross-app input routing. See [README.md](README.md) for commands and current platform limitations.

