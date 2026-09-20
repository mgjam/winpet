using System.Drawing.Imaging;
using WinPet.Domain;

namespace WinPet.Rendering;

// Pure bitmap composition, shared by the desktop, previews and rendering checks.
internal static class PetRenderer
{
    public static Bitmap Frame(IPetVisual pet, PetIntent intent, double seconds, int facing, CursorAttention attention = default,
        RoutinePose? routine = null, PetActivity? activity = null)
        => Frame(pet, new PetFrame(intent, seconds, facing, attention, routine, activity));

    public static Bitmap Frame(IPetVisual pet, PetFrame frameState)
    {
        var frame = new Bitmap(pet.Size.Width, pet.Size.Height, PixelFormat.Format32bppPArgb);
        try
        {
            using var graphics = Graphics.FromImage(frame);
            graphics.Clear(Color.Transparent);
            pet.Paint(graphics, frameState);
            return frame;
        }
        catch
        {
            frame.Dispose();
            throw;
        }
    }
}
