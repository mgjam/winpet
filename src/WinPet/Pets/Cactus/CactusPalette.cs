using System.Drawing.Drawing2D;

namespace WinPet.Pets.Cactus;

// Shared materials keep Cacti and its props in the same visual family.
internal static class CactusPalette
{
    public static readonly Color Edge = Color.FromArgb(36, 70, 53);
    public static readonly Color Green = Color.FromArgb(105, 170, 107);
    public static readonly Color PropTeal = Color.FromArgb(123, 165, 151);
    public static readonly Color PropHighlight = Color.FromArgb(211, 225, 213);
    public static readonly Color Water = Color.FromArgb(130, 181, 184);

    public static Pen Outline(float width = 2) => new(Edge, width)
    { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
}
