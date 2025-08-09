using Godot;
using Runevision.Common;
using Runevision.LayerProcGen;
using System.Collections.Generic;
using Terrain3D.Scripts.Utilities; // Added for TerrainManualRegionUtil
using Terrain3DBindings;
using Terrain3DWrapper = Terrain3DBindings.Terrain3D;

namespace Terrain3D.Scripts.Generation.Layers;

public partial class TerrainLODManager : Node
{
	public static TerrainLODManager instance;

	[Export(PropertyHint.NodeType, "Terrain3D")]
	public Node3D Terrain3D { get; set; }

	public Terrain3DWrapper terrain3DWrapper;

	private LayerArgumentDictionary layerArguments = new LayerArgumentDictionary
	{
		parameters = new Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Variant>>
		{
			{ "landscape_layer_id", new Godot.Collections.Dictionary<string, Variant> { { "id", "A" } } }
		}
	};

	static DebugToggle showCollision = DebugToggle.Create(">Terrain3D/Debug/Show Collision");
	static DebugToggle showCheckered = DebugToggle.Create(">Terrain3D/Checkered");
	static DebugToggle showGrey = DebugToggle.Create(">Terrain3D/Grey");
	static DebugToggle showHeightmap = DebugToggle.Create(">Terrain3D/Height");
	static DebugToggle showRoughmap = DebugToggle.Create(">Terrain3D/Heightmap");
	static DebugToggle showControlTexture = DebugToggle.Create(">Terrain3D/Control Texture");
	static DebugToggle showControlBlend = DebugToggle.Create(">Terrain3D/Control Blend");
	static DebugToggle showAutoShader = DebugToggle.Create(">Terrain3D/AutoShader");
	static DebugToggle showNavigation = DebugToggle.Create(">Terrain3D/Navigation");
	static DebugToggle showTextureHeight = DebugToggle.Create(">Terrain3D/Texture Height");
	static DebugToggle showTextureNormal = DebugToggle.Create(">Terrain3D/Texture Normal");
	static DebugToggle showTextureRough = DebugToggle.Create(">Terrain3D/Texture Rough");
	static DebugToggle showVertexGrid = DebugToggle.Create(">Terrain3D/Vertex Grid");

	class TerrainInfo
	{
		public Image heightMap;
		public Image colorMap;
		public Image controlMap;
	}

	struct TerrainLODLayer
	{
		public IChunkBasedDataLayer layer;
		public Dictionary<Point, TerrainInfo> chunks;
	}

	TerrainLODLayer[] layers;
	bool anyRegistrationChanges = false;
	GridBounds lastLowerLevelBounds;

	// Cache for regions where dynamic creation failed (to avoid spamming warnings every physics tick)
	private readonly HashSet<Vector3I> _failedRegionAdds = new();
	// Lazily discovered availability of a node-level add_region (some 0.9.3 builds moved it off storage)
	private enum AddRegionAvail { Unknown, Present, Absent }
	private AddRegionAvail _nodeAddRegionAvailable = AddRegionAvail.Unknown;

	DebugToggle debugLODBounds = DebugToggle.Create(">Visualizations/Terrain LOD Bounds");

	public override void _Ready()
	{
		SignalBus.Instance.GenerationSourceReady += layerArguments =>
		{
			layerArguments.parameters["landscape_layer_id"] = new Godot.Collections.Dictionary<string, Variant>
			{
				{ "id", "LandscapeLayerA" }
			};
			// GD.Print($"TerrainLODManager ready with layer arguments: {layerArguments.ToString()}");
			SetupLODLayer(0, LandscapeLayerA.GetInstance(layerArguments, "A"));
		};
		instance = this;
		showCollision.Callback += toggled => terrain3DWrapper.DebugShowCollision = toggled;
		showCheckered.Callback += toggled => terrain3DWrapper.Material.ShowCheckered = toggled;
		showGrey.Callback += toggled => terrain3DWrapper.Material.ShowGrey = toggled;
		showHeightmap.Callback += toggled => terrain3DWrapper.Material.ShowHeightmap = toggled;
		showRoughmap.Callback += toggled => terrain3DWrapper.Material.ShowRoughmap = toggled;
		showControlTexture.Callback += toggled => terrain3DWrapper.Material.ShowControlTexture = toggled;
		showControlBlend.Callback += toggled => terrain3DWrapper.Material.ShowControlBlend = toggled;
		showAutoShader.Callback += toggled => terrain3DWrapper.Material.ShowAutoshader = toggled;
		showNavigation.Callback += toggled => terrain3DWrapper.Material.ShowNavigation = toggled;
		showTextureHeight.Callback += toggled => terrain3DWrapper.Material.ShowTextureHeight = toggled;
		showTextureNormal.Callback += toggled => terrain3DWrapper.Material.ShowTextureNormal = toggled;
		showTextureRough.Callback += toggled => terrain3DWrapper.Material.ShowTextureRough = toggled;
		showVertexGrid.Callback += toggled => terrain3DWrapper.Material.ShowVertexGrid = toggled;
		// terrain3D = new Terrain3D(terrain3D);
		// terrain3D.Material = new Terrain3DMaterial();
		// terrain3D.Material.WorldBackground = WorldBackground.NONE;
		// AddChild(terrain3D.AsNode3D);
		terrain3DWrapper = new Terrain3DBindings.Terrain3D(Terrain3D);
		layers = new TerrainLODLayer[1];
	}

	public void SetupLODLayer(int lodLevel, IChunkBasedDataLayer layer)
	{
		layers[lodLevel] = new TerrainLODLayer
		{
			layer = layer,
			chunks = new Dictionary<Point, TerrainInfo>()
		};
	}

	public void RegisterChunk(int lodLevel, Point p /*, drop old Terrain3DStorage param */)
	{
		var loc = new Vector2I(-p.x, -p.y); // keep your sign convention
		int idx = terrain3DWrapper.Storage.GetRegionArrayIndex(loc);
		if (idx >= 0) {
			layers[lodLevel].chunks[p] = new TerrainInfo {
				heightMap  = terrain3DWrapper.Storage.HeightMaps[idx],
				colorMap   = terrain3DWrapper.Storage.ColorMaps[idx],
				controlMap = terrain3DWrapper.Storage.ControlMaps[idx],
			};
		} else {
			GD.Print($"Point {p} not found among region locations");
		}
		anyRegistrationChanges = true;
	}

	public void UnregisterChunk(int lodLevel, Point p)
	{
		if (layers[lodLevel].chunks.Remove(p, out TerrainInfo info))
		{
			anyRegistrationChanges = true;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (anyRegistrationChanges)
		{
			anyRegistrationChanges = false;

			// Find bounds of current terrain chunks in all layers.
			GridBounds lowestLayerBounds = GridBounds.Empty();
			int divisor = 1;
			for (int i = layers.Length - 1; i >= 0; i--)
			{
				foreach (var kvp in layers[i].chunks)
				{
					Point index = new Point(
						Crd.Div(kvp.Key.x, divisor),
						Crd.Div(kvp.Key.y, divisor)
					);
					lowestLayerBounds.Encapsulate(index);
				}

				divisor *= 2;
			}

			lastLowerLevelBounds = lowestLayerBounds;

			// Activate and deactivate terrain chunks.

			// // UnityEngine.Profiling.Profiler.BeginSample("HandleActivations"); //TODO: this maybe? https://docs.godotengine.org/en/latest/classes/class_performance.html
			int level = layers.Length - 1;
			for (int x = lowestLayerBounds.min.x; x < lowestLayerBounds.max.x; x++)
			{
				for (int y = lowestLayerBounds.min.y; y < lowestLayerBounds.max.y; y++)
				{
					HandleAreaIfCovered(level, new Point(x, y));
				}
			}
			// // UnityEngine.Profiling.Profiler.EndSample();
		}

		// Debug draw.
		if (debugLODBounds.visible)
		{
			DebugDrawer.alpha = debugLODBounds.animAlpha;
			var lowestLayer = layers[^1].layer;
			VisualizationManager.BeginDebugDraw(lowestLayer, 0);
			DebugDrawer.DrawRect(
				lastLowerLevelBounds.min * lowestLayer.chunkW,
				lastLowerLevelBounds.max * lowestLayer.chunkW,
				0,
				Colors.Yellow);
			VisualizationManager.EndDebugDraw();

			for (int i = 0; i < layers.Length; i++)
			{
				VisualizationManager.BeginDebugDraw(layers[i].layer, 0);
				foreach (var kvp in layers[i].chunks)
				{
					TerrainInfo info = kvp.Value;
					Image terrain = info.heightMap;
					bool active = !terrain.IsInvisible();
					if (active || (VisualizationManager.instance != null && VisualizationManager.instance.debugSeparate.visible)
					   )
					{
						Vector2 pos = kvp.Key * layers[i].layer.chunkSize;
						if (layers[i].layer is IGodotInstance godotLayer)
						{
							Node terrainNode = godotLayer.LayerRoot();
							if (terrainNode == null) continue;
							Vector2 size = new Vector2(layers[i].layer.chunkW * .5f, layers[i].layer.chunkH * .5f); //terrain.terrainData.size.xz() * 0.5f;
							// Draw rect.
							if (active)
								DebugDrawer.DrawRect(pos, pos + size * 2, 0, levelColors[i]);

							// Draw cross in LOD color.
							Vector3 center = (pos + size).xoy();
							float crossSize = size.X * (active ? 1f : 0.3f);
							DebugDrawer.DrawCross(center, crossSize, levelColors[i]);
						}
					}
				}

				VisualizationManager.EndDebugDraw();
			}

			DebugDrawer.alpha = 1f;
		}
	}

	public void HandleActivations()
	{
	}

	Color[] levelColors =
	{
		new(1.0f, 0.9f, 0.1f),
		new(0.1f, 1.0f, 0.7f),
		new(0.1f, 0.3f, 1.0f),
		new(0.8f, 0.1f, 0.5f)
	};


	// Returns true if full area is handled.
	bool HandleAreaIfCovered(int lodLevel, Point index, bool alreadyChecked = false, TerrainInfo selfInfo = null)
	{
		// if (lodLevel < 0)
		return false; // Stub implementation currently always returns false
		//
		// if (!alreadyChecked)
		// 	layers[lodLevel].chunks.TryGetValue(index, out selfInfo);
		//
		// // If at lowest LOD level, just handle self.
		// if (lodLevel == 0) {
		// 	if (selfInfo == null)
		// 		return false;
		// 	SetTerrainActiveStatus(selfInfo, true);
		// 	return true;
		// }
		//
		// int subLevel = lodLevel - 1;
		// Point subPointA = index * 2;
		// Point subPointB = subPointA + Point.right;
		// Point subPointC = subPointA + Point.up;
		// Point subPointD = subPointA + Point.one;
		// if (selfInfo == null) {
		// 	// We know here that own chunk is not available, so each sub-chunk is the
		// 	// highest potentially available and is allowed to be used individually.
		// 	bool fullAreaHandled = true;
		// 	fullAreaHandled &= HandleAreaIfCovered(subLevel, subPointA);
		// 	fullAreaHandled &= HandleAreaIfCovered(subLevel, subPointB);
		// 	fullAreaHandled &= HandleAreaIfCovered(subLevel, subPointC);
		// 	fullAreaHandled &= HandleAreaIfCovered(subLevel, subPointD);
		// 	return fullAreaHandled;
		// }
		//
		// // By now we know that own chunk is available, so only use sub-chunks if they cover
		// // the full area that own chunk covers, otherwise use own chunk.
		// var subChunks = layers[subLevel].chunks;
		// if (subChunks.TryGetValue(subPointA, out TerrainInfo subInfoA) & // All four must be evaluated
		// 	subChunks.TryGetValue(subPointB, out TerrainInfo subInfoB) & // so no && here
		// 	subChunks.TryGetValue(subPointC, out TerrainInfo subInfoC) &
		// 	subChunks.TryGetValue(subPointD, out TerrainInfo subInfoD)
		// ) {
		// 	// All sub-chunks are available, so use those and deactivate own chunk.
		// 	SetTerrainActiveStatus(selfInfo, false);
		// 	HandleAreaIfCovered(subLevel, subPointA, true, subInfoA);
		// 	HandleAreaIfCovered(subLevel, subPointB, true, subInfoB);
		// 	HandleAreaIfCovered(subLevel, subPointC, true, subInfoC);
		// 	HandleAreaIfCovered(subLevel, subPointD, true, subInfoD);
		// 	return true;
		// }
		//
		// // Not all sub-chunks are available, so use own chunk.
		// // Only deactivate sub-chunks if own chunk wasn't already active.
		// // If it was already active, sub-chunks can't be active too.
		// if (SetTerrainActiveStatus(layers[lodLevel].chunks[index], true)) {
		// 	DisableRecursive(subLevel, subPointA, true, subInfoA);
		// 	DisableRecursive(subLevel, subPointB, true, subInfoB);
		// 	DisableRecursive(subLevel, subPointC, true, subInfoC);
		// 	DisableRecursive(subLevel, subPointD, true, subInfoD);
		// }
		// Placeholder end
	}

	void DisableRecursive(int lodLevel, Point index, bool alreadyChecked = false, TerrainInfo info = null)
	{
		if (lodLevel < 0)
			return;

		if (!alreadyChecked)
			layers[lodLevel].chunks.TryGetValue(index, out info);

		if (info != null)
		{
			// If we can deactivate own chunk, it means own chunk was active before,
			// and that sub-chunks couldn't be active, so no need to handle those.
			// if (SetTerrainActiveStatus(info, false))
			return;
		}

		int subLevel = lodLevel - 1;
		Point subPointA = index * 2;
		Point subPointB = subPointA + Point.right;
		Point subPointC = subPointA + Point.up;
		Point subPointD = subPointA + Point.one;
		DisableRecursive(subLevel, subPointA);
		DisableRecursive(subLevel, subPointB);
		DisableRecursive(subLevel, subPointC);
		DisableRecursive(subLevel, subPointD);
	}

	// bool SetTerrainActiveStatus(TerrainInfo info, bool active) {
	// 	if (info.terrain.Visible == active)
	// 		return false;
	//
	// 	// // UnityEngine.Profiling.Profiler.BeginSample(active ? "Activate" : "Deactivate");
	// 	info.terrain.Visible = active;
	// 	// // UnityEngine.Profiling.Profiler.EndSample();
	// 	return true;
	// }

	public bool HasChunkAt(Vector3 position)
	{
		return terrain3DWrapper.Storage.HasRegion(position);
	}

	/// <summary>
	/// Align a world position to the region origin (assumes origin is min corner of a region).
	/// If Terrain3D ever switches to center-based regions this can be adjusted (+ regionSize/2).
	/// </summary>
	public Vector3 AlignToRegionOrigin(Vector3 worldPos)
	{
		if (terrain3DWrapper?.Storage == null) return worldPos;
		int rs = terrain3DWrapper.RegionSizePixels;
		int ax = Mathf.FloorToInt(worldPos.X / rs) * rs;
		int az = Mathf.FloorToInt(worldPos.Z / rs) * rs;
		return new Vector3(ax, 0, az);
	}

	/// <summary>
	/// Ensures a region covering worldPos exists. Returns true if region exists/created.
	/// </summary>
	public bool EnsureRegionAt(Vector3 worldPos, bool updateMaps = false)
	{
		if (terrain3DWrapper?.Storage == null) return false;
		var aligned = AlignToRegionOrigin(worldPos);
		// Always re-check existence first (could have been created by another system / frame)
		if (terrain3DWrapper.Storage.HasRegion(aligned)) return true;

		var key = new Vector3I((int)aligned.X, 0, (int)aligned.Z);
		// If we've already conclusively failed for this region this session, bail early
		if (_failedRegionAdds.Contains(key)) return false;

		var err = terrain3DWrapper.Storage.AddRegion(aligned, null, updateMaps);
		if (err == Error.Ok || err == Error.AlreadyExists)
		{
			return terrain3DWrapper.Storage.HasRegion(aligned);
		}

		// Storage path unavailable: attempt fallback to node-level method if present (some builds relocate API)
		if (err == Error.Unavailable)
		{
			if (_nodeAddRegionAvailable == AddRegionAvail.Unknown)
				_nodeAddRegionAvailable = terrain3DWrapper.AsNode3D.HasMethod("add_region") ? AddRegionAvail.Present : AddRegionAvail.Absent;

			if (_nodeAddRegionAvailable == AddRegionAvail.Present)
			{
				try
				{
					var v = terrain3DWrapper.AsNode3D.Call("add_region", aligned, new Godot.Collections.Array(), updateMaps);
					var nodeErr = v.As<Error>();
					if (nodeErr == Error.Ok || nodeErr == Error.AlreadyExists)
						return terrain3DWrapper.Storage.HasRegion(aligned);
					GD.PushWarning($"EnsureRegionAt: node-level add_region failed at {aligned} with {nodeErr}");
				}
				catch (System.Exception ex)
				{
					GD.PushWarning($"EnsureRegionAt: exception invoking node-level add_region at {aligned}: {ex.Message}");
				}
			}
			else if (_nodeAddRegionAvailable == AddRegionAvail.Absent)
			{
				// One-time general warning (only when first failure occurs and no node-level method available)
				if (_failedRegionAdds.Count == 0)
					GD.PushWarning("TerrainLODManager: dynamic region creation unavailable (no add_region on storage or terrain node). Regions outside pre-generated set will not load.");
			}
		}

		_failedRegionAdds.Add(key); // Mark as failed to suppress further spam
		if (err == Error.Unavailable)
		{
			// Attempt manual fallback creation
			if (TerrainManualRegionUtil.ManualEnsureRegion(terrain3DWrapper.Storage, aligned))
				return true;
		}
		GD.PushWarning($"EnsureRegionAt: AddRegion failed at {aligned} with {err} (manual fallback {(err==Error.Unavailable?"attempted":"skipped")})");
		return false;
	}

	public Error CreateNewChunkAt(Vector3 position)
	{
		var aligned = AlignToRegionOrigin(position); // fine to keep
		var data = (GodotObject)terrain3DWrapper.Data; // this must be Terrain3D.Get("data")

		// Mirror your AlreadyExists branch explicitly
		if ((bool)data.Call("has_regionp", aligned))
			return Error.AlreadyExists;

		// 0.9.3: create a blank region at the cell that contains this world position
		var regionObj = (GodotObject)data.Call("add_region_blankp", aligned, false);
		if (regionObj == null) return Error.Failed;

		// Bulk-adding? call once at the end; otherwise you can pass update=true above
		// data.Call("force_update_maps", 3); // TYPE_MAX = 3

		return Error.Ok;
	}

	public Terrain3D.Scripts.Utilities.Terrain3DRegion? GetChunkAt(Vector3 position)
	{
		int idx = terrain3DWrapper.Storage.GetRegionIndex(position); // uses get_region_idp on 0.9.3
		return idx >= 0 ? new Terrain3D.Scripts.Utilities.Terrain3DRegion(idx) : null;
	}

	public int? GetRegionId(Vector3 position)
	{
		var data = (GodotObject)terrain3DWrapper.Data;
		var id = (int)data.Call("get_region_idp", position);
		return (bool)data.Call("has_regionp", position) ? id : (int?)null;
	}

	/// <summary>
	/// Bulk ensure regions within an axis-aligned rectangle in XZ plane (inclusive bounds, world space).
	/// Useful to prewarm terrain around spawn when dynamic creation is partially unavailable.
	/// </summary>
	public void PrewarmRegions(Vector3 minWorld, Vector3 maxWorld, bool updateMaps = false)
	{
		if (terrain3DWrapper?.Storage == null) return;
		var rs = (int)terrain3DWrapper.Storage.RegionSize;
		int minX = Mathf.FloorToInt(minWorld.X / rs) * rs;
		int maxX = Mathf.FloorToInt(maxWorld.X / rs) * rs;
		int minZ = Mathf.FloorToInt(minWorld.Z / rs) * rs;
		int maxZ = Mathf.FloorToInt(maxWorld.Z / rs) * rs;
		for (int x = minX; x <= maxX; x += rs)
		{
			for (int z = minZ; z <= maxZ; z += rs)
			{
				EnsureRegionAt(new Vector3(x, 0, z), updateMaps);
			}
		}
	}
}
