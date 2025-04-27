using Runevision.Common;
using Runevision.LayerProcGen;
using Godot;
using System.Collections.Generic;

public class LSystemVillageChunk : LayerChunk<LSystemVillageLayer, LSystemVillageChunk>
{
    List<Vector3> housePositions = new();
    Point gridOrigin;

    public override void Create(int level, bool destroy)
    {
        GD.Print("LSystemVillageChunk Create");
        // if (destroy)
        //     housePositions.Clear();
        // else
        //     Build();
    }

    public void DebugDraw() {
		GD.Print("LSystemVillageChunk DebugDraw");
	}

    // void Build()
    // {
    //     gridOrigin = index * layer.chunkW;

    //     // Generate your L-system string
    //     var rules = new Dictionary<char, string>
    //     {
    //         {'A', "ADA"},
    //         {'B', "D[B]D[B]"},
    //         {'C', "ACA"},
    //         {'D', "DA"}
    //     };
    //     var lSystem = new LSystem("B", rules);
    //     string lSequence = lSystem.Generate(4);

    //     // Start interpreting at the chunk's origin
    //     var interpreter = new TurtleInterpreter(GetHeightAt);
    //     interpreter.Interpret(lSequence, gridOrigin.ToVector3(), Vector3.Forward, housePositions);

    //     // Now housePositions has the exact positions for houses in this chunk
    //     foreach (var pos in housePositions)
    //     {
    //         PlaceHouseInstance(pos);
    //     }
    // }

    // float GetHeightAt(Vector3 position)
    // {
    //     return GeoGridLayer.instance.SampleHeightAt(position.X, position.Z);
    // }

    // void PlaceHouseInstance(Vector3 position)
    // {
    //     position.Y = GetHeightAt(position);
    //     var houseScene = GD.Load<PackedScene>("res://House.tscn");
    //     var houseInstance = houseScene.Instantiate<Node3D>();
    //     houseInstance.Position = position;
    //     layer.layerParent.AddChild(houseInstance);
    // }
}
