using Godot;
using T3  = Terrain3DBindings;
using static Terrain3D.Scripts.Utilities.ControlExtension;
using System.Collections.Generic;
using System;

public class RoadPainterService
{
    private NodePath _terrainPath;
    private int   _roadTexId   = 2;   // ID in the TextureList
    private float _halfWidth   = 1.0f;
    private float _sampleStep  = 1.0f;
    private Node _sceneRoot;
    private T3.Terrain3D _terrain;
    private T3.Terrain3DStorage _storage;
    private float _vSpacing;
    private Vector2[] _brushOffsets;
    private bool _needsUpdate;
    private bool _isTerrainSet = false;

    public RoadPainterService(NodePath terrainPath)
    {
        _terrainPath = terrainPath;
    }

    public void SetTerrain(NodePath path)
    {
        // First, drop the leading '../' if it exists
        string pathStr = path.ToString();
        if (pathStr.StartsWith("../"))
            path = new NodePath(pathStr.Substring(3));

        GD.Print("Setting terrain path in RoadPainterService to: ", path);

        // Set the terrain path for the road painter service
        _terrainPath = path;

        // 1) grab the scene-root *now* that the scene is up
        var tree = Engine.GetMainLoop() as SceneTree;
        _sceneRoot = tree?.CurrentScene as Node3D
            ?? throw new InvalidOperationException("Could not resolve SceneTree.CurrentScene to Node3D");

        // 2) get the terrain node
        if (!_sceneRoot.HasNode(_terrainPath))
            throw new InvalidOperationException($"No node at '{_terrainPath}'");
        Node3D terrainNode = _sceneRoot.GetNode<Node3D>(_terrainPath);

        // 3) wrap it
        _terrain = new T3.Terrain3D(terrainNode);
        _storage = _terrain.Storage;
        _vSpacing = _terrain.MeshVertexSpacing;
        BuildBrush();

        // 4) set the flag to true
        _isTerrainSet = true;
        GD.Print("Terrain path set in RoadPainterService.");
    }
    
    private void BuildBrush()
    {
        var list = new List<Vector2>();
        for (float dx = -_halfWidth; dx <= _halfWidth; dx += _vSpacing)
        for (float dz = -_halfWidth; dz <= _halfWidth; dz += _vSpacing)
            if (dx*dx + dz*dz <= _halfWidth * _halfWidth)
                list.Add(new Vector2(dx, dz));

        _brushOffsets = list.ToArray();
    }

    public void PaintRoad(Vector3[] road, int[] roadStartIndices, int[] roadEndIndices)
    {
        // Check if the terrain is set before proceeding
        if (!_isTerrainSet)
        {
            GD.PrintErr("Terrain is not set. Cannot paint road.");
            return;
        }

        if (road.Length < 2) return;

        // one‑time initialisation
        if (_vSpacing == 0) _vSpacing = _terrain.MeshVertexSpacing;
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
            int endIdx   = (int)roadEndIndices[i];

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
        int ROAD_END_INDEX   = road.Length - 2;
        for (int i = ROAD_START_INDEX; i < ROAD_END_INDEX; ++i)
        {
            Vector3 a = road[i];
            Vector3 b = road[i + 1];
            float seg = a.DistanceTo(b);
            int   n   = Mathf.CeilToInt(seg / _vSpacing);

            for (int s = 0; s <= n; ++s)
            {
                Vector3 c = a.Lerp(b, (float)s / n);

                foreach (var o in _brushOffsets)
                    _storage.SetControl(c + new Vector3(o.X, 0, o.Y), roadCtrl);
            }
        }

        _needsUpdate = true;
    }
}
