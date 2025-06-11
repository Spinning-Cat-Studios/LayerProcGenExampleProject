using System.Reflection;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;
using System;
using Godot.Collections;

public class PlayLayer : ChunkBasedDataLayer<PlayLayer, PlayChunk, LayerService>, ILayerWithArguments
{
    public override int chunkW => 8;
    public override int chunkH => 8;

    public PlayLayer() { }

    public PlayLayer(LayerArgumentDictionary layerArguments)
    {
        GD.Print($"PlayLayer created with arguments: {layerArguments}");
        InitializePlayLayer(layerArguments);
    }

    private void ConstructLandscapeLayerDependency(
        LayerArgumentDictionary layerArguments,
        Type landscapeLayerType,
        int width,
        int height,
        string subtype)
    {
        var landscapeLayerArgs = layerArguments.Clone();
        // Add "landscape_layer_id": "A" through "landscape_layer_id": "D" to the layer arguments
        landscapeLayerArgs.parameters["landscape_layer_id"] = new Dictionary<string, Variant>
        {
            { "id", landscapeLayerType.Name }
        };

        // Find the static GetInstance method with the correct signature
        var getInstanceMethod = landscapeLayerType.GetMethod(
            "GetInstance",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new Type[] { typeof(LayerArgumentDictionary), typeof(string) },
            null
        );
        if (getInstanceMethod == null)
            throw new InvalidOperationException($"GetInstance(LayerArgumentDictionary, string) not found on {landscapeLayerType.Name}");

        // Call the static method
        var landscapeLayerInstance = (AbstractChunkBasedDataLayer)getInstanceMethod.Invoke(
            null,
            new object[] { landscapeLayerArgs, subtype }
        );

        AddLayerDependency(new LayerDependency(landscapeLayerInstance, width, height));
    }

    private void InitializePlayLayer(LayerArgumentDictionary layerArguments)
    {
        TerrainBlackboard.Initialize(new NodePath("Controller/TerrainLODManager/Terrain3D"));

        ConstructLandscapeLayerDependency(layerArguments, typeof(LandscapeLayerD), 2048, 2048, "D");
        ConstructLandscapeLayerDependency(layerArguments, typeof(LandscapeLayerC), 1024, 1024, "C");
        ConstructLandscapeLayerDependency(layerArguments, typeof(LandscapeLayerB), 512, 512, "B");
        ConstructLandscapeLayerDependency(layerArguments, typeof(LandscapeLayerA), 256, 256, "A");

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

                // Check if already ready, and manually trigger if so
                if (LandscapeChunkCounterBlackboard.LandscapeChunksAreReady)
                    Handler();
            }
        ));
        // GD.Print("PlayLayer Create");
    }
}
