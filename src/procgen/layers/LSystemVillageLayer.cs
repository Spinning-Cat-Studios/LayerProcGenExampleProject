using Runevision.Common;
using Runevision.LayerProcGen;
using Godot;
using System;

public class LSystemVillageLayer : ChunkBasedDataLayer<LSystemVillageLayer, LSystemVillageChunk>, ILayerVisualization
{
	public override int chunkW { get { return 128; } }
	public override int chunkH { get { return 128; } }

    static readonly Action createChunkDoneDefault = static () => GD.Print("✅  A chunk finished generating");
    static readonly Action removeChunkDoneDefault = static () => GD.Print("🗑️  A chunk level got removed");

    public Node3D layerParent;

    /// <summary>
    /// Default constructor required for generic constraints.
    /// </summary>
    public LSystemVillageLayer() : this(
            rollingGridWidth: 32,
            rollingGridHeight: 0,
            rollingGridMaxOverlap: 3,
            createChunkDone: createChunkDoneDefault,
            removeChunkDone: removeChunkDoneDefault
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
        Action removeChunkDone      = null)
        : base(rollingGridWidth,
               rollingGridHeight,
               rollingGridMaxOverlap,
               createChunkDone ?? createChunkDoneDefault,
               removeChunkDone ?? removeChunkDoneDefault)
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
