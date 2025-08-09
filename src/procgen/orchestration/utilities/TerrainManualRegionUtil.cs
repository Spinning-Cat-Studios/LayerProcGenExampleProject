using Godot;
using Terrain3DBindings;

namespace Terrain3D.Scripts.Utilities;

/// <summary>
/// Manual region creation fallback for Terrain3D builds lacking dynamic add_region.
/// Creates blank height/control/color images and appends them to storage arrays.
/// Use only if storage.AddRegion returns Error.Unavailable and no node-level add_region exists.
/// </summary>
public static class TerrainManualRegionUtil
{
    public static bool ManualEnsureRegion(Terrain3DStorage storage, Vector3 worldPos)
    {
        if (storage == null) return false;
        int rs = (int)storage.RegionSize;
        int ox = Mathf.FloorToInt(worldPos.X / rs) * rs;
        int oz = Mathf.FloorToInt(worldPos.Z / rs) * rs;
        // Already present?
        var offsets = storage.RegionOffsets;
        for (int i = 0; i < offsets.Count; i++)
        {
            var o = offsets[i];
            if (o.X == ox && o.Y == oz) return true;
        }

        var size = new Vector2I(rs, rs);
        Image MakeHeight()
        {
            // Use CreateEmpty (Create is obsolete in current Godot versions)
            var img = Image.CreateEmpty(size.X, size.Y, false, Image.Format.Rf);
            img.Fill(new Color(0, 0, 0, 1));
            return img;
        }
        Image MakeControl()
        {
            var img = Image.CreateEmpty(size.X, size.Y, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 0));
            return img;
        }
        Image MakeColor()
        {
            var img = Image.CreateEmpty(size.X, size.Y, false, Image.Format.Rgba8);
            img.Fill(new Color(0, 0, 0, 1));
            return img;
        }

        var height = MakeHeight();
        var control = MakeControl();
        var color = MakeColor();

        offsets.Add(new Vector2I(ox, oz));
        storage.RegionOffsets = offsets;

        var hms = storage.HeightMaps; hms.Add(height); storage.HeightMaps = hms;
        var cms = storage.ControlMaps; cms.Add(control); storage.ControlMaps = cms;
        var cols = storage.ColorMaps; cols.Add(color); storage.ColorMaps = cols;

        storage.ForceUpdateMaps();
        GD.Print($"[ManualRegion] Added region @ ({ox},{oz}) via manual fallback");
        return true;
    }
}
