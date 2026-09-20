using System.Drawing.Imaging;

namespace WinPet.Tests.Previewing;

// Validates generated artifacts independently of the production generator.
internal static class PreviewChecks
{
    public static void Verify(string directory, IEnumerable<string> sceneIds, int frames, int delay, Size petSize)
    {
        foreach (string id in sceneIds)
        {
            using var still = new Bitmap(Path.Combine(directory, id + ".png"));
            using var gif = Image.FromFile(Path.Combine(directory, id + ".gif"));
            if (still.Size != petSize || gif.Size != new Size(240, 208) || gif.GetFrameCount(FrameDimension.Time) != frames)
                throw new InvalidDataException($"Invalid preview dimensions or frame count: {id}");
            byte[] delays = gif.GetPropertyItem(0x5100)?.Value ?? [];
            if (delays.Length != frames * 4 || Enumerable.Range(0, frames).Any(i => BitConverter.ToInt32(delays, i * 4) != delay))
                throw new InvalidDataException($"Invalid preview frame timing: {id}");
            byte[] loop = gif.GetPropertyItem(0x5101)?.Value ?? [];
            if (loop.Length < 2 || BitConverter.ToUInt16(loop) != 0)
                throw new InvalidDataException($"Preview must loop continuously: {id}");
            // Force the decoder through the entire animation, including delta frames.
            for (int i = 0; i < frames; i++)
            {
                gif.SelectActiveFrame(FrameDimension.Time, i);
                using var decoded = new Bitmap(gif);
                if (decoded.GetPixel(0, 0).ToArgb() != Color.FromArgb(245, 242, 232).ToArgb())
                    throw new InvalidDataException($"Preview palette/background changed: {id}, frame {i}");
            }
        }
    }
}
