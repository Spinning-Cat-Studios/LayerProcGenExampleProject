using Godot;
using Terrain3DBindings;
using TerrainSample.Scripts.Generation.Layers;

namespace LayerProcGen.Samples.LayerProcGen._0._1._0.TerrainSample.Scripts.Utilities;

public class Terrain3DRegion
{
    private readonly int regionIndex;

    public Terrain3DRegion(int regionIndex)
    {
        this.regionIndex = regionIndex;
    }

    public Image HeightMap
    {
        get => TerrainLODManager.instance.terrain3D.Storage.GetMapRegion(MapType.TYPE_HEIGHT, regionIndex);
        set => TerrainLODManager.instance.terrain3D.Storage.SetMapRegion(MapType.TYPE_HEIGHT, regionIndex, value);
    }

    public Image ControlMap
    {
        get => TerrainLODManager.instance.terrain3D.Storage.GetMapRegion(MapType.TYPE_CONTROL, regionIndex);
        set => TerrainLODManager.instance.terrain3D.Storage.SetMapRegion(MapType.TYPE_CONTROL, regionIndex, value);
    }

    public Image ColorMap
    {
        get => TerrainLODManager.instance.terrain3D.Storage.GetMapRegion(MapType.TYPE_COLOR, regionIndex);
        set => TerrainLODManager.instance.terrain3D.Storage.SetMapRegion(MapType.TYPE_COLOR, regionIndex, value);
    }

    public Image MaxMap
    {
        get => TerrainLODManager.instance.terrain3D.Storage.GetMapRegion(MapType.TYPE_MAX, regionIndex);
        set => TerrainLODManager.instance.terrain3D.Storage.SetMapRegion(MapType.TYPE_MAX, regionIndex, value);
    }
}