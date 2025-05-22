using System.Reflection;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;
using System;
using System.Collections.Generic;

public class PlayLayer : ChunkBasedDataLayer<PlayLayer, PlayChunk, LayerService>
{
    private bool _subscribedToPlayLayerReady = false;
    private NodePath _playerNodePath = new();
    private NodePath _terrainNodePath = new();
    private Dictionary<string, object> _layerArguments = new();

    private LayerArgumentDictionary _layerArgumentsResource = new();

    private Dictionary<string, object> _layers = new()
    {
        { nameof(LandscapeLayerA), LandscapeLayerA.instance },
        { nameof(LandscapeLayerB), LandscapeLayerB.instance },
        { nameof(LandscapeLayerC), LandscapeLayerC.instance },
        { nameof(LandscapeLayerD), LandscapeLayerD.instance },
        { nameof(LSystemVillageLayer), LSystemVillageLayer.instance }
    };
    public override int chunkW => 8;
    public override int chunkH => 8;

    public override void LayerPostmount(LayerArgumentDictionary layerGlobalArgs)
    {
        PlayLayerBlackboard.PlayLayerReady = true;
        PlayLayerBlackboard.LayerArguments = layerGlobalArgs;
        SignalBus.Instance.CallDeferred(
            "emit_signal",
            SignalBus.SignalName.PlayLayerReady,
            layerGlobalArgs
        );
    }

    public PlayLayer(LayerArgumentDictionary layerGlobalArgs)
    {
        _layerArgumentsResource = layerGlobalArgs;
        GD.Print($"PlayLayer constructor called with arguments: {layerGlobalArgs}");
        InitializePlayLayer();
    }

    public PlayLayer()
    {
        GD.Print("PlayLayer constructor called with no arguments.");
        InitializePlayLayer();
    }

    private void InitializePlayLayer()
    {
        GD.Print("PlayLayer constructor called with no arguments.");
        Callable.From(HookSignalsDeferred).CallDeferred();

        var landscapeLayerD = _layers[nameof(LandscapeLayerD)] as LandscapeLayerD;
        // landscapeLayerD.SetLayerArguments(_layerArguments);

        AddLayerDependency(new LayerDependency(
            landscapeLayerD,
            2048,
            2048,
            landscapeLayerD.GetLevelCount() - 1,
            null
        ));

        var landscapeLayerC = _layers[nameof(LandscapeLayerC)] as LandscapeLayerC;
        // landscapeLayerC.SetLayerArguments(_layerArguments);

        AddLayerDependency(new LayerDependency(
            landscapeLayerC,
            1024,
            1024,
            landscapeLayerC.GetLevelCount() - 1,
            null
        ));

        var landscapeLayerB = _layers[nameof(LandscapeLayerB)] as LandscapeLayerB;
        // landscapeLayerB.SetLayerArguments(_layerArguments);

        AddLayerDependency(new LayerDependency(
            landscapeLayerB,
            512,
            512,
            landscapeLayerB.GetLevelCount() - 1,
            null
        ));

        var landscapeLayerA = _layers[nameof(LandscapeLayerA)] as LandscapeLayerA;
        // landscapeLayerA.SetLayerArguments(_layerArguments);

        AddLayerDependency(new LayerDependency(
            landscapeLayerA,
            256,
            256,
            landscapeLayerA.GetLevelCount() - 1,
            null
        ));

        var villageLayer = LSystemVillageLayer.instance;
        // villageLayer.SetLayerArguments(_layerArguments);

        AddLayerDependency(new LayerDependency(
            villageLayer,
            256,
            256,
            villageLayer.GetLevelCount() - 1,
            (bounds, level, levelData) =>
            {
                void Handler()
                {
                    villageLayer.EnsureLoadedInBounds(bounds, level, levelData);
                    GD.Print("LSystemVillageLayer dependency loaded after LandscapeChunksReady signal.");
                }

                SignalBus.Instance.LandscapeChunksReady += Handler;

                // Check if already ready, and manually trigger if so
                if (LandscapeChunkCounterBlackboard.LandscapeChunksAreReady)
                    Handler();
            }
        ));
    }

    private void HookSignalsDeferred()
    {
        if (_subscribedToPlayLayerReady) return;

        SignalBus.Instance.PlayLayerReady += OnPlayLayerReady;
        _subscribedToPlayLayerReady = true;
    }

    private void OnPlayLayerReady(LayerArgumentDictionary layerArguments)
    {
        var playLayerKey = nameof(PlayLayer);

        if (layerArguments.parameters
                 .TryGetValue(playLayerKey, out var layerParams))
        {
            if (layerParams.TryGetValue("PlayerPath", out var playerPathVariant))
            {
                _playerNodePath = new NodePath(playerPathVariant.ToString());
                _layerArguments["PlayerNodePath"] = playerPathVariant.ToString();
            }
            if (layerParams.TryGetValue("TerrainPath", out var terrainPathVariant))
            {
                _terrainNodePath = new NodePath(terrainPathVariant.ToString());
                _layerArguments["TerrainNodePath"] = terrainPathVariant.ToString();
                TerrainBlackboard.Initialize(terrainPathVariant.ToString());
            }
        }
        // foreach(var layer in _layers)
        // {
        //     var layerInstance = layer.Value as IChunkBasedDataLayer;
        //     // Debugging output
        //     // GD.Print($"Setting layer arguments for {layer.Key}");
        //     // foreach (var kvp in _layerArguments)
        //     // {
        //     //     GD.Print($"  {kvp.Key}: {kvp.Value}");
        //     // }
        //     // Note, this runs _after_ the layer is constructed,
        //     // so we will need a signal in order to use the arguments
        //     // in the relevant layer.
        //     //
        //     // Since one of these determines how much to construct,
        //     // we need to have a default LOD to use in the interim until a 
        //     // timer timeout runs.
        //     //
        //     // Feels a bit convoluted ... will think about this a bit more.
        //     layerInstance.SetLayerArguments(_layerArguments);
        // }
    }
}
