using System.Reflection;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;
using System;

public class PlayLayer : ChunkBasedDataLayer<PlayLayer, PlayChunk, LayerService>
{
    public override int chunkW => 8;
    public override int chunkH => 8;
    
    private bool _subscribed = false;
    private GridBounds _pendingBounds;
    private int _pendingLevel;
    private ChunkLevelData _pendingLevelData;

    private void OnLandscapeChunksReady()
    {
        // This will be set by the dependency logic in ChunkBasedDataLayer
        if (_pendingBounds != null && _pendingLevelData != null)
        {
            LSystemVillageLayer.instance.EnsureLoadedInBounds(_pendingBounds, _pendingLevel, _pendingLevelData);
            GD.Print("LSystemVillageLayer dependency loaded after LandscapeChunksReady signal.");
        }
    }

    public PlayLayer()
    {
        TerrainBlackboard.Initialize(new NodePath("Controller/TerrainLODManager/Terrain3D"));

        AddLayerDependency(new LayerDependency(LandscapeLayerD.instance, 2048, 2048));
        AddLayerDependency(new LayerDependency(LandscapeLayerC.instance, 1024, 1024));
        AddLayerDependency(new LayerDependency(LandscapeLayerB.instance, 512, 512));
        AddLayerDependency(new LayerDependency(LandscapeLayerA.instance, 256, 256));

        AddLayerDependency(new LayerDependency(
            LSystemVillageLayer.instance,
            256,
            256,
            LSystemVillageLayer.instance.GetLevelCount() - 1,
            (bounds, level, levelData) => {
                SignalBus.Instance.LandscapeChunksReady += () => {
                    LSystemVillageLayer.instance.EnsureLoadedInBounds(bounds, level, levelData);
                    GD.Print("LSystemVillageLayer dependency loaded after LandscapeChunksReady signal.");
                };
            }
        ));
        // GD.Print("PlayLayer Create");
    }
}