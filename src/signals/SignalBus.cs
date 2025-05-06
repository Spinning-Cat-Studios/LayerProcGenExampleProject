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

    // Singleton instance reference (set this script as an autoload in Project Settings).
    public static SignalBus Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
}
