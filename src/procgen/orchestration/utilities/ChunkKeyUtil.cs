using Godot;
using Runevision.Common;

static class ChunkKeyUtil
{
    // positive, unique for |x|,|y| < 32768  (fits in 32‑bit signed int)
    public static int Make(Point p) =>
        ((p.x & 0xFFFF) << 16) | (p.y & 0xFFFF);
}
