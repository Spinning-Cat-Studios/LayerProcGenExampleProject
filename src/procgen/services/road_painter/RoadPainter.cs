using Godot;
using T3  = Terrain3DBindings;
using static Terrain3D.Scripts.Utilities.ControlExtension;
using System.Collections.Generic;
using System.Linq;
using Runevision.Common;

public partial class RoadPainter : Node
{
    [Export] private NodePath _terrainPath;
    [Export] private int   _roadTexId   = 2;   // ID in the TextureList
    [Export] private float _halfWidth   = 1.0f;
    [Export] private float _sampleStep  = 1.0f;

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

        // Connect to global signal bus by hooking the event:
        // SignalBus.Instance.RoadsGenerated += OnRoadsGenerated;

        // Connect to the signal for when the road end positions are computed
        SignalBus.Instance.InitialRoadEndPositionsComputed += OnInitialRoadEndPositionsComputed;
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

    private void OnRoadsGenerated(
        Vector3[] roadPositions,
        Vector3[] roadDirections,
        int[] roadStartIndices,
        int[] roadEndIndices,
        Vector3 chunkIndex)
    {
        // Sensechecking.
        // GD.Print("Received RoadsGenerated signal with chunk index: ", chunkIndex);
        // GD.Print("Road positions: ", string.Join(", ", roadPositions));
        // GD.Print("Road directions: ", string.Join(", ", roadDirections));
        // Handle the roads generated event.
        // GD.Print("Received RoadsGenerated signal with chunk index: ", chunkIndex);
        PaintRoad(roadPositions, roadStartIndices, roadEndIndices);
    }

    private void OnInitialRoadEndPositionsComputed(
        Vector3[] roadStartPositions,
        Vector3[] roadEndPositions,
        Vector3 chunkIndex)
    {
        // Handle the initial road end positions computed event.
        GD.Print("Received InitialRoadEndPositionsComputed signal with chunk index: ", chunkIndex);
        // Sensechecking.
        // GD.Print("Road start positions: ", string.Join(", ", roadStartPositions));
        GD.Print("Road end positions: ", string.Join(", ", roadEndPositions));
    }

    private void OnAllChunksGenerated(string layerName)
    {
        // Handle the all chunks generated event.
        GD.Print("Received AllChunksGenerated signal for layer: ", layerName);
        if (layerName == "LSystemVillageLayer")
        {
            // Perform any necessary actions when all chunks are generated.
            GD.Print("All chunks for LSystemVillageLayer have been generated.");
        }
    }

    public void EchoPaintRoad(Vector3[] roadPositions)
    {
        GD.Print("Hello from EchoPaintRoad");
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


    public override void _PhysicsProcess(double _)
    {
        if (!_needsUpdate) return;
        _storage.ForceUpdateMaps(T3.MapType.TYPE_CONTROL);
        _needsUpdate = false;
    }

}
