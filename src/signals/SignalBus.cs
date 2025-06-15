using Godot;
using Runevision.LayerProcGen;

public partial class SignalBus : Node
{
    private static SignalBus _instance;
    
    // C# property
    public static SignalBus Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SignalBus();
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }
    
    // GDScript accessible method
    public static SignalBus GetInstance()
    {
        return Instance;
    }
    
    public override void _Ready()
    {
        _instance = this;
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
}
