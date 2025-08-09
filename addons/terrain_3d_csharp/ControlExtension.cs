using System;

namespace Terrain3D.Scripts.Utilities;

public static class ControlExtension
{
    // Bit layout (Terrain3D 0.9.2 / 0.9.3a assumed)
    // |31........27|26.....22|21........14|13..10|9..6|5 4 3 2|1|0|
    // | base (5)   | over (5)| blend (8)  | ang4 |sc3 | rsvd |N|A|
    // NOTE: If upstream changes, adjust constants only.
    private const int BASE_BITS = 5;          // 0..31
    private const int OVERLAY_BITS = 5;       // 0..31
    private const int BLEND_BITS = 8;         // 0..255
    private const int ANGLE_BITS = 4;         // 16 steps (22.5° each)
    private const int SCALE_BITS = 3;         // 0..7 (mapped in plugin)
    private const int FLAG_HOLE_BIT = 2;      // (kept for reference – may differ)
    private const int FLAG_NAV_BIT = 1;
    private const int FLAG_AUTOSHADER_BIT = 0;

    private const int OFFSET_BASE = 32 - BASE_BITS; // 27
    private const int OFFSET_OVERLAY = OFFSET_BASE - OVERLAY_BITS; // 22
    private const int OFFSET_BLEND = OFFSET_OVERLAY - BLEND_BITS;  // 14
    private const int OFFSET_ANGLE = OFFSET_BLEND - ANGLE_BITS;    // 10
    private const int OFFSET_SCALE = OFFSET_ANGLE - SCALE_BITS;    // 6

    private static uint Mask(int bits) => (uint)((1 << bits) - 1);

    public static byte GetBaseTextureId(this uint control)
        => (byte)((control >> OFFSET_BASE) & Mask(BASE_BITS));

    public static void SetBaseTextureId(this ref uint control, byte baseTextureId)
        => control = (control & ~(Mask(BASE_BITS) << OFFSET_BASE)) | (uint)((baseTextureId & Mask(BASE_BITS)) << OFFSET_BASE);

    public static byte GetOverlayTextureId(this uint control)
        => (byte)((control >> OFFSET_OVERLAY) & Mask(OVERLAY_BITS));

    public static void SetOverlayTextureId(this ref uint control, byte overLayTextureId)
        => control = (control & ~(Mask(OVERLAY_BITS) << OFFSET_OVERLAY)) | (uint)((overLayTextureId & Mask(OVERLAY_BITS)) << OFFSET_OVERLAY);

    public static byte GetTextureBlend(this uint control)
        => (byte)((control >> OFFSET_BLEND) & Mask(BLEND_BITS));

    public static void SetTextureBlend(this ref uint control, byte blend)
        => control = (control & ~(Mask(BLEND_BITS) << OFFSET_BLEND)) | (uint)((blend & Mask(BLEND_BITS)) << OFFSET_BLEND);

    public static byte GetUvAngle(this uint control)
        => (byte)((control >> OFFSET_ANGLE) & Mask(ANGLE_BITS));

    public static void SetUvAngle(this ref uint control, byte uVAngle)
        => control = (control & ~(Mask(ANGLE_BITS) << OFFSET_ANGLE)) | (uint)((uVAngle & Mask(ANGLE_BITS)) << OFFSET_ANGLE);

    public static byte GetUvScale(this uint control)
        => (byte)((control >> OFFSET_SCALE) & Mask(SCALE_BITS));

    public static void SetUvScale(this ref uint control, byte uvScale)
        => control = (control & ~(Mask(SCALE_BITS) << OFFSET_SCALE)) | (uint)((uvScale & Mask(SCALE_BITS)) << OFFSET_SCALE);

    public static bool IsHole(this uint control)
        => ((control >> FLAG_HOLE_BIT) & 0x1) == 1;

    public static void SetHole(this ref uint control, bool hole)
        => control = (control & ~((uint)0x1 << FLAG_HOLE_BIT)) | (uint)((hole ? 1 : 0) << FLAG_HOLE_BIT);

    public static bool IsNavigation(this uint control)
        => ((control >> FLAG_NAV_BIT) & 0x1) == 1;

    public static void SetNavigation(this ref uint control, bool navigation)
        => control = (control & ~((uint)0x1 << FLAG_NAV_BIT)) | (uint)((navigation ? 1 : 0) << FLAG_NAV_BIT);

    public static bool IsAutoshaded(this uint control)
        => ((control >> FLAG_AUTOSHADER_BIT) & 0x1) == 1;

    public static void SetAutoshaded(this ref uint control, bool autoShaded)
        => control = (control & ~((uint)0x1 << FLAG_AUTOSHADER_BIT)) | (uint)((autoShaded ? 1 : 0) << FLAG_AUTOSHADER_BIT);
}