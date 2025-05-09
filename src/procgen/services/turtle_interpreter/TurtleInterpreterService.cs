using Godot;
using System;

public class TurtleInterpreterService
{
    private readonly Func<Vector3, float> _heightSampler;

    public TurtleInterpreterService(Func<Vector3, float> heightSampler)
    {
        _heightSampler = heightSampler;
    }

    public void Interpret(
        string lSequence, 
        TurtleState initialState, 
        LSystemResult result)
    {
        var interpreter = new TurtleInterpreter(_heightSampler);

        interpreter.Interpret(
            lSequence,
            initialState,
            result.HousePositions,
            result.RoadPositionDirections,
            result.RoadStartIndices,
            result.RoadEndIndices,
            result.RoadStartPositions,
            result.RoadEndPositions
        );
    }
}
