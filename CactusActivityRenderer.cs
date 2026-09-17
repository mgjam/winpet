using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace WinPet;

// Shared transparent layer and fade. Each activity owns its geometry and entrance
// motion; facial animation and the scheduler never need to know those details.
internal static class CactusActivityRenderer
{
    public static void Paint(Graphics g, Size size, ActivityPose activity)
    {
        if (activity.Kind == PetActivity.None || activity.Amount <= 0) return;
        using var props = new Bitmap(size.Width, size.Height);
        using (var pg = Graphics.FromImage(props))
        {
            pg.SmoothingMode = SmoothingMode.AntiAlias;
            switch (activity.Kind)
            {
                case PetActivity.Read: CactusReading.Paint(pg, activity.Seconds, activity.Amount); break;
                case PetActivity.Think: CactusThinking.Paint(pg, activity.Seconds, activity.Amount); break;
                case PetActivity.Sing: CactusSinging.Paint(pg, activity.Seconds, activity.Amount); break;
                case PetActivity.Laptop: CactusLaptop.Paint(pg, activity.Seconds, activity.Amount); break;
                case PetActivity.Violin: CactusViolin.Paint(pg, activity.Seconds, activity.Amount); break;
                default: throw new ArgumentOutOfRangeException(nameof(activity), activity.Kind, "Missing activity artwork.");
            }
        }
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix { Matrix33 = activity.Amount });
        g.DrawImage(props, new Rectangle(Point.Empty, size), 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, attributes);
    }
}
