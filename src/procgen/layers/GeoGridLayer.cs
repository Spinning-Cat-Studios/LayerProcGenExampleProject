#nullable enable
using Runevision.Common;
using Runevision.LayerProcGen;
using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Godot.Util;

// The GeoGridLayer is an intermediary layer between the LocationLayer and the CultivationLayer.
// Each CultivationLayer chunk needs data well outside of its own bounds.
// If they were each to calculate that themselves, there would be a lot of redundant
// calculations of the overlapping areas. The GeoGridLayer performs these calculations
// instead (essentially caching them) so multiple CultivationLayer chunks can use
// the same already calculated data.

public class GeoGridLayer : ChunkBasedDataLayer<GeoGridLayer, GeoGridChunk>
{
    public override int chunkW { get { return 360; } }
    public override int chunkH { get { return 360; } }

    public Point gridChunkRes;

    public GeoGridLayer()
    {
        gridChunkRes = chunkSize / TerrainPathFinder.halfCellSize;

        AddLayerDependency(new LayerDependency(LocationLayer.instance, LocationLayer.requiredPadding, 1));
    }

    public void GetDataInBounds(ILC q, GridBounds bounds, float[,] heights, Vector3[,] dists, uint[,] controls)
    {
        HandleGridPoints(q, bounds, chunkSize / TerrainPathFinder.halfCellSize,
            (GeoGridChunk chunk, Point localPointInChunk, Point globalPoint) =>
            {
                int x = globalPoint.x - bounds.min.x;
                int z = globalPoint.y - bounds.min.y;
                heights[z, x] = chunk.heights[localPointInChunk.y, localPointInChunk.x];
                dists[z, x] = chunk.dists[localPointInChunk.y, localPointInChunk.x];
                controls[z, x] = chunk.controls[localPointInChunk.y, localPointInChunk.x];
            }
        );
    }

        public GeoGridChunk GetChunk(Point index)
    {
        lock(chunks)
        {
            GeoGridChunk chunk = chunks[index];
            return chunk != null && chunk.level >= 0 ? chunk : null;
        }
    }

    public float SampleHeightAt(float x, float z)
    {
        int cellSize = TerrainPathFinder.halfCellSize;

        int globalX = Mathf.FloorToInt(x / cellSize);
        int globalZ = Mathf.FloorToInt(z / cellSize);

        Point chunkIndex = new Point(
            globalX / gridChunkRes.x,
            globalZ / gridChunkRes.y
        );

        GeoGridChunk chunk = GetChunk(chunkIndex);
        if (chunk == null)
            return 0f; // default fallback if chunk not available

        int localX = Mathf.PosMod(globalX, gridChunkRes.x);
        int localZ = Mathf.PosMod(globalZ, gridChunkRes.y);

        return chunk.heights[localZ, localX];
    }
}
