using System.Collections.Generic;
using Godot;
using Terrain3D.Scripts.Generation.Layers;
using Terrain3DBindings;

namespace Terrain3D.Scripts.Utilities;

public class Terrain3DRegion
{
    public readonly int regionIndex;
    private static readonly Dictionary<Vector2I, int> LoDs = new Dictionary<Vector2I, int>();

    public Terrain3DRegion(int regionIndex)
    {
        this.regionIndex = regionIndex;
    }

    public int LoD
    {
        get => LoDs.GetValueOrDefault(RegionOffset, int.MaxValue);
        set => LoDs.TryAdd(RegionOffset, value);
    }

    public Image HeightMap
    {
        get => TerrainLODManager.instance.terrain3DWrapper.Storage.GetMapRegion(MapType.TYPE_HEIGHT, regionIndex);
        set
        {
            var storage = TerrainLODManager.instance.terrain3DWrapper.Storage;
            if (value == null) return;
            if (storage.Instance.HasMethod("set_map_region"))
            {
                storage.SetMapRegion(MapType.TYPE_HEIGHT, regionIndex, value);
            }
            else
            {
                var arr = storage.HeightMaps; if (regionIndex >= 0 && regionIndex < arr.Count) { arr[regionIndex] = value; storage.HeightMaps = arr; }
            }
        }
    }

    public Image ControlMap
    {
        get => TerrainLODManager.instance.terrain3DWrapper.Storage.GetMapRegion(MapType.TYPE_CONTROL, regionIndex);
        set
        {
            var storage = TerrainLODManager.instance.terrain3DWrapper.Storage;
            if (value == null) return;
            if (storage.Instance.HasMethod("set_map_region"))
            {
                storage.SetMapRegion(MapType.TYPE_CONTROL, regionIndex, value);
            }
            else
            {
                var arr = storage.ControlMaps; if (regionIndex >= 0 && regionIndex < arr.Count) { arr[regionIndex] = value; storage.ControlMaps = arr; }
            }
        }
    }

    public Image ColorMap
    {
        get => TerrainLODManager.instance.terrain3DWrapper.Storage.GetMapRegion(MapType.TYPE_COLOR, regionIndex);
        set
        {
            var storage = TerrainLODManager.instance.terrain3DWrapper.Storage;
            if (value == null) return;
            if (storage.Instance.HasMethod("set_map_region"))
            {
                storage.SetMapRegion(MapType.TYPE_COLOR, regionIndex, value);
            }
            else
            {
                var arr = storage.ColorMaps; if (regionIndex >= 0 && regionIndex < arr.Count) { arr[regionIndex] = value; storage.ColorMaps = arr; }
            }
        }
    }

    public Image MaxMap
    {
        get => TerrainLODManager.instance.terrain3DWrapper.Storage.GetMapRegion(MapType.TYPE_MAX, regionIndex);
        set
        {
            var storage = TerrainLODManager.instance.terrain3DWrapper.Storage;
            if (value == null) return;
            if (storage.Instance.HasMethod("set_map_region"))
            {
                storage.SetMapRegion(MapType.TYPE_MAX, regionIndex, value);
            }
            else
            {
                GD.PushWarning("MaxMap setter fallback: TYPE_MAX not directly supported without set_map_region");
            }
        }
    }

    public Vector2I RegionOffset
    {
        get => TerrainLODManager.instance.terrain3DWrapper.Storage.RegionOffsets[regionIndex];
        set => TerrainLODManager.instance.terrain3DWrapper.Storage.RegionOffsets[regionIndex] = value;
    }

    public static Terrain3DRegion Create(int regionIndex)
    {
        return new Terrain3DRegion(regionIndex);
    }
}