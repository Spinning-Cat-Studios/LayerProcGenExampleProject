using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Godot;
using Godot.Collections;
using Godot.Util;
using Runevision.Common;
using Runevision.LayerProcGen;
using Terrain3DBindings;
using TerrainSample.Scripts.Generation.Layers;
using TerrainSample.Scripts.Utilities;

public struct MapQueuedTerrainCallback<L, C> : IQueuedAction
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public float[,] heightmap;
    public int[,,] splatmap;
    public int[,] detailMap;
    public MeshInstance3D[] treeInstances; //there is no TreeInstance in Godot, but we can use Meshinstance, should be as powerful
    public L layer;
    public Point index;
    private readonly int regionSize;

    public MapQueuedTerrainCallback(
        float[,] heightmap,
        int[,,] splatmap,
        int[,] detailMap,
        MeshInstance3D[] treeInstances,
        L layer,
        Point index
    )
    {
        this.heightmap = heightmap;
        this.splatmap = splatmap;
        this.detailMap = detailMap;
        this.treeInstances = treeInstances;
        this.layer = layer;
        this.index = index;
        regionSize = (int)RegionSize.SIZE_1024;
    }

    static Terrain3DRegion? GetOrCreateTerrain(Vector3 position, L layer)
    {
        if (!TerrainLODManager.instance.HasChunkAt(position))
            TerrainLODManager.instance.CreateNewChunkAt(position);
        var chunk = TerrainLODManager.instance.GetChunkAt(position);
        return chunk.LoD < layer.lodLevel ? null : chunk;
    }

    public void Process()
    {
        LayerManagerBehavior.instance.StartCoroutine(ProcessRoutine());
    }

    public IEnumerator ProcessRoutine()
    {
        var startPos = index * layer.chunkW;
        Terrain3DRegion? terrain = GetOrCreateTerrain(new Vector3(startPos.x, 0, startPos.y), layer);
        if (terrain == null)
            yield break;

        terrain.HeightMap ??= Image.Create(regionSize, regionSize, false, Image.Format.Rf);
        DPoint cellSize = (DPoint)layer.chunkSize / layer.gridResolution;
        float minHeight = layer.terrainBaseHeight;
        float totalHeight = layer.terrainHeight - layer.terrainBaseHeight;
        // TerrainLODManager.instance.terrain3D.Storage.HeightRange = new Vector2(minHeight, layer.terrainHeight);

        GD.Print($"HandleUnderSizedRegions: {cellSize}, {layer.chunkSize}, {layer.gridResolution}");
        // GD.Print($"\t: {layer.lodLevel} {position} in region:{terrain.RegionOffset}, {startPos}; on index:{index}, {index * layer.chunkW}");
        for (var x = 0; x < layer.chunkSize.x; x++)
        {
            for (var z = 0; z < layer.chunkSize.y; z++)
            {
                Vector3 globalPosition = new Vector3(startPos.x + x, 0, startPos.y + z);
                TerrainLODManager.instance.terrain3D.Storage.SetHeight(globalPosition, heightmap[(int)(z / cellSize.y), (int)(x / cellSize.x)]);
            }
        }

        TerrainLODManager.instance.terrain3D.Storage.ForceUpdateMaps(MapType.TYPE_HEIGHT);
        yield return null;
    }
}

public struct ImgQueuedTerrainCallback<L, C> : IQueuedAction
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public Image heightmap;
    public int[,,] splatmap;
    public int[,] detailMap;
    public MeshInstance3D[] treeInstances; //there is no TreeInstance in Godot, but we can use Meshinstance, should be as powerful
    public L layer;
    public Point index;
    private Point startPos;

    public ImgQueuedTerrainCallback(
        Image heightmap,
        int[,,] splatmap,
        int[,] detailMap,
        MeshInstance3D[] treeInstances,
        L layer,
        Point startPos,
        Point index
    )
    {
        this.heightmap = heightmap;
        this.splatmap = splatmap;
        this.detailMap = detailMap;
        this.treeInstances = treeInstances;
        this.layer = layer;
        this.startPos = startPos;
        this.index = index;
    }

    static Terrain3DRegion? GetOrCreateTerrain(Vector3 position, L layer)
    {
        if (!TerrainLODManager.instance.HasChunkAt(position))
            TerrainLODManager.instance.CreateNewChunkAt(position);
        var chunk = TerrainLODManager.instance.GetChunkAt(position);
        return chunk.LoD < layer.lodLevel ? null : chunk;
    }

    public void Process()
    {
        LayerManagerBehavior.instance.StartCoroutine(ProcessRoutine());
    }

    public IEnumerator ProcessRoutine()
    {
        Terrain3DRegion? terrain = GetOrCreateTerrain(new Vector3(startPos.x, 0, startPos.y), layer);
        if (terrain == null)
            yield break;
        terrain.HeightMap = heightmap;
        TerrainLODManager.instance.terrain3D.Storage.ForceUpdateMaps(MapType.TYPE_HEIGHT);
        yield return null;
    }
}

public struct QueuedTerrainRecycleCallback<L, C> : IQueuedAction
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public void Process()
    {
        throw new NotImplementedException();
    }
}

public abstract class LandscapeChunk<L, C> : LayerChunk<L, C>
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    protected float[,] heights;
    public Vector3[,] dists;
    public Vector4[,] splats;

    const int GridOffset = 4;

    protected LandscapeChunk()
    {
        heights = new float[layer.gridResolution, layer.gridResolution];
    }

    public override void Create(int level, bool destroy)
    {
        if (destroy)
        {
            heights.Clear();
        }
        else
        {
            Build();
        }

        GD.Print($"{GetType().Name} ({bounds}) {MethodBase.GetCurrentMethod()}: {level}, {destroy}");
        base.Create(level, destroy);
    }

    private void Build()
    {
        SimpleProfiler.ProfilerHandle ph;

        DPoint cellSize = (DPoint)layer.chunkSize / layer.chunkResolution;
        DPoint terrainOrigin = index * layer.chunkSize - cellSize * GridOffset;

        ph = SimpleProfiler.Begin(phc, "Height Noise");
        float height = layer.terrainBaseHeight;
        HeightNoise(terrainOrigin, cellSize, layer.gridResolution, ref heights, ref dists, layer.terrainHeight, height);
        SimpleProfiler.End(ph);

        IQueuedAction action;
        if (layer.chunkW < (int)RegionSize.SIZE_1024)
        {
            action = new MapQueuedTerrainCallback<L, C>(
                heights, null, null, null,
                layer, index
            );
            MainThreadActionQueue.Enqueue(action);
        }
        else
        {
            var regionSize = (int)RegionSize.SIZE_1024;

            var subRegionSize = new Point(layer.chunkW / regionSize, layer.chunkH / regionSize);

            for (int i = 0; i < subRegionSize.x; i++)
            for (int j = 0; j < subRegionSize.y; j++)
            {
                var subIndex = new Point(i, j);
                var baseStartPos = index * layer.chunkW;
                var subStartPos = subIndex * regionSize;
                var startPos = baseStartPos + subStartPos;
                GD.Print($"HandleOverSizedRegions: {cellSize}, {subIndex} {layer.chunkSize}, {startPos}");


                var img = Image.Create(regionSize, regionSize, false, Image.Format.Rf);

                CopyHeights(subIndex, subRegionSize, regionSize, layer, ref heights, ref img);

                // CopyHeights(layer.gridResolution, layer.terrainBaseHeight, layer.terrainHeight, heights.AsReadOnlySpan(), ref img);

                action = new ImgQueuedTerrainCallback<L, C>(
                    img, null, null, null,
                    layer, startPos, index
                );
                MainThreadActionQueue.Enqueue(action);
            }
        }
    }

    private static void CopyHeights(Point subIndex, Point subRegionSize, int regionSize, L layer, ref float[,] heights, ref Image img)
    {
        var offW = subIndex.x * heights.GetLength(0) / subRegionSize.x;
        var offD = subIndex.y * heights.GetLength(1) / subRegionSize.y;
        var difW = layer.chunkW / heights.GetLength(0);
        var difD = layer.chunkH / heights.GetLength(1);
        for (var x = 0; x < regionSize; x++)
        {
            for (var z = 0; z < regionSize; z++)
            {
                img.SetPixel(x, z, Colors.Red * (heights[z / difW + offD, x / difD + offW]));
            }
        }
    }

    static void HeightNoise(in DPoint terrainOrigin, in DPoint cellSize, int gridResolution,
        ref float[,] heights, ref Vector3[,] dists, float terrainHeight, float terrainBaseHeight)
    {
        var inverseTerrainHeight = 1f / terrainHeight;

        for (var zRes = 0; zRes < gridResolution; zRes++)
        {
            for (var xRes = 0; xRes < gridResolution; xRes++)
            {
                var p = (Vector2)(terrainOrigin + new Point(xRes, zRes) * cellSize);
                heights[zRes, xRes] = TerrainNoise.GetHeight(p);
                // dists[zRes, xRes] = new Vector3(0f, 0f, 1000f);
            }
        }
    }

    static void HandleEdges(int fromEdge, float lowerDist, ref float[,] heights)
    {
        for (int i = fromEdge; i < heights.GetLength(0) - fromEdge; i++)
        {
            heights[fromEdge, i] -= lowerDist;
            heights[i, fromEdge] -= lowerDist;
            heights[heights.GetLength(0) - fromEdge - 1, i] -= lowerDist;
            heights[i, heights.GetLength(0) - fromEdge - 1] -= lowerDist;
        }
    }
}

public abstract class LandscapeLayer<L, C> : ChunkBasedDataLayer<L, C>
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public abstract int lodLevel { get; }

    public const int GridResolution = 256;
    public int gridResolution = GridResolution;
    public int chunkResolution = GridResolution - 8;
    public float terrainBaseHeight = -100;
    public float terrainHeight = 200;

    protected LandscapeLayer(int rollingGridWidth = 32, int rollingGridHeight = 0, int rollingGridMaxOverlap = 3) : base(rollingGridWidth, rollingGridHeight, rollingGridMaxOverlap)
    {
        TerrainNoise.SetFullTerrainHeight(new Vector2(terrainBaseHeight, terrainHeight));
    }

    // public Image GetTerrainHeight(Vector3 worldPos)
    // {
    //     return layerParent != null &&
    //            layerParent.Storage.HasRegion(worldPos)
    //         ? layerParent.Storage.GetMapRegion(MapType.TYPE_HEIGHT, layerParent.Storage.GetRegionIndex(worldPos))
    //         : new Image();
    // }
    //
    // public Image GetTerrainControl(Vector3 worldPos)
    // {
    //     return layerParent != null &&
    //            layerParent.Storage.HasRegion(worldPos)
    //         ? layerParent.Storage.GetMapRegion(MapType.TYPE_CONTROL, layerParent.Storage.GetRegionIndex(worldPos))
    //         : new Image();
    // }
    //
    // public Image GetTerrainColor(Vector3 worldPos)
    // {
    //     return layerParent != null &&
    //            layerParent.Storage.HasRegion(worldPos)
    //         ? layerParent.Storage.GetMapRegion(MapType.TYPE_COLOR, layerParent.Storage.GetRegionIndex(worldPos))
    //         : new Image();
    // }
}

//@formatter:off
public class LandscapeLayerA : LandscapeLayer<LandscapeLayerA, LandscapeChunkA> {
    public override int lodLevel => 0;
    public override int chunkW => (int)RegionSize.SIZE_1024/4;
    public override int chunkH => (int)RegionSize.SIZE_1024/4;
}

public class LandscapeLayerB : LandscapeLayer<LandscapeLayerB, LandscapeChunkB> {
    public override int lodLevel => 1;
    public override int chunkW => (int)RegionSize.SIZE_1024/2;
    public override int chunkH => (int)RegionSize.SIZE_1024/2;
}

public class LandscapeLayerC : LandscapeLayer<LandscapeLayerC, LandscapeChunkC> {
    public override int lodLevel => 2;
    public override int chunkW => (int)RegionSize.SIZE_1024;
    public override int chunkH => (int)RegionSize.SIZE_1024;
}

public class LandscapeLayerD : LandscapeLayer<LandscapeLayerD, LandscapeChunkD> {
    public override int lodLevel => 3;
    public override int chunkW => (int)RegionSize.SIZE_1024*2;
    public override int chunkH => (int)RegionSize.SIZE_1024*2;
}

public class LandscapeChunkA : LandscapeChunk<LandscapeLayerA, LandscapeChunkA> { }
public class LandscapeChunkB : LandscapeChunk<LandscapeLayerB, LandscapeChunkB> { }
public class LandscapeChunkC : LandscapeChunk<LandscapeLayerC, LandscapeChunkC> { }
public class LandscapeChunkD : LandscapeChunk<LandscapeLayerD, LandscapeChunkD> { }

//@formatter:on