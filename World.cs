namespace WinPet;

internal sealed record World(IReadOnlyList<RectangleF> Areas, IReadOnlyList<RectangleF> Obstacles)
{
    public bool Fits(PointF position, SizeF size)
    {
        var body = new RectangleF(position, size);
        return Areas.Any(area => area.Contains(body)) && !Obstacles.Any(body.IntersectsWith);
    }

    public PointF? FindSpace(PointF near, SizeF size)
    {
        if (Fits(near, size)) return near;
        PointF? best = null;
        float distance = float.MaxValue;
        foreach (var area in Areas)
        {
            if (area.Width < size.Width || area.Height < size.Height) continue;
            // Free-space corners are formed by work-area and obstacle boundaries.
            var xs = new List<float> { area.Left, area.Right - size.Width, Math.Clamp(near.X, area.Left, area.Right - size.Width) };
            var ys = new List<float> { area.Top, area.Bottom - size.Height, Math.Clamp(near.Y, area.Top, area.Bottom - size.Height) };
            foreach (var obstacle in Obstacles.Where(o => o.IntersectsWith(area)))
            {
                xs.Add(obstacle.Left - size.Width);
                xs.Add(obstacle.Right);
                ys.Add(obstacle.Top - size.Height);
                ys.Add(obstacle.Bottom);
            }
            foreach (float x in xs.Distinct())
            foreach (float y in ys.Distinct())
            {
                var candidate = new PointF(x, y);
                float d = (x - near.X) * (x - near.X) + (y - near.Y) * (y - near.Y);
                if (d < distance && Fits(candidate, size)) { best = candidate; distance = d; }
            }
        }
        return best;
    }

    public bool Supported(PointF position, SizeF size) => !Fits(new PointF(position.X, position.Y + 0.5f), size);

    public PointF Move(PointF position, SizeF size, float dx, float dy, out bool hitX, out bool hitY)
    {
        hitX = hitY = false;
        // Steps below one pixel prevent tunneling, including fast flicks and thin windows.
        int steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Abs(dx), Math.Abs(dy))));
        float sx = dx / steps, sy = dy / steps;
        for (int i = 0; i < steps; i++)
        {
            if (sx != 0)
            {
                var next = new PointF(position.X + sx, position.Y);
                if (Fits(next, size)) position = next;
                else { position = Approach(position, size, sx, 0); hitX = true; sx = 0; }
            }
            if (sy != 0)
            {
                var next = new PointF(position.X, position.Y + sy);
                if (Fits(next, size)) position = next;
                else { position = Approach(position, size, 0, sy); hitY = true; sy = 0; }
            }
        }
        return position;
    }

    private PointF Approach(PointF position, SizeF size, float dx, float dy)
    {
        float low = 0, high = 1;
        for (int i = 0; i < 12; i++)
        {
            float middle = (low + high) / 2;
            if (Fits(new PointF(position.X + dx * middle, position.Y + dy * middle), size)) low = middle;
            else high = middle;
        }
        return new PointF(position.X + dx * low, position.Y + dy * low);
    }
}
