# Windows Desktop Pet — Product

## Purpose

A quiet companion for the small pauses in work: waiting for a build, a response, or a meeting to move on. It offers a moment of amusement without pulling the user into another app or an engagement loop. The first pet is Cacti, a small potted cactus; keep appearance separate from behavior so additional pets and selection can be added later.

The user already has a pet inside VS Code. This companion complements it specifically while working outside VS Code; integration with the editor pet is unnecessary.

## Core experience

- The exposed desktop is the playground. Application-window interiors are forbidden, including during dragging and falling. The pet must never appear to walk across an app's content.
- Cacti stays available on the active Windows workspace when the user switches among workspaces. Only visible windows on that workspace occupy his playground; fully invisible overlays do not.
- Window sides are solid obstacles; accessible top edges are platforms. The pet can bump into a side, turn around, fall onto a top edge, and walk along it while its body remains outside the window.
- The pet quietly alternates between walking, idling, looking around, sitting, and sleeping. A few simple rituals give it personality without demanding attention.
- Clicking the pet produces a brief reaction. Dragging picks it up; releasing drops it with gravity until it lands on a valid surface. A modest flick/throw is optional if easy to implement reliably.
- If a supporting window moves, closes, or minimizes, the pet falls or is safely repositioned. If a window moves over it, resolve the overlap without drawing it over application content.
- When no space can contain the pet, it quietly hides and returns when space becomes available. It does not force windows to move or shrink itself to fit every gap.

## V1 scope

One cactus with simple animation, autonomous behavior, click reactions, drag-and-drop, gravity, and stable landing. A transparent overlay receives input only on the pet; the rest of the desktop and applications work normally. Include a small tray menu to pause/resume and quit.

Respect monitor work areas and taskbars. Handle ordinary window movement, resizing, minimizing, and closing. Prefer predictable behavior over elaborate physics or artwork; placeholder visuals are acceptable for the first prototype.

## Non-goals

No progression, feeding obligations, currencies, achievements, streaks, quests, notifications, social feed, or gamification. No sounds or unsolicited prompts in v1. No objects to throw at the pet, inventory, elaborate simulation, editor integration, or app-content interaction. No installer, auto-update system, or automatic startup required for the prototype.

## Success criterion

During a normal work session outside VS Code, the pet provides occasional amusement without creating a new task or interrupting work. It stays out of application interiors, reacts to clicks, can be picked up and dropped, lands reliably, and leaves ordinary mouse and keyboard interaction unaffected. Maximized windows and unavailable desktop space cause it to disappear quietly rather than violate these boundaries.
