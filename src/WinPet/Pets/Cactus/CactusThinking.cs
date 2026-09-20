namespace WinPet.Pets.Cactus;

internal static class CactusThinking
{
    public static void Paint(Graphics g, double seconds, float amount)
    {
        using var outline = new Pen(Color.FromArgb(36, 70, 53), 2);
        g.FillEllipse(Brushes.Ivory, 53, 3, 19, 15);
        g.DrawEllipse(outline, 53, 3, 19, 15);
        g.FillEllipse(Brushes.Ivory, 55, 19, 4, 4);
        g.FillEllipse(Brushes.Ivory, 53, 25, 3, 3);
        int dots = 1 + (int)(seconds % 3);
        float dotsLeft = 62.5f - ((dots - 1) * 5 + 2) / 2f;
        for (int i = 0; i < dots; i++) g.FillEllipse(Brushes.DarkSlateGray, dotsLeft + i * 5, 9.5f, 2, 2);
    }
}
