using Godot;
using T3  = Terrain3DBindings;
using static Terrain3D.Scripts.Utilities.ControlExtension;
using System.Collections.Generic;
using System;

public class RoadPainterService
{
    private NodePath _terrainPath;
    private int _roadTexId = 2;   // ID in the TextureList
    private float _halfWidth = 1.0f;
    private float _sampleStep = 1.0f;
    private Node _sceneRoot;

    private float _vSpacing;
    private Vector2[] _brushOffsets;
    private bool _needsUpdate;


    private T3.Terrain3D Terrain => TerrainBlackboard.Terrain;
    private T3.Terrain3DStorage Storage => TerrainBlackboard.Storage;

    private bool IsTerrainSet => Terrain != null && Storage != null;

    public RoadPainterService() { }

    private void BuildBrush()
    {
        _vSpacing = Terrain?.MeshVertexSpacing ?? 1.0f;
        var list = new List<Vector2>();
        for (float dx = -_halfWidth; dx <= _halfWidth; dx += _vSpacing)
            for (float dz = -_halfWidth; dz <= _halfWidth; dz += _vSpacing)
                if (dx * dx + dz * dz <= _halfWidth * _halfWidth)
                    list.Add(new Vector2(dx, dz));
        _brushOffsets = list.ToArray();
    }

    public void PaintRoad(Vector3[] road, int[] roadStartIndices, int[] roadEndIndices)
    {
        // Check if the terrain is set before proceeding
        if (!IsTerrainSet)
        {
            GD.PrintErr("Terrain is not set. Cannot paint road.");
            return;
        }

        if (road.Length < 2) return;

        // one‑time initialisation
        if (_vSpacing == 0) _vSpacing = Terrain.MeshVertexSpacing;
        if (_brushOffsets == null) BuildBrush();

        uint roadCtrl = 0;
        roadCtrl.SetBaseTextureId((byte)_roadTexId);
        roadCtrl.SetTextureBlend(0);
        roadCtrl.SetAutoshaded(false);

        // Split the road into subroads starting from roadStartIndice and ending at roadEndIndices
        // and paint each segment with the road control.
        List<List<Vector3>> subroads = new List<List<Vector3>>();
        for (int i = 0; i < roadStartIndices.Length; ++i)
        {
            int startIdx = (int)roadStartIndices[i];
            int endIdx = (int)roadEndIndices[i];

            if (startIdx < 0 || endIdx > road.Length || startIdx >= endIdx)
                continue;

            List<Vector3> subroad = new List<Vector3>();
            for (int j = startIdx; j < endIdx; ++j)
                subroad.Add(road[j]);

            subroads.Add(subroad);
        }
        // Paint each subroad
        foreach (var subroad in subroads)
        {
            PaintSubroad(subroad.ToArray(), roadCtrl);
        }
    }

    private void PaintSubroad(Vector3[] road, uint roadCtrl)
    {
        int ROAD_START_INDEX = 2;
        int ROAD_END_INDEX = road.Length - 2;
        for (int i = ROAD_START_INDEX; i < ROAD_END_INDEX; ++i)
        {
            Vector3 a = road[i];
            Vector3 b = road[i + 1];
            float seg = a.DistanceTo(b);
            int n = Mathf.CeilToInt(seg / _vSpacing);

            for (int s = 0; s <= n; ++s)
            {
                Vector3 c = a.Lerp(b, (float)s / n);

                foreach (var o in _brushOffsets)
                    Storage.SetControl(c + new Vector3(o.X, 0, o.Y), roadCtrl);
            }
        }

        _needsUpdate = true;
    }

    public void GenerateRoadsBetweenHamlets(List<((int, int) a, (int, int) b, string aJson, string bJson)> adjacentHamletRoadEndpoints)
    {
        foreach (var (a, b, aJson, bJson) in adjacentHamletRoadEndpoints)
        {
            // Use the endpoints (a, b) to generate roads between the hamlets.
            GD.Print($"Generating road between hamlets at {a} and {b}");
        }
    }

    // Flush the changes to the terrain.
    public void UpdateIfNeeded()
    {
        if (!_needsUpdate) return;
        Storage.ForceUpdateMaps(T3.MapType.TYPE_CONTROL);
        _needsUpdate = false;
    }
}
