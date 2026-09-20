using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using WinPet.Domain;

namespace WinPet.Rendering;

// Shared transparent layer and fade. Each activity owns its geometry and entrance
// motion; facial animation and the scheduler never need to know those details.
internal static class PetActivityRenderer
{
    public static void Paint(Graphics g, Size size, Action<Graphics, double, float> paint, double seconds, float amount)
    {
        if (amount <= 0) return;
        using var props = new Bitmap(size.Width, size.Height);
        using (var pg = Graphics.FromImage(props))
        {
            pg.SmoothingMode = SmoothingMode.AntiAlias;
            paint(pg, seconds, amount);
        }
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix { Matrix33 = amount });
        g.DrawImage(props, new Rectangle(Point.Empty, size), 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, attributes);
    }
}
