using Runevision.Common;
using Runevision.LayerProcGen;
using Godot;
using System.Collections.Generic;
using System.Linq;
using Godot.Util;
using System;
using LayerProcGenExampleProject.Services;

public class LSystemVillageChunk : LayerChunk<LSystemVillageLayer, LSystemVillageChunk, VillageService>
{
    List<Vector3> housePositions = new();
    private Node3D? _chunkParent; 
    private readonly List<Vector3> _pendingHousePositions = new();

    public override void Create(
        int level,
        bool destroy,
        Action ready,
        Action done,
        LayerService service)
    {
        if (destroy)
        {
            housePositions.Clear();

            // Get the service from the layer to clear the DB records for this chunk.
            var villageService = this.layer.GetService() as VillageService;
            villageService?.ClearPersistedRoadChunk(this.index);
        }
        else
        {
            var villageService = service as VillageService
                ?? throw new InvalidCastException("Expected a VillageService");
            Build(ready, done, villageService);
        }
    }

    public void DebugDraw() {
		GD.Print("LSystemVillageChunk DebugDraw");
	}

    Node3D GetChunkParent()
    {
        // Re-use it if we already made it
        if (_chunkParent != null && GodotObject.IsInstanceValid(_chunkParent))
            return _chunkParent;

        // ❶ Get the SceneTree – works from non-Node classes too
        var tree = (SceneTree)Engine.GetMainLoop();

        // ❷ Find the node you placed in the editor
        //     RootNode3D/Layers/LSystemVillage
        var villageRoot = tree.Root
            .GetNode<Node3D>("RootNode3D/Layers/LSystemVillage");

        // ❸ Make a node for *this* chunk the first time we need it
        _chunkParent = new Node3D
        {
            Name = $"Chunk_{index.x}_{index.y}"
        };
        villageRoot.AddChild(_chunkParent);

        return _chunkParent;
    }

    public override void Initialize(LSystemVillageLayer layerInstance, Point index)
    {
        base.Initialize(layerInstance, index);
    }

    void Build(Action ready, Action done, VillageService villageService)
    {
        ready?.Invoke();
        // 1. Generate all data in one call
        LSystemResult result = villageService.GenerateVillageData(index, layer);

        // 2. Houses → scene
        foreach (var pos in result.HousePositions)
            QueueHouseInstance(pos);
        FlushHousesToScene();

        // 3. Paint / signal roads
        var roadPos  = result.RoadPositionDirections.Select(p => p.Item1).ToArray();
        var roadDirs = result.RoadPositionDirections.Select(p => p.Item2).ToArray();

        SignalBus.Instance.CallDeferred(
            "emit_signal",
            SignalBus.SignalName.RoadsGenerated,
            roadPos,
            roadDirs,
            result.RoadStartIndices.ToArray(),
            result.RoadEndIndices.ToArray(),
            index.ToVector3()
        );

        // GD.Print($"Chunk {index.x}, {index.y} generated with {result.HousePositions.Count} houses and {result.RoadEndPositions.Count} road ends.");
        // convert List<Vector3> to String
        // GD.Print($"Road end positions: {roadEndPositionsString}");
        // 4. Persist to DB
        villageService.PersistRoadChunk(index, result.RoadEndPositions);

        done?.Invoke();
    }

    float GetHeightAt(Vector3 position)
    {
        var coords2D = new Vector2(position.X, position.Z);
        return TerrainNoise.GetHeight(coords2D);
    }

    void QueueHouseInstance(Vector3 position)
    {
        // This runs on a background thread.
        // Only perform thread-safe calculations here.
        position.Y = GetHeightAt(position);
        // Do not call GD.Load or Instantiate(). Just store the data.
        _pendingHousePositions.Add(position);
    }

    void FlushHousesToScene()
    {
        if (_pendingHousePositions.Count == 0) return;

        // Copy the positions to a Godot Array so it survives the lambda capture.
        var batch = new Godot.Collections.Array<Vector3>(_pendingHousePositions);
        // Clear the list for the next build cycle.
        _pendingHousePositions.Clear();

        // Schedule the Godot-related work to run on the main thread.
        SceneTree tree = (SceneTree)Engine.GetMainLoop();
        tree.CreateTimer(0)                           // 0-sec One-Shot Timer
            .Connect("timeout",
                Callable.From(() =>
                {
                    // This code now runs safely on the main thread.
                    var parent = GetChunkParent();
                    // Load the scene once, outside the loop.
                    var houseScene = GD.Load<PackedScene>("res://src/scenes/l_system_prefabs/house.tscn");

                    foreach (Vector3 position in batch)
                    {
                        var inst = houseScene.Instantiate<Node3D>();
                        inst.Position = position;
                        inst.Name = "House_" + inst.Position.ToString();
                        parent.AddChild(inst);
                    }
                }));
    }
}
