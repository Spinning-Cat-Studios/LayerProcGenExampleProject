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

        AddLayerDependency(new LayerDependency(
            landscapeLayerD,
            2048,
            2048,
            landscapeLayerD.GetLevelCount() - 1,
            null
        ));

        var landscapeLayerC = _layers[nameof(LandscapeLayerC)] as LandscapeLayerC;

        AddLayerDependency(new LayerDependency(
            landscapeLayerC,
            1024,
            1024,
            landscapeLayerC.GetLevelCount() - 1,
            null
        ));

        var landscapeLayerB = _layers[nameof(LandscapeLayerB)] as LandscapeLayerB;

        AddLayerDependency(new LayerDependency(
            landscapeLayerB,
            512,
            512,
            landscapeLayerB.GetLevelCount() - 1,
            null
        ));

        var landscapeLayerA = _layers[nameof(LandscapeLayerA)] as LandscapeLayerA;

        AddLayerDependency(new LayerDependency(
            landscapeLayerA,
            256,
            256,
            landscapeLayerA.GetLevelCount() - 1,
            null
        ));

        var villageLayer = LSystemVillageLayer.instance;

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
        GD.Print("PlayLayer ready signal received.");
        // Leave this empty for now, the view is to use the overload constructor
        // in order to pass the layer arguments during construction,
        // rather than running this after the constructor.
        //
        // In this way we avoid too much in the way of spaghetti flow of control.
        //
        // Even though spaghetti is highly nutritious and a very popular dish,
        // it is probably not the best option for organising the flow of logic in
        // this instance.
        //
        // We can retain this for now, but likely we might want to alter the
        // arguments passed to it in future, maybe YAGNI, but maybe not.
        //
        // Will see!
    }
}
