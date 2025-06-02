using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Util;
using Runevision.LayerProcGen;
using Terrain3DBindings;
using SemaphoreSlim = System.Threading.SemaphoreSlim;

public abstract class LandscapeLayer<L, C, S> : ChunkBasedDataLayer<L, C, S>
	where L : LandscapeLayer<L, C, S>, new()
	where C : LandscapeChunk<L, C, S>, new()
	where S : LayerService
{
	public abstract int lodLevel { get; }

	public const int GridResolution = 256;
	public int gridResolution = GridResolution;
	public int chunkResolution = GridResolution - 8;
	public float terrainBaseHeight = -100;
	public float terrainHeight = 200;

	// public static readonly SemaphoreSlim ChunkCountSemaphore = new(1, 1);

	public static int gridDoneCounter { get; set; } = 0;

	static readonly Godot.Collections.Dictionary<string, int> TotalChunkDictionary = new()
	{
		{ nameof(LandscapeLayerA), 25 },
		{ nameof(LandscapeLayerB), 25 },
		{ nameof(LandscapeLayerC), 81 },
		{ nameof(LandscapeLayerD), 25 }
	};

	static void createChunkDoneDefault(string layerName)
	{
		lock (LandscapeChunkCounterBlackboard.ChunkCountLock)
		{
			LandscapeChunkCounterBlackboard.ChunkCountDictionary[layerName] =
				LandscapeChunkCounterBlackboard.ChunkCountDictionary.GetValueOrDefault(layerName, 0) + 1;
			// GD.Print($"🗺️ {layerName} chunk created, number created: {LandscapeChunkCounterBlackboard.ChunkCountDictionary[layerName]}");
			LandscapeChunkCounterBlackboard.GridDoneCounter++;
			// GD.Print($"{LandscapeChunkCounterBlackboard.GridDoneCounter} chunks done generating cf {TotalChunkDictionary.Values.Sum()}");
			if (LandscapeChunkCounterBlackboard.GridDoneCounter >= TotalChunkDictionary.Values.Sum())
			{
				LandscapeChunkCounterBlackboard.LandscapeChunksAreReady = true;
				GD.Print("✅ All chunks finished generating, emitting signal to TerrainManagerService");
				SignalBus.Instance.CallDeferred(
					"emit_signal",
					SignalBus.SignalName.LandscapeChunksReady
				);
				LandscapeChunkCounterBlackboard.GridDoneCounter = 0;
			}
		}
	}

	protected LandscapeLayer(
		int rollingGridWidth = 32,
		int rollingGridHeight = 0,
		int rollingGridMaxOverlap = 3,
		Action createChunkDone = null,
		LayerArgumentDictionary layerArguments = null
	) : base(
		rollingGridWidth,
		rollingGridHeight,
		rollingGridMaxOverlap,
		createChunkDone: createChunkDone ?? (() => createChunkDoneDefault(typeof(L).Name)))
	{
		TerrainNoise.SetFullTerrainHeight(new Vector2(terrainBaseHeight, terrainHeight));
		GD.Print($"LandscapeLayer<{typeof(L).Name}> constructor called with arguments: {layerArguments}");
		// var cultivationLayer = CultivationLayer.GetInstance(layerArguments);
		if (lodLevel < 2)
			AddLayerDependency(new LayerDependency(CultivationLayer.instance, CultivationLayer.requiredPadding, 0));
		if (lodLevel < 3)
			AddLayerDependency(new LayerDependency(LocationLayer.instance, LocationLayer.requiredPadding, 1));
	}
}

//@formatter:off
public class LandscapeLayerA : LandscapeLayer<LandscapeLayerA, LandscapeChunkA, LayerService>
{
	public override int lodLevel => 0;
	public override int chunkW => (int)RegionSize.SIZE_1024 / 8;
	public override int chunkH => (int)RegionSize.SIZE_1024 / 8;
	
	public LandscapeLayerA(LayerArgumentDictionary args = null)
    	: base(layerArguments: args) { 
		GD.Print("LandscapeLayerA constructor called with arguments: " + args);
	}

	public LandscapeLayerA()
	{
		GD.Print("LandscapeLayerA constructor called with no arguments.");
		// TODO: refactor chunk based data layer not to require parameterless constructor in inherited classes.
	}
}

public class LandscapeLayerB : LandscapeLayer<LandscapeLayerB, LandscapeChunkB, LayerService>
{
	public override int lodLevel => 1;
	public override int chunkW => (int)RegionSize.SIZE_1024 / 4;
	public override int chunkH => (int)RegionSize.SIZE_1024 / 4;
	
	public LandscapeLayerB(LayerArgumentDictionary args = null)
    	: base(layerArguments: args) { 
		GD.Print("LandscapeLayerB constructor called with arguments: " + args);
	}

	public LandscapeLayerB()
	{
		GD.Print("LandscapeLayerB constructor called with no arguments.");
		// TODO: refactor chunk based data layer not to require parameterless constructor in inherited classes.
	}
}

public class LandscapeLayerC : LandscapeLayer<LandscapeLayerC, LandscapeChunkC, LayerService>
{
	public override int lodLevel => 2;
	public override int chunkW => (int)RegionSize.SIZE_1024 / 4;
	public override int chunkH => (int)RegionSize.SIZE_1024 / 4;
	
	public LandscapeLayerC(LayerArgumentDictionary args = null)
    	: base(layerArguments: args) { 
		GD.Print("LandscapeLayerC constructor called with arguments: " + args);
	}

	public LandscapeLayerC()
	{
		GD.Print("LandscapeLayerC constructor called with no arguments.");
		// TODO: refactor chunk based data layer not to require parameterless constructor in inherited classes.
	}
}

public class LandscapeLayerD : LandscapeLayer<LandscapeLayerD, LandscapeChunkD, LayerService>
{
	public override int lodLevel => 3;
	public override int chunkW => (int)RegionSize.SIZE_1024;
	public override int chunkH => (int)RegionSize.SIZE_1024;
	
	public LandscapeLayerD(LayerArgumentDictionary args = null)
    	: base(layerArguments: args) { 
		GD.Print("LandscapeLayerD constructor called with arguments: " + args);
	}

	public LandscapeLayerD()
	{
		GD.Print("LandscapeLayerD constructor called with no arguments.");
		// TODO: refactor chunk based data layer not to require parameterless constructor in inherited classes.
	}
}

public class LandscapeChunkA : LandscapeChunk<LandscapeLayerA, LandscapeChunkA, LayerService> { }
public class LandscapeChunkB : LandscapeChunk<LandscapeLayerB, LandscapeChunkB, LayerService> { }
public class LandscapeChunkC : LandscapeChunk<LandscapeLayerC, LandscapeChunkC, LayerService> { }
public class LandscapeChunkD : LandscapeChunk<LandscapeLayerD, LandscapeChunkD, LayerService> { }

//@formatter:on
