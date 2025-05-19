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
    private readonly List<Node3D> _pendingHouses = new();

    public override void Create(
        int level,
        bool destroy,
        Action ready,
        Action done,
        LayerService service)
    {
        var villageService = service as VillageService
            ?? throw new InvalidCastException("Expected a VillageService");
        if (destroy)
            housePositions.Clear();
        else
            Build(ready, done, villageService);
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
        position.Y = GetHeightAt(position);

        var houseScene = GD.Load<PackedScene>(
            "res://src/scenes/l_system_prefabs/house.tscn");
        var inst = houseScene.Instantiate<Node3D>();
        inst.Position = position;

        _pendingHouses.Add(inst); 
    }

    void FlushHousesToScene()
    {
        // GD.Print("LSystemVillageChunk FlushHousesToScene");
        if (_pendingHouses.Count == 0) return;

        // Copy to Godot Array so it survives the lambda capture
        var batch = new Godot.Collections.Array<Node3D>(_pendingHouses.Cast<Node3D>());

        // Clear the list for the next build cycle
        _pendingHouses.Clear();

        // Schedule the whole batch to be added next frame
        SceneTree tree = (SceneTree)Engine.GetMainLoop();
        tree.CreateTimer(0)                           // 0-sec One-Shot Timer
            .Connect("timeout",
                Callable.From(() =>
                {
                    var parent = GetChunkParent();    // now definitely in tree
                    // GD.Print("Adding " + batch.Count + " houses to scene for parent " + parent.Name);
                    foreach (Node3D houseNode in batch)
                    {
                        houseNode.Name = "House_" + houseNode.Position.ToString();
                        parent.AddChild(houseNode);
                    }
                }));
    }
}
