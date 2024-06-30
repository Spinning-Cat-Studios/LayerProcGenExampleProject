using System.Reflection;
using Godot;
using Runevision.Common;
using Runevision.LayerProcGen;
using Terrain3DBindings;
using TerrainSample.Scripts.Generation.Layers;

public struct QueuedTerrainCallback<L, C> : IQueuedAction
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public float[,] heightmap;
    public int[,,] splatmap;
    public int[,] detailMap;
    public MeshInstance3D[] treeInstances; //there is no TreeInstance in Godot, but we can use Meshinstance, should be as powerful
    public Vector3 position;
    public TransformWrapper chunkParent;
    public L layer;
    public Point index;

    public QueuedTerrainCallback(
        float[,] heightmap,
        int[,,] splatmap,
        int[,] detailMap,
        MeshInstance3D[] treeInstances,
        TransformWrapper chunkParent,
        Vector3 position,
        L layer,
        Point index
    )
    {
        this.heightmap = heightmap;
        this.splatmap = splatmap;
        this.detailMap = detailMap;
        this.treeInstances = treeInstances;
        this.chunkParent = chunkParent;
        this.position = position;
        this.layer = layer;
        this.index = index;
    }

    public void Process()
    {
        throw new System.NotImplementedException();
    }

    public void ProcessRoutine()
    {
        // if (!TerrainLODManager.instance.HasChunkFor(index, layer.lodLevel))
        // {
        //     //TODO: generate this one
        //     //TODO TODO: mechanic to increase LOD precision, we don't need to reduce it, as that will be done by Terrain3D itself.
        //     TerrainLODManager.instance.RegisterChunk(layer.lodLevel, index, terrain);
        // }
    }
}

public struct QueuedTerrainRecycleCallback<L, C> : IQueuedAction
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public void Process()
    {
        throw new System.NotImplementedException();
    }
}

public abstract class LandscapeChunk<L, C> : LayerChunk<L, C>, IGodotInstance
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    
    
    public override void Create(int level, bool destroy)
    {
        GD.Print($"{GetType().Name} ({bounds}) {MethodBase.GetCurrentMethod()}: {level}, {destroy}");
        base.Create(level, destroy);
    }

    public Node? LayerRoot()
    {
        throw new System.NotImplementedException();
    }
}

public abstract class LandscapeLayer<L, C> : ChunkBasedDataLayer<L, C>, IGodotInstance
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public abstract int lodLevel { get; }

    public Terrain3D? layerParent;


    public Image GetTerrainHeight(Vector3 worldPos)
    {
        return layerParent != null &&
               layerParent.Storage.HasRegion(worldPos)
            ? layerParent.Storage.GetMapRegion(MapType.TYPE_HEIGHT, layerParent.Storage.GetRegionIndex(worldPos))
            : new Image();
    }

    public Image GetTerrainControl(Vector3 worldPos)
    {
        return layerParent != null &&
               layerParent.Storage.HasRegion(worldPos)
            ? layerParent.Storage.GetMapRegion(MapType.TYPE_CONTROL, layerParent.Storage.GetRegionIndex(worldPos))
            : new Image();
    }

    public Image GetTerrainColor(Vector3 worldPos)
    {
        return layerParent != null &&
               layerParent.Storage.HasRegion(worldPos)
            ? layerParent.Storage.GetMapRegion(MapType.TYPE_COLOR, layerParent.Storage.GetRegionIndex(worldPos))
            : new Image();
    }

    public Node LayerRoot() => layerParent?.Instance as Node3D;
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