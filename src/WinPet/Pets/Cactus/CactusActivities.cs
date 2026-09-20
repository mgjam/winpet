using WinPet.Domain;

namespace WinPet.Pets.Cactus;

// Cacti's activity definitions are shared by its routines and preview catalog.
internal static class CactusActivities
{
    public static CactusActivity Read { get; } = new("read", CactusReading.Paint, t => new((float)Math.Sin(t * 1.8) * .48f, 1), .85f);
    public static CactusActivity Think { get; } = new("think", CactusThinking.Paint, _ => new(.6f, -.75f));
    public static CactusActivity Sing { get; } = new("sing", CactusSinging.Paint, mouth: MouthShape.Whistle);
    public static CactusActivity Violin { get; } = new("violin", CactusViolin.Paint, _ => new(.25f, .35f));
    public static CactusActivity Laptop { get; } = new("laptop", CactusLaptop.Paint, _ => new(0, .8f), .85f);
}
