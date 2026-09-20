namespace WinPet.Pets.Cactus;

internal static class CactusSinging
{
    public static void Paint(Graphics g, double seconds, float amount)
    {
        for (int i = 0; i < 3; i++)
        {
            float phase = (float)((seconds * .32 + i / 3.0) % 1);
            float x = i == 1 ? 6 : 68 + (float)Math.Sin(phase * Math.PI * 2);
            float y = 34 - phase * 28;
            float opacity = Math.Min(1, Math.Min(phase / .12f, (1 - phase) / .2f));
            CactusMusicNotes.Paint(g, x, y, opacity);
        }
    }
}
