using Godot;
using Runevision.LayerProcGen;

public partial class SignalBus : Node
{
    private static SignalBus _instance;

    // C# property - now only has a getter. It can't create an instance.
    public static SignalBus Instance => _instance;

    // GDScript accessible method
    public static SignalBus GetInstance()
    {
        return _instance;
    }

    // Using _EnterTree is more robust for singletons as it runs before _Ready.
    public override void _EnterTree()
    {
        if (_instance != null)
        {
            // Another instance was loaded, probably by mistake.
            QueueFree(); 
            return;
        }
        _instance = this;
    }

    public override void _Ready()
    {
        // You can keep this or remove it, as _EnterTree now handles the instance assignment.
        // It's good practice to ensure the instance is set here as well.
        if (_instance == null)
        {
            _instance = this;
        }
    }

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
    public delegate void RoadPainterServiceTimerTimeoutEventHandler();

    [Signal]
    public delegate void LSystemVillageChunkReadyEventHandler();

    [Signal]
    public delegate void LandscapeChunksReadyEventHandler();

    [Signal]
    public delegate void GenerationSourceReadyEventHandler(
        LayerArgumentDictionary layerArguments
    );

    [Signal]
    public delegate void ReconstructNodesEventHandler(
        Vector3 checkpointPosition,
        Vector3 currentCameraPosition,
        float distFromCheckpoint
    );

    [Signal]
    public delegate void PlayerSpawnEventHandler(
        Vector3 playerPosition
    );
}
