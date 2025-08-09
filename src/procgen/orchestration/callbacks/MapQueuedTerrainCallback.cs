using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Godot;
using Godot.Collections;
using Godot.Util;
using Runevision.Common;
using Runevision.LayerProcGen;
using Terrain3DBindings;
using Terrain3D.Scripts.Generation.Layers;
using Terrain3D.Scripts.Utilities; // ControlExtension bitfield helpers

public struct MapQueuedTerrainCallback<L, C, S> : IQueuedAction
	where L : LandscapeLayer<L, C, S>, new()
	where C : LandscapeChunk<L, C, S>, new()
	where S : LayerService
{
	public float[,] heightmap;
	public uint[,] controlmap;
	public int[,] detailMap;
	public MeshInstance3D[] treeInstances; //there is no TreeInstance in Godot, but we can use Meshinstance, should be as powerful
	public L layer;
	public Point index;
	private readonly int regionSize;

	public MapQueuedTerrainCallback(
		float[,] heightmap,
		uint[,] controlmap,
		int[,] detailMap,
		MeshInstance3D[] treeInstances,
		L layer,
		Point index
	)
	{
		this.heightmap = heightmap;
		this.controlmap = controlmap;
		this.detailMap = detailMap;
		this.treeInstances = treeInstances;
		this.layer = layer;
		this.index = index;
		regionSize = (int)RegionSize.SIZE_1024;
	}

	static Terrain3DRegion GetOrCreateTerrain(Vector3 position, L layer)
	{
		if (!TerrainLODManager.instance.HasChunkAt(position))
			TerrainLODManager.instance.CreateNewChunkAt(position);
		var chunk = TerrainLODManager.instance.GetChunkAt(position);
		return chunk.LoD < layer.lodLevel ? null : chunk;
	}

	public void Process()
	{
		return; // TODO: remove this line when we are ready to process the terrain
		// LayerManagerBehavior.instance.StartCoroutine(ProcessRoutine());
	}

	public IEnumerator ProcessRoutine()
	{
		var startPos = index * layer.chunkW;
		Terrain3DRegion terrain = GetOrCreateTerrain(new Vector3(startPos.x, 0, startPos.y), layer);
		if (terrain == null)
			yield break;

		terrain.HeightMap ??= Image.CreateEmpty(regionSize, regionSize, false, Image.Format.Rf);
		terrain.ControlMap ??= Image.CreateEmpty(regionSize, regionSize, false, Image.Format.Rf);
		DPoint cellSize = (DPoint)layer.chunkSize / layer.gridResolution;
		float minHeight = layer.terrainBaseHeight;
		float totalHeight = layer.terrainHeight - layer.terrainBaseHeight;
		for (var x = 0; x < layer.chunkSize.x; x++)
		{
			for (var z = 0; z < layer.chunkSize.y; z++)
			{
				Vector3 globalPosition = new Vector3(startPos.x + x, 0, startPos.y + z);
				float h = heightmap[(int)(z / cellSize.y), (int)(x / cellSize.x)];
				uint packed = controlmap[(int)(z / cellSize.y), (int)(x / cellSize.x)];

				// Ensure region exists before painting.
				if (!TerrainLODManager.instance.EnsureRegionAt(globalPosition))
					continue;

				var dataObj = TerrainBlackboard.TerrainData; // new API object (Terrain3DData)
				if (dataObj != null)
				{
					try
					{
						// Height
						dataObj.Call("set_height", globalPosition, h);

						// Control bitfield unpack (see ControlExtension for layout)
						byte baseId = packed.GetBaseTextureId();
						byte overlayId = packed.GetOverlayTextureId();
						byte blendByte = packed.GetTextureBlend();
						byte angleSteps = packed.GetUvAngle(); // 0..15 -> *22.5°
						byte scaleVal = packed.GetUvScale();
						bool autoshader = packed.IsAutoshaded();
						bool nav = packed.IsNavigation();
						bool hole = packed.IsHole();

						// Autoshader (disable explicitly if false)
						if (!autoshader || dataObj.HasMethod("set_control_auto"))
							dataObj.Call("set_control_auto", globalPosition, autoshader);

						// Base texture id
						if (dataObj.HasMethod("set_control_base_id"))
							dataObj.Call("set_control_base_id", globalPosition, (int)baseId);

						// Overlay texture id (best guess for method name; guard with HasMethod)
						if (overlayId != 0 && dataObj.HasMethod("set_control_overlay_id"))
							dataObj.Call("set_control_overlay_id", globalPosition, (int)overlayId);

						// Blend (0..255 -> 0..1)
						if (blendByte != 0 && dataObj.HasMethod("set_control_blend"))
							dataObj.Call("set_control_blend", globalPosition, blendByte / 255.0f);

						// Angle (skip if zero)
						if (angleSteps != 0 && dataObj.HasMethod("set_control_angle"))
							dataObj.Call("set_control_angle", globalPosition, angleSteps * 22.5f);

						// Scale (raw value; plugin maps internally)
						if (scaleVal != 0 && dataObj.HasMethod("set_control_scale"))
							dataObj.Call("set_control_scale", globalPosition, scaleVal);

						// Navigation / hole flags (method names speculative; guarded)
						if (nav && dataObj.HasMethod("set_control_navigation"))
							dataObj.Call("set_control_navigation", globalPosition, true);
						if (hole && dataObj.HasMethod("set_control_hole"))
							dataObj.Call("set_control_hole", globalPosition, true);

						continue; // next pixel done via data API
					}
					catch (Exception e)
					{
						GD.PushWarning($"MapQueuedTerrainCallback: data API failure at {globalPosition}: {e.Message}; falling back to legacy storage.");
						// fall back below
					}
				}

				// Legacy storage path (pre‑0.9.3 or failure)
				TerrainLODManager.instance.terrain3DWrapper.Storage.SetHeight(globalPosition, h);
				TerrainLODManager.instance.terrain3DWrapper.Storage.SetControl(globalPosition, packed);
			}
		}
		// Force map updates: prefer new data API if present
		try
		{
			TerrainBlackboard.TerrainData?.Call("force_update_maps", 0); // 0 = height
			TerrainBlackboard.TerrainData?.Call("force_update_maps", 1); // 1 = control
		}
		catch
		{
			TerrainLODManager.instance.terrain3DWrapper.Storage.ForceUpdateMaps();
		}
		yield return null;
	}
}
