using WinPet.Domain;
using WinPet.Pets.Cactus;

namespace WinPet.Previewing;

// Cacti's review scenes; the generator is independent of any particular pet.
internal static class CactusPreviewCatalog
{
    public static IReadOnlyList<PreviewScene> Scenes { get; } = Array.AsReadOnly<PreviewScene>([
        new("on-ground", "Physical states", "On ground", "Supported, standing quietly.", PetIntent.Idle),
        new("in-air", "Physical states", "In air", "Airborne expression; blinking continues. Midair pokes keep this expression.", PetIntent.Falling),
        new("held", "Physical states", "Being held", "Picked up: wide eyes and blush, with independent blinking.", PetIntent.Dragged),
        new("standing", "Activities", "Standing", "Idle gaze and natural blinking.", PetIntent.Idle),
        new("walking", "Activities", "Walking", "Foot stride and blinking run independently.", PetIntent.Move),
        new("sleeping", "Activities", "Sleeping", "Eyes stay closed, even with a nearby cursor.", PetIntent.Dormant, true),
        new("reading", "Activities", "Reading", "Open the book, read and turn pages, then put it away. Review clip uses a shortened hold.", PetIntent.Idle, false, true, Activity: CactusActivities.Read),
        new("thinking", "Activities", "Thinking", "Thought bubble with centered dots; blink continues. Review clip uses a shortened hold.", PetIntent.Idle, false, true, Activity: CactusActivities.Think),
        new("singing", "Activities", "Singing / whistling", "Puckered lips and silent notes. Review clip uses a shortened hold.", PetIntent.Idle, false, true, Activity: CactusActivities.Sing),
        new("laptop", "Activities", "Writing on laptop", "Alternating typing taps and a short thinking pause. Settled writing lasts 20–28 seconds; this review shortens the hold.", PetIntent.Idle, false, true, Activity: CactusActivities.Laptop),
        new("violin", "Activities", "Playing violin", "Warm wooden violin and back-and-forth bowing (silent). Settled playing lasts 18–25 seconds; this review shortens the hold.", PetIntent.Idle, false, true, Activity: CactusActivities.Violin),
        new("poked", "Activities", "Being poked", "Grounded click reaction: blush and smile.", PetIntent.React),
        new("watering", "Activities", "Watering", "Cacti holds a little watering can, tips it into the pot, then puts it away.", PetIntent.Idle, false, true, CactusWatering.Action),
        new("looking", "Activities", "Looking around", "Autonomous side-to-side gaze.", PetIntent.Observe),
        new("sitting", "Activities", "Sitting", "Resting awake; eyelids still blink.", PetIntent.Rest),
        new("blinking", "Shared extras", "Blinking", "Continuous close, brief hold, reopen; independent of activity time.", PetIntent.Idle),
        new("cursor-tracking", "Shared extras", "Cursor tracking", "Marker moves left, up, right, down, then away; eyes ease back to their default.", PetIntent.Idle, true),
        new("thinking-attention", "Combinations", "Thinking + attention + blink", "Bubble, cursor gaze and eyelids coexist.", PetIntent.Idle, true, Activity: CactusActivities.Think),
        new("reading-attention", "Combinations", "Reading + attention + blink", "Glances toward cursor with a slight bookward bias.", PetIntent.Idle, true, Activity: CactusActivities.Read),
        new("singing-attention", "Combinations", "Whistling + attention + blink", "Mouth and notes continue while eyes track and blink.", PetIntent.Idle, true, Activity: CactusActivities.Sing),
        new("laptop-attention", "Combinations", "Laptop + attention + blink", "Typing continues while eyes follow the cursor and blink.", PetIntent.Idle, true, Activity: CactusActivities.Laptop),
        new("violin-attention", "Combinations", "Violin + attention + blink", "Bowing continues while eyes follow the cursor and blink.", PetIntent.Idle, true, Activity: CactusActivities.Violin)
    ]);
}
