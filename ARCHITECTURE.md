# Windows Desktop Pet — Architecture Proposal

Prototype note: the initial implementation uses .NET 10 and a pet-sized Windows Forms window with per-pixel alpha rendering and no-activation styles. This keeps rendering and input geometry together without a desktop-sized overlay. `IPet` separates cactus artwork from shared motion. The remaining sections describe the original proposal; see README for current limitations.

Workspace handling uses the public [IVirtualDesktopManager API](https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-ivirtualdesktopmanager) to reject windows on inactive workspaces. Cacti's unowned tool window can remain unassigned to a particular workspace; if Windows does assign it and it becomes inactive, move only that window to the active workspace or recreate its handle when no current app can supply a workspace ID. Never switch the user's workspace. Ignore layered windows whose global alpha is explicitly zero; keep ordinary and partly transparent apps solid.

## Starting point

Build a small Windows desktop app in C#/.NET, likely using WPF for rendering and Win32/DWM interop for window geometry and mouse routing. Choose a supported .NET SDK at implementation time. Keep one application project and a few focused components; no framework, service layer, or plugin system is needed.

This is a prototype proposal, not a fixed specification. Validate overlay behavior, input routing, and window bounds first. Change rendering technology, overlay arrangement, or collision implementation when experiments justify it, while preserving the behavior in [PRODUCT.md](PRODUCT.md).

## Overlay and input

Use a borderless transparent overlay, excluded from the taskbar and normal activation. It must not steal keyboard focus. One overlay per monitor is a reasonable starting point; a pet-sized overlay is another option if it makes input isolation more reliable.

Only the visible pet's interactive shape should receive mouse input. Transparent space must pass clicks, scrolling, and dragging to the desktop or applications below. Prototype selective native hit-testing or window regions, including cross-process behavior; do not assume WPF transparency or `IsHitTestVisible` alone provides desktop-wide click-through.

Capture the mouse for an active pet drag and release capture on drop or cancellation. Resolve requested positions against the world geometry, so dragging over an app does not render the pet inside it. A tray menu provides pause/resume and quit without adding persistent controls.

## World geometry

Enumerate top-level windows with Win32 and obtain visible frame bounds through DWM where available. Exclude the pet's own windows, desktop shell surfaces, minimized windows, and cloaked/hidden windows. Include ordinary application windows and dialogs; document any unusual transparent or shaped-window limitations found during testing.

Start with conservative rectangular solid obstacles. Their union is forbidden space. Window top edges provide support only where the pet's entire body fits outside all obstacles; another window may block a potential platform. DWM frame bounds should help avoid treating invisible resize margins or shadows as solid surfaces.

Refresh geometry at a modest interval; add window-event hooks only if polling proves inadequate. Track supporting windows and revalidate the pet after changes. If overlap appears, relocate to a nearby valid position without animating through forbidden interiors; hide if no valid position exists. Restore it when space returns.

## Motion and behavior

Keep position, velocity, body bounds, facing, and current behavior in a small pet model. A bounded timestep update advances motion independently of rendering. Use gravity and swept or substepped collision checks to avoid tunneling through thin obstacles during drops.

Resolve against the pet's full body, not just its center: stop horizontal motion at sides, cancel downward velocity on landing, and block motion into window undersides. Monitor work-area bottoms are ground. Removing support resumes falling. Keep animation separate from collision bounds to avoid jitter.

A small state machine is sufficient: idle, walk, look around, sit, sleep, react, dragged, and falling. Timers select quiet autonomous behaviors; direct interaction temporarily overrides them. Keep speeds and durations as a few tunable constants. Throw velocity is optional and should be capped if implemented.

## Monitors, DPI, and taskbars

Use one consistent world-coordinate system, preferably physical screen pixels, with explicit conversion to WPF units per monitor. Support negative monitor coordinates and mixed DPI. Derive each monitor's usable area from Windows work-area information to keep the pet out of taskbars and reserved desktop strips.

Treat gaps between monitors as outside the world. Permit crossing an adjoining monitor boundary only where the body fits in valid space; retaining the pet on its current monitor is an acceptable initial simplification. Recompute bounds on display, DPI, or work-area changes and safely reposition after a monitor disconnect. Fullscreen or maximized coverage may leave no room: hide quietly, then recover.

## Implementation and validation

Suggested responsibilities: overlay/input, window geometry, pet motion/behavior, and rendering. These can be a few files in one project. Keep native interop together and geometry calculations independent enough to test without a live desktop.

First prove a placeholder pet can coexist with other apps without intercepting their input. Then add obstacles and gravity, dragging and reactions, and finally simple rituals/animation.

Manually verify click-through and focus behavior across different apps; side collisions and top-edge landing; fast drops; dragging across occupied space; moving/resizing/removing support; maximized/fullscreen coverage; taskbars; and mixed-DPI monitors. Add focused automated tests for geometry and landing where useful. Record prototype limitations rather than growing v1 to solve every shell edge case.

