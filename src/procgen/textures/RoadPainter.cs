using Godot;
using T3  = Terrain3DBindings;
using static Terrain3D.Scripts.Utilities.ControlExtension;

public partial class RoadPainter : Node
{
    [Export] private NodePath _terrainPath;
    [Export] private int   _roadTexId   = 2;   // ID in the TextureList
    [Export] private float _halfWidth   = 3.0f;
    [Export] private float _sampleStep  = 1.0f;

    private T3.Terrain3D        _terrain;
    private T3.Terrain3DStorage _storage;
    private float               _vSpacing;

    public override void _Ready()
    {
        SetTerrainPath(_terrainPath);

        // Connect to global signal bus by hooking the event:
        SignalBus.Instance.RoadsGenerated += OnRoadsGenerated;
    }

    public void SetTerrainPath(NodePath path)
    {
        _terrainPath = path;
        // 1. Grab the native node just as a regular Node3D
        Node3D terrainNode = GetNode<Node3D>(_terrainPath);
        // 2. Wrap it
        _terrain = new T3.Terrain3D(terrainNode);
        _storage     = _terrain.Storage;
    }

    private void OnRoadsGenerated(Vector3[] roadPositions, Vector3[] roadDirections, Vector3 chunkIndex)
    {
        GD.Print("Received RoadsGenerated signal with chunk index: ", chunkIndex);
        // GD.Print("Road positions: ", string.Join(", ", roadPositions));
        // GD.Print("Road directions: ", string.Join(", ", roadDirections));
        // Handle the roads generated event.
    }

    public void PaintRoad(Vector3[] roadPositions)
    {
        // build packed control value
        uint roadCtrl = 0;
        roadCtrl.SetBaseTextureId((byte)_roadTexId);
        roadCtrl.SetTextureBlend(0);
        roadCtrl.SetAutoshaded(false);

        for (int i = 0; i < roadPositions.Length - 1; ++i)
        {
            Vector3 a = roadPositions[i];
            Vector3 b = roadPositions[i + 1];
            float segLen = a.DistanceTo(b);
            int   steps  = Mathf.CeilToInt(segLen / _sampleStep);

            for (int s = 0; s <= steps; ++s)
            {
                Vector3 centre = a.Lerp(b, (float)s / steps);

                for (float dx = -_halfWidth; dx <= _halfWidth; dx += _vSpacing)
                for (float dz = -_halfWidth; dz <= _halfWidth; dz += _vSpacing)
                {
                    if (dx*dx + dz*dz > _halfWidth * _halfWidth) continue;

                    Vector3 worldPos = centre + new Vector3(dx, 0, dz);
                    _storage.SetControl(worldPos, roadCtrl);
                }
            }
        }

        // _storage.ForceUpdateMaps(T3.MapType.TYPE_CONTROL);
    }
}
