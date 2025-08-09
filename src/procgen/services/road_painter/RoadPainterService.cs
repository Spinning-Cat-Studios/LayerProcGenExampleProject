using Godot;
using T3  = Terrain3DBindings;
using static Terrain3D.Scripts.Utilities.ControlExtension;
using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Linq;
using LayerProcGenExampleProject.Models.Graph;
using Terrain3D.Scripts.Generation.Layers; // TerrainLODManager

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
    private readonly Random _rnd;
    private T3.Terrain3D Terrain => TerrainBlackboard.Terrain;
    private T3.Terrain3DStorage Storage => TerrainBlackboard.Storage;
    private GodotObject _data; // 0.9.3a: Terrain node holds a "data" object with granular control setters

    private bool IsTerrainSet => Terrain != null && Storage != null;

    public RoadPainterService()
    { 
        _rnd = new Random(Constants.GLOBAL_SEED);
    }

    private void BuildBrush()
    {
        _vSpacing = Terrain?.VertexSpacing ??
            // Fallback for older binding – suppress obsolete warning intentionally
            #pragma warning disable CS0618
            Terrain?.MeshVertexSpacing ??
            #pragma warning restore CS0618
            1.0f;
        var list = new List<Vector2>();
        for (float dx = -_halfWidth; dx <= _halfWidth; dx += _vSpacing)
            for (float dz = -_halfWidth; dz <= _halfWidth; dz += _vSpacing)
                if (dx * dx + dz * dz <= _halfWidth * _halfWidth)
                    list.Add(new Vector2(dx, dz));
        _brushOffsets = list.ToArray();
    }

    public void PaintRoad(Vector3[] waypoints) =>
        PaintRoad(waypoints, new int[] { 0 }, new int[] { waypoints.Length - 1 }, 0, 0);

    public void PaintRoad(Vector3[] road, int[] roadStartIndices, int[] roadEndIndices, int roadStartIndex = 2, int roadEndIndexOffset = 1)
    {
        // Check if the terrain is set before proceeding
        if (!IsTerrainSet)
        {
            GD.PrintErr("Terrain is not set. Cannot paint road.");
            return;
        }

        if (road.Length < 2) return;

        // one‑time initialisation
        if (_vSpacing == 0) _vSpacing = Terrain.VertexSpacing; // prefer new name
        if (_brushOffsets == null) BuildBrush();
        if (_data == null)
        {
            // Attempt to grab the new data object (0.9.3+). Safe if absent.
            try { _data = Terrain.AsNode3D.Get("data").AsGodotObject(); } catch { /* ignore */ }
        }

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
            PaintSubroad(subroad.ToArray(), roadCtrl, roadStartIndex, roadEndIndexOffset);
        }
    }

    private void PaintSubroad(Vector3[] road, uint roadCtrl, int startIndex, int endIndexOffset)
    {
        int road_end_index = road.Length - 1 - endIndexOffset;
        for (int i = startIndex; i < road_end_index; ++i)
        {
            Vector3 a = road[i];
            Vector3 b = road[i + 1];
            float seg = a.DistanceTo(b);
            int n = Mathf.CeilToInt(seg / _vSpacing);

            for (int s = 0; s <= n; ++s)
            {
                Vector3 c = a.Lerp(b, (float)s / n);
                foreach (var o in _brushOffsets)
                {
                    Vector3 p = c + new Vector3(o.X, 0, o.Y);
                    PaintPoint(p, roadCtrl);
                }
            }
        }

        _needsUpdate = true;
    }

    private void PaintPoint(Vector3 worldPos, uint roadCtrl)
    {
        // Ensure region exists before attempting to paint to avoid Terrain3DData 'No active region' errors.
        if (!TerrainLODManager.instance.EnsureRegionAt(worldPos))
            return;
        // If we have the new data object, set granular controls (base id, disable autoshader, blend)
        if (_data != null)
        {
            try
            {
                // Extract pieces from packed control value (in case future additions come in)
                byte baseId = roadCtrl.GetBaseTextureId();
                byte blend = roadCtrl.GetTextureBlend();
                bool autoshader = roadCtrl.IsAutoshaded();
                _data.Call("set_control_auto", worldPos, autoshader); // false for roads
                _data.Call("set_control_base_id", worldPos, (int)baseId);
                _data.Call("set_control_blend", worldPos, blend / 255.0f);
                // Angle / scale optional – only set if non-zero to reduce calls
                byte angle = roadCtrl.GetUvAngle();
                if (angle != 0) _data.Call("set_control_angle", worldPos, angle * 22.5f);
                byte scale = roadCtrl.GetUvScale();
                if (scale != 0) _data.Call("set_control_scale", worldPos, scale); // internal mapping
                return;
            }
            catch (Exception e)
            {
                GD.PushWarning($"RoadPainterService: data API failed, falling back to storage SetControl: {e.Message}");
                _data = null; // disable further attempts this session
            }
        }

        // Fallback: legacy storage method (pre-0.9.3)
        try { Storage.SetControl(worldPos, roadCtrl); } catch { /* swallow – avoid spam */ }
    }

    public void GenerateRoadsBetweenHamlets(
        List<((int, int) a, (int, int) b, string aJson, string bJson)> adjacentHamletRoadEndpoints,
        RoadGraph roadGraph)
    {
        // This method is called from a background thread.
        // We can do all the data processing and graph updates here,
        // but the actual painting must be deferred to the main thread.
        var allWaypointsToPaint = new List<Vector3[]>();

        foreach (var (a, b, aJson, bJson) in adjacentHamletRoadEndpoints)
        {
            // Use the endpoints (a, b) to generate roads between the hamlets.
            // GD.Print($"Generating road between hamlets at {a} and {b}");
            // Compute intermediate point between a and b given the JSON data.
            // This is a three step process:
            // 1. Determine which pairs of road endpoints are closest to each other.
            // 2. Compute intermediate waypoints between the two closest endpoints.
            // 3. Paint the road between the two endpoints using the waypoints.

            // Step 1.
            // Deserialize the string information to get the road endpoints.
            var aRoadEndPositionsArrList = JsonSerializer.Deserialize<List<float[]>>(aJson);
            var bRoadEndPositionsArrLlist = JsonSerializer.Deserialize<List<float[]>>(bJson);
            var aRoadEndPositions = aRoadEndPositionsArrList.Select(arr => new Vector3(arr[0], arr[1], arr[2])).ToList();
            var bRoadEndPositions = bRoadEndPositionsArrLlist.Select(arr => new Vector3(arr[0], arr[1], arr[2])).ToList();

            // Find the closest pair of road endpoints between hamlet a and b.
            Vector3 closestA = Vector3.Zero;
            Vector3 closestB = Vector3.Zero;
            float closestDistance = float.MaxValue;

            foreach (var aPos in aRoadEndPositions)
            {
                foreach (var bPos in bRoadEndPositions)
                {
                    float distance = aPos.DistanceTo(bPos);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestA = aPos;
                        closestB = bPos;
                    }
                }
            }

            // Step 2. Compute waypoints between the two closest endpoints.
            float minSegmentLength = closestDistance * 0.1f;
            float noiseFactor = 0.1f;
            List<Vector3> waypoints = GenerateNoisyWaypoints(closestA, closestB, minSegmentLength, noiseFactor);


            // GD.Print($"Generated waypoints between {a} and {b}: {waypoints.Count} waypoints.");

            // Step 3. Add the new connecting road to our graph
            var startNode = roadGraph.AddNode(closestA);
            var endNode = roadGraph.AddNode(closestB);
            roadGraph.AddEdge(startNode, endNode, waypoints);

            // Step 4. Collect the waypoints to be painted later.
            allWaypointsToPaint.Add(waypoints.ToArray());
        }

        // If there's anything to paint, schedule it on the main thread.
        if (allWaypointsToPaint.Any())
        {
            Callable.From(() => {
                foreach(var waypoints in allWaypointsToPaint)
                {
                    PaintRoad(waypoints);
                }
            }).CallDeferred();
        }
    }

    private List<Vector3> GenerateNoisyWaypoints(Vector3 a, Vector3 b, float minSegmentLength, float noiseFactor)
    {
        float distance = a.DistanceTo(b);
        if (distance < minSegmentLength)
        {
            // Base case: just return the endpoints
            return new List<Vector3> { a, b };
        }

        // Compute intermediate point with noise
        Vector3 intermediate = (a + b) / 2;
        float noiseAmount = distance * noiseFactor;
        var (jitterX, jitterZ) = GenerateJitter(noiseAmount);
        intermediate += new Vector3(jitterX, 0, jitterZ);

        // Recursively subdivide
        var left = GenerateNoisyWaypoints(a, intermediate, minSegmentLength, noiseFactor);
        var right = GenerateNoisyWaypoints(intermediate, b, minSegmentLength, noiseFactor);

        // Merge, avoiding duplicate intermediate point
        left.RemoveAt(left.Count - 1);
        left.AddRange(right);
        left.Add(b);
        return left;
    }

    public (float jitterX, float jitterZ) GenerateJitter(float range)
    {
        float jitterX = (float)(_rnd.NextDouble() * (2 * range) - range);
        float jitterZ = (float)(_rnd.NextDouble() * (2 * range) - range);
        return (jitterX, jitterZ);
    }

    // Flush the changes to the terrain.
    public void UpdateIfNeeded()
    {
        if (!_needsUpdate) return;
        // Prefer new data object map update; map type 1 == control maps
        if (_data != null)
        {
            try { _data.Call("force_update_maps", 1); }
            catch { Storage.ForceUpdateMaps(T3.MapType.TYPE_CONTROL); }
        }
        else
        {
            Storage.ForceUpdateMaps(T3.MapType.TYPE_CONTROL);
        }
        _needsUpdate = false;
    }
}
