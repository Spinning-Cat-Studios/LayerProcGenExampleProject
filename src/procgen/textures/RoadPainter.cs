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
        _terrain  = GetNode<T3.Terrain3D>(_terrainPath);
        _storage  = _terrain.Storage;
        _vSpacing = _terrain.MeshVertexSpacing;

        // Connect to global signal bus
        SignalBus.Instance.Connect(nameof(SignalBus.RoadsGeneratedEventHandler), new Callable(this, nameof(OnRoadsGenerated)));
    }

    private void OnRoadsGenerated(Vector3[] roadPositions, Vector3[] roadDirections, Vector3 chunkIndex)
    {
        GD.Print("Received RoadsGenerated signal with chunk index: ", chunkIndex);
        GD.Print("Road positions: ", string.Join(", ", roadPositions));
        GD.Print("Road directions: ", string.Join(", ", roadDirections));
        // Handle the roads generated event.
    }

    public void PaintRoad(Vector3[] wayPoints)
    {
        // build packed control value
        uint roadCtrl = 0;
        roadCtrl.SetBaseTextureId((byte)_roadTexId);
        roadCtrl.SetTextureBlend(0);
        roadCtrl.SetAutoshaded(false);

        for (int i = 0; i < wayPoints.Length - 1; ++i)
        {
            Vector3 a = wayPoints[i];
            Vector3 b = wayPoints[i + 1];
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

        _storage.ForceUpdateMaps(T3.MapType.TYPE_CONTROL);
    }
}
