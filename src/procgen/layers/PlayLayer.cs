using System.Reflection;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;
using System;
using Godot.Collections;
using LayerProcGenExampleProject.ProcGen.Layers.PlayLayerComponents;

public class PlayLayer : ChunkBasedDataLayer<PlayLayer, PlayChunk, LayerService>, ILayerWithArguments
{
    public override int chunkW => PlayLayerConfiguration.CHUNK_WIDTH;
    public override int chunkH => PlayLayerConfiguration.CHUNK_HEIGHT;

    private readonly PlayerPositionManager _playerManager;
    private readonly LandscapeLayerOrchestrator _landscapeOrchestrator;
    private readonly LazyEvaluationHandler _lazyEvaluationHandler;

    public PlayLayer()
    {
        _playerManager = new PlayerPositionManager();
        _landscapeOrchestrator = new LandscapeLayerOrchestrator(this, _playerManager);
        _lazyEvaluationHandler = new LazyEvaluationHandler(this, _playerManager, _landscapeOrchestrator);
    }

    public PlayLayer(LayerArgumentDictionary layerArguments) : this()
    {
        GD.Print($"PlayLayer created with arguments: {layerArguments}");
        InitializePlayLayer(layerArguments);
    }

    private void InitializePlayLayer(LayerArgumentDictionary layerArguments)
    {
        TerrainBlackboard.Initialize(new NodePath("Controller/TerrainLODManager/Terrain3D"));

        bool hasPlayerPosition = _playerManager.TryInitializeFromArguments(layerArguments);

        if (hasPlayerPosition)
        {
            GD.Print($"[PlayLayer] Initial player position: {_playerManager.LastKnownPlayerPosition}");
            _landscapeOrchestrator.SetupLandscapeLayersWithLazyLoading(layerArguments, _playerManager.LastKnownPlayerPosition);
        }
        else
        {
            GD.Print("[PlayLayer] No player position found, using default loading");
            _landscapeOrchestrator.SetupLandscapeLayersDefault(layerArguments);
        }

        SetupVillageLayer(layerArguments);
        Callable.From(_lazyEvaluationHandler.HookSignalsDeferred).CallDeferred();
    }

    private void SetupVillageLayer(LayerArgumentDictionary layerArguments)
    {
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
                    if (!LandscapeChunkCounterBlackboard.LandscapeChunksAreReady)
                    {
                        return;
                    }
                    villageLayer.EnsureLoadedInBounds(bounds, level, levelData);
                    GD.Print("LSystemVillageLayer dependency loaded after LandscapeChunksReady signal.");
                }

                SignalBus.Instance.LandscapeChunksReady += Handler;

                if (LandscapeChunkCounterBlackboard.LandscapeChunksAreReady)
                    Handler();
            }
        ));
    }
}
