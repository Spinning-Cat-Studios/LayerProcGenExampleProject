using Godot;

public partial class SignalBus : Node
{
    [Signal]
    public delegate void RoadsGeneratedEventHandler(
        Vector3[] roadPositions,
        int chunkKey
    );

    [Signal]
    public delegate void ChunkMapsReadyEventHandler(int chunkKey);

    // Singleton instance reference (set this script as an autoload in Project Settings).
    public static SignalBus Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
}
