using WinPet.Domain;

namespace WinPet.Previewing;

internal sealed record PreviewScene(string Id, string Group, string Title, string Description,
    PetIntent Intent, bool TrackCursor = false, bool Transition = false,
    PetAction? Action = null, PetActivity? Activity = null);
