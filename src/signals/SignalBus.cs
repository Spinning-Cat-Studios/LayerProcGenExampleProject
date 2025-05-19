using Godot;

public partial class SignalBus : Node
{
    [Signal]
    public delegate void RoadsGeneratedEventHandler(
        Vector3[] roadPositions,
        Vector3[] roadDirections, 
        int[] roadStartIndices,
        int[] roadEndIndices,
        Vector3 chunkIndex);
    
    [Signal]
    public delegate void InitialRoadEndPositionsComputedEventHandler(
        Vector3[] roadStartPositions,
        Vector3[] roadEndPositions,
        Vector3 chunkIndex);

    [Signal]
    public delegate void AllLSystemVillageChunksGeneratedEventHandler();

    [Signal]
    public delegate void LSystemVillageChunkReadyEventHandler(
        NodePath terrainPath
    );

    // Singleton instance reference (set this script as an autoload in Project Settings).
    public static SignalBus Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
}
