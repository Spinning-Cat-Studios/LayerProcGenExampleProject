using System.Reflection;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;
using System;
using System.Collections.Generic;

public class PlayLayer : ChunkBasedDataLayer<PlayLayer, PlayChunk, LayerService>, ILayerWithArguments
{
    public override int chunkW => 8;
    public override int chunkH => 8;

    private static bool _isVillageLayerInitialized = false;

    public PlayLayer() { }

    public PlayLayer(LayerArgumentDictionary layerArguments)
    {
        GD.Print($"PlayLayer created with arguments: {layerArguments}");
        InitializePlayLayer(layerArguments);
    }

    private void InitializePlayLayer(LayerArgumentDictionary layerArguments)
    {
        TerrainBlackboard.Initialize(new NodePath("Controller/TerrainLODManager/Terrain3D"));

        var landscapeLayerA = LandscapeLayerA.GetInstance(layerArguments);
        var landscapeLayerB = LandscapeLayerB.GetInstance(layerArguments);
        var landscapeLayerC = LandscapeLayerC.GetInstance(layerArguments);
        var landscapeLayerD = LandscapeLayerD.GetInstance(layerArguments);

        AddLayerDependency(new LayerDependency(landscapeLayerD, 2048, 2048));
        AddLayerDependency(new LayerDependency(landscapeLayerC, 1024, 1024));
        AddLayerDependency(new LayerDependency(landscapeLayerB, 512, 512));
        AddLayerDependency(new LayerDependency(landscapeLayerA, 256, 256));

        var villageLayer = LSystemVillageLayer.GetInstance(layerArguments);

        AddLayerDependency(new LayerDependency(
            villageLayer,
            256,
            256,
            villageLayer.GetLevelCount() - 1,
            (bounds, level, levelData) =>
            {
                void Handler()
                {
                    _isVillageLayerInitialized = true;
                    villageLayer.EnsureLoadedInBounds(bounds, level, levelData);
                    GD.Print("LSystemVillageLayer dependency loaded after LandscapeChunksReady signal.");
                }

                SignalBus.Instance.LandscapeChunksReady += Handler;

                // Check if already ready, and manually trigger if so
                if (LandscapeChunkCounterBlackboard.LandscapeChunksAreReady && !_isVillageLayerInitialized)
                    Handler();
            }
        ));
        // GD.Print("PlayLayer Create");
    }
}
