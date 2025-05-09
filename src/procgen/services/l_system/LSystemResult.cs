using Godot;
using System.Collections.Generic;

public class LSystemResult
{
    public List<Vector3> HousePositions { get; } = new();
    public List<(Vector3, Vector3)> RoadPositionDirections { get; } = new();
    public List<int> RoadStartIndices { get; } = new();
    public List<int> RoadEndIndices { get; } = new();
    public List<Vector3> RoadStartPositions { get; } = new();
    public List<Vector3> RoadEndPositions { get; } = new();
}
