#nullable enable
using Runevision.Common;
using Runevision.LayerProcGen;
using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using Godot.Collections;
using Godot.Util;

// The GeoGridLayer is an intermediary layer between the LocationLayer and the CultivationLayer.
// Each CultivationLayer chunk needs data well outside of its own bounds.
// If they were each to calculate that themselves, there would be a lot of redundant
// calculations of the overlapping areas. The GeoGridLayer performs these calculations
// instead (essentially caching them) so multiple CultivationLayer chunks can use
// the same already calculated data.

public class GeoGridChunk : LayerChunk<GeoGridLayer, GeoGridChunk>, IDisposable
{
    public override void Create(int level, bool destroy)
    {
        GD.Print($"{GetType().Name} ({bounds}) {MethodBase.GetCurrentMethod()}: {level}, {destroy}");
        base.Create(level, destroy);
    }
    /*public float[,] heights;
    public Vector3[,] dists;
    public Vector4[,] splats;

    public GeoGridChunk()
    {
        unsafe
        {
            heights = new float[layer.gridChunkRes.y, layer.gridChunkRes.x];
            dists = new Vector3[layer.gridChunkRes.y, layer.gridChunkRes.x];
            splats = new Vector4[layer.gridChunkRes.y, layer.gridChunkRes.x];
            // LayerManager.instance.abort += Dispose;
        }
    }

    public void Dispose()
    {

    }

    public override void Create(int level, bool destroy)
    {
        if (destroy)
        {
            heights.Clear();
            dists.Clear();
            splats.Clear();
        }
        else
        {
            Build();
        }
    }

    static ListPool<LocationSpec> locationSpecListPool = new ListPool<LocationSpec>(64);

    void Build()
    {
        Point gridChunkRes = layer.gridChunkRes;
        Point gridOrigin = index * gridChunkRes;
        SimpleProfiler.ProfilerHandle ph;

        // Apply terrain noise.
        ph = SimpleProfiler.Begin(phc, "Height Noise");
        for (var zRes = 0; zRes < gridChunkRes.y; zRes++)
        {
            for (var xRes = 0; xRes < gridChunkRes.x; xRes++)
            {
                DPoint p = new DPoint(
                    (gridOrigin.x + xRes) * TerrainPathFinder.halfCellSize,
                    (gridOrigin.y + zRes) * TerrainPathFinder.halfCellSize
                );
                heights[zRes, xRes] = TerrainNoise.GetHeight((Vector2)p);
                dists[zRes, xRes] = new Vector3(0f, 0f, 1000f);
            }
        }
        SimpleProfiler.End(ph);

        // Apply deformation from locations.
        ph = SimpleProfiler.Begin(phc, "Deform-LocationDeformation");
        List<LocationSpec> locationSpecs = locationSpecListPool.Get();
        LocationLayer.instance.GetLocationSpecsOverlappingBounds(this, locationSpecs, bounds);
        var heightsNA = heights.AsSpan();
        var distsNA = dists.AsSpan();
        var splatsNA = splats.AsSpan();
        TerrainDeformation.ApplySpecs(
        	ref heightsNA, ref distsNA, ref splatsNA,
        	gridOrigin,
        	gridChunkRes,
        	Point.one * TerrainPathFinder.halfCellSize,
        	locationSpecs,
        	(SpecPointB p) => {
        		p.innerWidth += 2;
        		p.centerElevation = 0;
        		return p;
        	},
        	(SpecData d) => {
        		d.bounds = new Vector4(
        			d.bounds.X - 2, d.bounds.Y - 2, d.bounds.Z + 2, d.bounds.W + 2);
        		return d;
        	});
        locationSpecListPool.Return(ref locationSpecs);
        SimpleProfiler.End(ph);
    }*/
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

public class GeoGridLayer : ChunkBasedDataLayer<GeoGridLayer, GeoGridChunk>
{
    public override int chunkW { get { return 360; } }
    public override int chunkH { get { return 360; } }

    public Point gridChunkRes;

    public GeoGridLayer()
    {
        // gridChunkRes = chunkSize / TerrainPathFinder.halfCellSize;
        //
        // AddLayerDependency(new LayerDependency(LocationLayer.instance, LocationLayer.requiredPadding, 1));
    }

    public void GetDataInBounds(ILC q, GridBounds bounds, float[,] heights, Vector3[,] dists, Vector4[,] splats)
    {
        // HandleGridPoints(q, bounds, chunkSize / TerrainPathFinder.halfCellSize,
        //     (GeoGridChunk chunk, Point localPointInChunk, Point globalPoint) =>
        //     {
        //         int x = globalPoint.x - bounds.min.x;
        //         int z = globalPoint.y - bounds.min.y;
        //         heights[z, x] = chunk.heights[localPointInChunk.y, localPointInChunk.x];
        //         dists[z, x] = chunk.dists[localPointInChunk.y, localPointInChunk.x];
        //         splats[z, x] = chunk.splats[localPointInChunk.y, localPointInChunk.x];
        //     }
        // );
    }
}
