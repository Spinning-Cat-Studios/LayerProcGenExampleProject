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
}
