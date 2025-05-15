using Runevision.Common;
using Runevision.LayerProcGen;
using Godot;
using System;
using LayerProcGenExampleProject.Services;
using LayerProcGenExampleProject.Services.SQLite;
using Godot.Util;

public class LSystemVillageLayer : ChunkBasedDataLayer<LSystemVillageLayer, LSystemVillageChunk, VillageService>, ILayerVisualization
{
	public override int chunkW { get { return 128; } }
	public override int chunkH { get { return 128; } }

    public static int gridDoneCounter { get; set; } = 0;

    static readonly int TotalChunks = 25;

    static readonly Action createChunkDoneDefault = static () => {
        gridDoneCounter++;
        if (gridDoneCounter >= TotalChunks)
        {
            GD.Print("✅  All chunks finished generating, emitting signal to VillageManagerService");
            SignalBus.Instance.CallDeferred(
                "emit_signal",
                SignalBus.SignalName.AllLSystemVillageChunksGenerated
            );
            gridDoneCounter = 0;
        }
    };
    static readonly Action removeChunkDoneDefault = static () => GD.Print("🗑️  A chunk level got removed");

    static float GetHeightAt(Vector3 position)
    {
        var coords2D = new Vector2(position.X, position.Z);
        return TerrainNoise.GetHeight(coords2D);
    }

    private static readonly VillageService _villageService = new VillageService(
        new SQLiteService(),
        new TurtleInterpreterService(GetHeightAt),
        new RoadPainterService()
    );

    public Node3D layerParent;

    /// <summary>
    /// Default constructor required for generic constraints.
    /// </summary>
    public LSystemVillageLayer() : this(
            rollingGridWidth: 32,
            rollingGridHeight: 0,
            rollingGridMaxOverlap: 3,
            createChunkDone: createChunkDoneDefault,
            removeChunkDone: removeChunkDoneDefault,
            service: _villageService
        ) { }

    /// <summary>
    /// Builds the layer and optionally lets you plug in delegates that are
    /// invoked when a chunk has finished generating or being removed.
    /// 
    /// All parameters have default values so the type still satisfies the
    /// `new()` constraint used by <see cref="ChunkBasedDataLayer{L,C}.instance"/>.
    /// </summary>
    public LSystemVillageLayer(
        int    rollingGridWidth      = 32,
        int    rollingGridHeight     = 0,
        int    rollingGridMaxOverlap = 3,
        Action createChunkDone      = null,
        Action removeChunkDone      = null,
        VillageService service = null)
        : base(rollingGridWidth,
               rollingGridHeight,
               rollingGridMaxOverlap,
               createChunkDone ?? createChunkDoneDefault,
               removeChunkDone ?? removeChunkDoneDefault,
               service ?? _villageService)
    {
        GD.Print("LSystemVillageLayer created");

        layerParent = new Node3D { Name = "LSystemVillageLayer" };

        // If you ever need layer‑to‑layer dependencies, add them here:
        // AddLayerDependency(new LayerDependency(OtherLayer.instance, padding, level));
    }

    public Node LayerRoot() => layerParent;

	public static DebugToggle debugLayer = DebugToggle.Create(">Layers/LSystemVillageLayer");

    public void VisualizationUpdate() { 
        if (!debugLayer.visible)
			return;
		for (int i = 0; i < GetLevelCount(); i++) {
			VisualizationManager.BeginDebugDraw(this, i);
			HandleAllChunks(i, c => c.DebugDraw());
			VisualizationManager.EndDebugDraw();
		}
     }
}
