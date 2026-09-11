using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WinPet;

// GDI+ encodes each complete frame and its palette. Assemble those standard GIF
// image blocks into a looping animation; no Python, external encoder or packages.
internal sealed class GifWriter : IDisposable
{
    private readonly BinaryWriter writer;
    private bool started;
    private int[]? previousPixels;
    private Size size;

    public GifWriter(string path) => writer = new BinaryWriter(File.Create(path));

    public void AddFrame(Bitmap frame, ushort delayHundredths)
    {
        if (started && frame.Size != size) throw new ArgumentException("GIF frames must have the same dimensions.");
        size = frame.Size;
        var bounds = ChangedBounds(frame);
        using var changed = frame.Clone(bounds, PixelFormat.Format32bppArgb);
        using var indexed = IndexedFrame(changed);
        using var encoded = new MemoryStream();
        indexed.Save(encoded, ImageFormat.Gif);
        byte[] bytes = encoded.ToArray();
        int paletteLength = (bytes[10] & 128) != 0 ? 3 * (1 << ((bytes[10] & 7) + 1)) : 0;
        if (!started)
        {
            writer.Write("GIF89a"u8);
            writer.Write(bytes, 6, 7 + paletteLength);
            writer.Write(new byte[] { 0x21, 0xff, 11 });
            writer.Write("NETSCAPE2.0"u8);
            writer.Write(new byte[] { 3, 1, 0, 0, 0 });
            started = true;
        }
        // Keep previous pixels outside this frame's changed rectangle. Identical
        // frames become a 1px update, keeping checked-in GIFs small without tools.
        writer.Write(new byte[] { 0x21, 0xf9, 4, 4 });
        writer.Write(delayHundredths);
        writer.Write((ushort)0);
        int offset = 13 + paletteLength;
        while (bytes[offset] == 0x21)
        {
            offset += 2;
            SkipBlocks(bytes, ref offset);
        }
        if (bytes[offset] != 0x2c) throw new InvalidDataException("GIF encoder did not emit an image.");
        writer.Write((byte)0x2c);
        writer.Write((ushort)bounds.X);
        writer.Write((ushort)bounds.Y);
        writer.Write(bytes, offset + 5, 4);
        byte packed = bytes[offset + 9];
        offset += 10;
        if ((packed & 128) != 0)
        {
            writer.Write(packed);
            int localLength = 3 * (1 << ((packed & 7) + 1));
            writer.Write(bytes, offset, localLength);
            offset += localLength;
        }
        else
        {
            if (paletteLength == 0) throw new InvalidDataException("GIF frame has no palette.");
            writer.Write((byte)(packed | 128 | (bytes[10] & 7)));
            writer.Write(bytes, 13, paletteLength);
        }
        int imageStart = offset++;
        SkipBlocks(bytes, ref offset);
        writer.Write(bytes, imageStart, offset - imageStart);
    }

    private static Bitmap IndexedFrame(Bitmap frame)
    {
        // Supplying an explicit palette avoids GDI+'s default halftone dithering,
        // which otherwise turns the cream background and green body into speckles.
        var colors = new int[frame.Width * frame.Height];
        var source = frame.LockBits(new Rectangle(Point.Empty, frame.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            for (int y = 0; y < frame.Height; y++)
                Marshal.Copy(source.Scan0 + y * source.Stride, colors, y * frame.Width, frame.Width);
        }
        finally { frame.UnlockBits(source); }
        var paletteColors = colors.GroupBy(c => c).OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
            .Take(256).Select(g => Color.FromArgb(g.Key)).ToArray();
        var lookup = paletteColors.Select((c, i) => (c, i)).ToDictionary(p => p.c.ToArgb(), p => (byte)p.i);
        foreach (int color in colors.Distinct())
        {
            if (lookup.ContainsKey(color)) continue;
            var c = Color.FromArgb(color);
            int best = int.MaxValue;
            byte nearest = 0;
            for (int i = 0; i < paletteColors.Length; i++)
            {
                var p = paletteColors[i];
                int distance = (c.R - p.R) * (c.R - p.R) + (c.G - p.G) * (c.G - p.G) + (c.B - p.B) * (c.B - p.B);
                if (distance < best) { best = distance; nearest = (byte)i; }
            }
            lookup[color] = nearest;
        }
        var indexed = new Bitmap(frame.Width, frame.Height, PixelFormat.Format8bppIndexed);
        var palette = indexed.Palette;
        for (int i = 0; i < paletteColors.Length; i++) palette.Entries[i] = paletteColors[i];
        indexed.Palette = palette;
        var target = indexed.LockBits(new Rectangle(Point.Empty, indexed.Size), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
        try
        {
            var row = new byte[target.Stride];
            for (int y = 0; y < frame.Height; y++)
            {
                for (int x = 0; x < frame.Width; x++) row[x] = lookup[colors[y * frame.Width + x]];
                Marshal.Copy(row, 0, target.Scan0 + y * target.Stride, row.Length);
            }
        }
        finally { indexed.UnlockBits(target); }
        return indexed;
    }

    private Rectangle ChangedBounds(Bitmap frame)
    {
        var pixels = new int[frame.Width * frame.Height];
        var data = frame.LockBits(new Rectangle(Point.Empty, frame.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            for (int y = 0; y < frame.Height; y++)
                Marshal.Copy(data.Scan0 + y * data.Stride, pixels, y * frame.Width, frame.Width);
        }
        finally { frame.UnlockBits(data); }
        int left = frame.Width, top = frame.Height, right = -1, bottom = -1;
        if (previousPixels is not null)
        {
            for (int y = 0; y < frame.Height; y++)
            for (int x = 0; x < frame.Width; x++)
            {
                int index = y * frame.Width + x;
                if (pixels[index] == previousPixels[index]) continue;
                left = Math.Min(left, x); right = Math.Max(right, x);
                top = Math.Min(top, y); bottom = Math.Max(bottom, y);
            }
        }
        var bounds = previousPixels is null ? new Rectangle(Point.Empty, frame.Size)
            : right < 0 ? new Rectangle(0, 0, 1, 1) : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        previousPixels = pixels;
        return bounds;
    }

    private static void SkipBlocks(byte[] bytes, ref int offset)
    {
        int count;
        while ((count = bytes[offset++]) != 0) offset += count;
    }

    public void Dispose()
    {
        if (started) writer.Write((byte)0x3b);
        writer.Dispose();
    }
}
