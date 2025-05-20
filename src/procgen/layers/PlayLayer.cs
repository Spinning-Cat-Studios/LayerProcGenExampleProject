using System.Reflection;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;
using System;

public class PlayLayer : ChunkBasedDataLayer<PlayLayer, PlayChunk, LayerService>
{
    public override int chunkW => 8;
    public override int chunkH => 8;

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
                    void Handler() {
                        LSystemVillageLayer.instance.EnsureLoadedInBounds(bounds, level, levelData);
                        GD.Print("LSystemVillageLayer dependency loaded after LandscapeChunksReady signal.");
                    }

                    SignalBus.Instance.LandscapeChunksReady += Handler;

                    // Check if already ready, and manually trigger if so
                    if (LandscapeChunkCounterBlackboard.LandscapeChunksAreReady)
                        Handler();
                };
            }
        ));
        // GD.Print("PlayLayer Create");
    }
}