using System.Reflection;
using Godot;
using Runevision.LayerProcGen;

public class PlayChunk : LayerChunk<PlayLayer, PlayChunk>
{
    public PlayChunk()
    {
    }
    public override void Create(int level, bool destroy)
    {
        GD.Print($"{GetType().Name} ({bounds}) {MethodBase.GetCurrentMethod()}: {level}, {destroy}");
        base.Create(level, destroy);
    }
}

public class PlayLayer : ChunkBasedDataLayer<PlayLayer, PlayChunk>
{
    public override int chunkW => 8;
    public override int chunkH => 8;

    public PlayLayer()
    {
        AddLayerDependency(new LayerDependency(LandscapeLayerA.instance, 128, 128));
        AddLayerDependency(new LayerDependency(LandscapeLayerB.instance, 256, 256));
        AddLayerDependency(new LayerDependency(LandscapeLayerC.instance, 1024, 1024));
        AddLayerDependency(new LayerDependency(LandscapeLayerD.instance, 2048, 2048));
    }
}