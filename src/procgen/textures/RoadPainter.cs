using Godot;
using T3  = Terrain3DBindings;
using static Terrain3D.Scripts.Utilities.ControlExtension;
using System.Collections.Generic;
using System.Linq;

public partial class RoadPainter : Node
{
    [Export] private NodePath _terrainPath;
    [Export] private int   _roadTexId   = 2;   // ID in the TextureList
    [Export] private float _halfWidth   = 1.0f;
    [Export] private float _sampleStep  = 1.0f;

    private readonly Dictionary<int, Vector3[]> _pending = new();

    private readonly Dictionary<int, Vector3[]> _waypoints = new();
    private readonly HashSet<int>               _mapsReady = new();

    private T3.Terrain3D        _terrain;
    private T3.Terrain3DStorage _storage;
    private float               _vSpacing;
    private Vector2[]       _brushOffsets;
    private bool _needsUpdate;

    public override void _Ready()
    {
        SetTerrain(_terrainPath);
        _vSpacing = _terrain.MeshVertexSpacing;
        _sampleStep = _vSpacing;
        BuildBrush();

        // Connect to the signals by hooking handlers to the events
        SignalBus.Instance.RoadsGenerated += OnRoadsGenerated;
        SignalBus.Instance.ChunkMapsReady += OnChunkMapsReady;
    }

    private void OnRoadsGenerated(Vector3[] points, int key)
    {
        GD.Print("RoadPainter.OnRoadsGenerated", key);
        _waypoints[key] = points;
        TryPaint(key);
    }

    private void OnChunkMapsReady(int key)
    {
        _mapsReady.Add(key);
        TryPaint(key);
    }

    private void TryPaint(int key)
    {
        // GD.Print("RoadPainter.TryPaint", key);
        if (!_mapsReady.Contains(key))            // terrain not ready yet
            return;
        // GD.Print("RoadPainter.TryPaint: maps ready", key);
        if (!_waypoints.TryGetValue(key, out var road))
            return;                               // no way‑points yet
        GD.Print("RoadPainter.TryPaint: waypoints", key);

        PaintRoad(road);                          // ← your existing method
        _waypoints.Remove(key);                   // tidy up
        _mapsReady.Remove(key);
        _needsUpdate = true;                      // upload once in _PhysicsProcess
    }

    public void SetTerrain(NodePath path)
    {
        _terrainPath = path;
        // 1. Grab the native node just as a regular Node3D
        Node3D terrainNode = GetNode<Node3D>(_terrainPath);
        // 2. Wrap it
        _terrain = new T3.Terrain3D(terrainNode);
        _storage     = _terrain.Storage;
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

    public void PaintRoad(Vector3[] road)
    {
        GD.Print("RoadPainter.PaintRoad", road.Length);
        if (road.Length < 2) return;

        // one‑time initialisation
        if (_vSpacing == 0) _vSpacing = _terrain.MeshVertexSpacing;
        if (_brushOffsets == null) BuildBrush();

        uint roadCtrl = 0;
        roadCtrl.SetBaseTextureId((byte)_roadTexId);
        roadCtrl.SetTextureBlend(0);
        roadCtrl.SetAutoshaded(false);

        for (int i = 0; i < road.Length - 1; ++i)
        {
            Vector3 a = road[i];
            Vector3 b = road[i + 1];
            float seg = a.DistanceTo(b);
            int   n   = Mathf.CeilToInt(seg / _vSpacing);

            for (int s = 0; s <= n; ++s)
            {
                Vector3 centre = a.Lerp(b, (float)s / n);

                foreach (var o in _brushOffsets)
                {
                    Vector3 p = centre + new Vector3(o.X, 0, o.Y);

                    uint ctrl = (uint)_storage.GetControl(p);

                    ctrl.SetBaseTextureId((byte)_roadTexId);
                    ctrl.SetTextureBlend(0);
                    ctrl.SetAutoshaded(false);

                    _storage.SetControl(p, ctrl);
                }
            }
        }

        _needsUpdate = true;
    }


    public override void _PhysicsProcess(double _)
    {
        if (!_needsUpdate) return;
        _storage.ForceUpdateMaps();     // TYPE_MAX – updates height+control
        _needsUpdate = false;
    }

}
