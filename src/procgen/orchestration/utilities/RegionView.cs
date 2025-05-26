using System.Collections.Generic;
using Godot;
using Terrain3D.Scripts.Generation.Layers;
using TokisanGames;

namespace Terrain3D.Scripts.Utilities;

public class RegionView
{
    public readonly int regionIndex;
    private static readonly Dictionary<Vector2I, int> LoDs = new Dictionary<Vector2I, int>();

    public RegionView(int regionIndex)
    {
        this.regionIndex = regionIndex;
    }

    public int LoD
    {
        get => LoDs.GetValueOrDefault(RegionOffset, int.MaxValue);
        set => LoDs.TryAdd(RegionOffset, value);
    }

    public Image? HeightMap
    {
        get => TerrainLODManager.instance.terrain3D.Storage.GetMaps(Terrain3DStorage.MapType.Height)[regionIndex].As<Image>();
        set
        {
            var maps = TerrainLODManager.instance.terrain3D.Storage.GetMaps(Terrain3DStorage.MapType.Height).Duplicate();
            maps[regionIndex] = value;
            TerrainLODManager.instance.terrain3D.Storage.SetMaps(Terrain3DStorage.MapType.Height, maps);
        }
    }

    public Image? ControlMap
    {
        get => TerrainLODManager.instance.terrain3D.Storage.GetMaps(Terrain3DStorage.MapType.Control)[regionIndex].As<Image>();
        set
        {
            var maps = TerrainLODManager.instance.terrain3D.Storage.GetMaps(Terrain3DStorage.MapType.Control).Duplicate();
            maps[regionIndex] = value;
            TerrainLODManager.instance.terrain3D.Storage.SetMaps(Terrain3DStorage.MapType.Control, maps);
        }
    }

    public Image? ColorMap
    {
        get => TerrainLODManager.instance.terrain3D.Storage.GetMaps(Terrain3DStorage.MapType.Color)[regionIndex].As<Image>();
        set
        {
            var maps = TerrainLODManager.instance.terrain3D.Storage.GetMaps(Terrain3DStorage.MapType.Color).Duplicate();
            maps[regionIndex] = value;
            TerrainLODManager.instance.terrain3D.Storage.SetMaps(Terrain3DStorage.MapType.Color, maps);
        }
    }

    public Image? MaxMap
    {
        get => TerrainLODManager.instance.terrain3D.Storage.GetMaps(Terrain3DStorage.MapType.Max)[regionIndex].As<Image>();
        set
        {
            var maps = TerrainLODManager.instance.terrain3D.Storage.GetMaps(Terrain3DStorage.MapType.Max).Duplicate();
            maps[regionIndex] = value;
            TerrainLODManager.instance.terrain3D.Storage.SetMaps(Terrain3DStorage.MapType.Max, maps);
        }
    }

    public Vector2I RegionOffset
    {
        get => (Vector2I)TerrainLODManager.instance.terrain3D.Storage.RegionOffsets[regionIndex];
        set => TerrainLODManager.instance.terrain3D.Storage.RegionOffsets[regionIndex] = value;
    }

    public static RegionView Create(int regionIndex)
    {
        return new RegionView(regionIndex);
    }
}