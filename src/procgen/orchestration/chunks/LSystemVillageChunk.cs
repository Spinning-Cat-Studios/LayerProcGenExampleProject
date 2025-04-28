using Runevision.Common;
using Runevision.LayerProcGen;
using Godot;
using System.Collections.Generic;
using System.Linq;

public class LSystemVillageChunk : LayerChunk<LSystemVillageLayer, LSystemVillageChunk>
{
    List<Vector3> housePositions = new();
    Point gridOrigin;

    const int GLOBAL_SEED = 12345; // TODO: make this configurable and/or random but stored in the database when finally hook this up to a backend.
    const int CHUNK_X_RANDOM = 73856093;
    const int CHUNK_Y_RANDOM = 19349663;
    const int LSYSTEM_ITERATIONS = 3;

    public override void Create(int level, bool destroy)
    {
        GD.Print("LSystemVillageChunk Create");
        if (destroy)
            housePositions.Clear();
        else
            Build();
    }

    public void DebugDraw() {
		GD.Print("LSystemVillageChunk DebugDraw");
	}

    void Build()
    {
        gridOrigin = index * layer.chunkW;

        // Generate your L-system string
        var rules = new Dictionary<char, string>
        {
            {'A', "ADA"},
            {'B', "D[B]D[B]"},
            {'C', "ACA"},
            {'D', "DA"}
        };

        // Mix in chunk coords so each chunk seed varies
        int chunkSeed = GLOBAL_SEED
                    + index.x * CHUNK_X_RANDOM
                    + index.y * CHUNK_Y_RANDOM;
        
        var rnd = new System.Random(chunkSeed);
        var alphabet = rules.Keys.ToArray();

        // Build a 3‑char axiom
        string axiom = string.Concat(
            Enumerable.Range(0, 3)
                    .Select(_ => alphabet[rnd.Next(alphabet.Length)])
        );

        // Generate the L-system sequence
        var lSystem = new LSystem(axiom, rules);
        string lSequence = lSystem.Generate(LSYSTEM_ITERATIONS);

        GD.Print("L-System Sequence: " + lSequence);

        // Start interpreting at the chunk's origin
        var interpreter = new TurtleInterpreter(GetHeightAt);
        interpreter.Interpret(lSequence, gridOrigin.ToVector3(), Vector3.Forward, housePositions);

        GD.Print("House positions: " + string.Join(", ", housePositions));

        // // Now housePositions has the exact positions for houses in this chunk
        // foreach (var pos in housePositions)
        // {
        //     PlaceHouseInstance(pos);
        // }
    }

    float GetHeightAt(Vector3 position)
    {
        return GeoGridLayer.instance.SampleHeightAt(position.X, position.Z);
    }

    // void PlaceHouseInstance(Vector3 position)
    // {
    //     position.Y = GetHeightAt(position);
    //     var houseScene = GD.Load<PackedScene>("res://src/scenes/l_system_prefabs/house.tscn");
    //     var houseInstance = houseScene.Instantiate<Node3D>();
    //     houseInstance.Position = position;
    //     layer.layerParent.AddChild(houseInstance);
    // }
}
