using Runevision.Common;
using Runevision.LayerProcGen;
using Godot;
using System;
using LayerProcGenExampleProject.Services;
using LayerProcGenExampleProject.Services.Database;
using Godot.Util;
using System.Linq;
using System.Threading;

public class LSystemVillageLayer : ChunkBasedDataLayer<LSystemVillageLayer, LSystemVillageChunk, VillageService>, ILayerVisualization
{
	public override int chunkW { get { return 128; } }
	public override int chunkH { get { return 128; } }

    private static int gridDoneCounter = 0;

    public static string layerName { get; } = nameof(LSystemVillageLayer);
    static readonly int TotalChunks = 25;
    private static readonly object _gridDoneLock = new();

    static readonly Action createChunkReadyDefault = static () =>
    {
        SignalBus.Instance.CallDeferred(
            "emit_signal",
            SignalBus.SignalName.LSystemVillageChunkReady
        );
    };

    static readonly Action createChunkDoneDefault = static () =>
    {
        lock (_gridDoneLock)
        {
            int count = ++gridDoneCounter;
            if (count >= TotalChunks)
            {
                SignalBus.Instance.CallDeferred(
                    "emit_signal",
                    SignalBus.SignalName.AllLSystemVillageChunksGenerated
                );
                gridDoneCounter = 0;
            }
        }
    };
    static readonly Action removeChunkDoneDefault = static () => GD.Print("🗑️  A chunk level got removed");

    static float GetHeightAt(Vector3 position)
    {
        var coords2D = new Vector2(position.X, position.Z);
        return TerrainNoise.GetHeight(coords2D);
    }

    public override void ApplyArguments(LayerArgumentDictionary args)
    {
        // GD.Print("LSystemVillageLayer.ApplyArguments");
        // GD.Print(args.parameters);
        // outer key is the layer’s name (you could also use GetType().Name)
        var myKey = nameof(LSystemVillageLayer);

        // look up the inner map for this layer
        if (args.parameters
                 .TryGetValue(myKey, out var layerParams))
        {
            // Handle timer creation if specified
            if (layerParams.TryGetValue("Node:Timer", out var timerSignalVariant))
            {
                string signalMethod = timerSignalVariant.AsString();
                // GD.Print($"Creating Timer node for {myKey}, connecting to {signalMethod}");

                var timer = new Godot.Timer();
                timer.WaitTime = 0.5f; // Or make this configurable
                timer.Autostart = true;
                timer.OneShot = false;

                var tree = Engine.GetMainLoop() as SceneTree;
                var sceneRoot = tree?.CurrentScene as Node3D;
                if (sceneRoot != null)
                {
                    sceneRoot.CallDeferred("add_child", timer);
                    // GD.Print($"Timer node scheduled to be added to {sceneRoot.Name}");

                    // Connect the timeout signal to a method on SignalBus.Instance
                    timer.CallDeferred(
                        "connect",
                        "timeout",
                        Callable.From((Action)(() =>
                        {
                            SignalBus.Instance.CallDeferred(
                                "emit_signal",
                                signalMethod
                            );
                        }))
                    );

                    timer.TreeEntered += () =>
                    {
                        timer.Start();
                        // GD.Print($"Timer started with wait time: {timer.WaitTime}");
                    };

                    timer.TreeExited += () =>
                    {
                        timer.Stop();
                        // GD.Print($"Timer stopped");
                    };
                }
                else
                {
                    GD.PrintErr("Scene root is null, cannot add Timer node.");
                }
            }
        }
    }

    private static readonly VillageService _villageService = new VillageService(
        new DatabaseService(),
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
            createChunkReady: createChunkReadyDefault,
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
        Action createChunkReady      = null,
        Action createChunkDone       = null,
        Action removeChunkDone       = null,
        VillageService service       = null)
        : base(rollingGridWidth,
               rollingGridHeight,
               rollingGridMaxOverlap,
               createChunkReady ?? createChunkReadyDefault,
               createChunkDone ?? createChunkDoneDefault,
               removeChunkDone ?? removeChunkDoneDefault,
               service ?? _villageService)
    {
        GD.Print("LSystemVillageLayer constructor called");

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
