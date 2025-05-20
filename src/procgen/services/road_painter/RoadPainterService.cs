using Godot;
using T3  = Terrain3DBindings;
using static Terrain3D.Scripts.Utilities.ControlExtension;
using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Linq;

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

    private bool IsTerrainSet => Terrain != null && Storage != null;

    public RoadPainterService()
    { 
        _rnd = new Random(Constants.GLOBAL_SEED);
    }

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
            // Compute intermediate point between a and b given the JSON data.
            // This is a three step process:
            // 1. Determine which pairs of road endpoints are closest to each other.
            // 2. Compute the intermediate point between the two closest endpoints.
            // 3. Add a certain amount of noise to the intermediate point to make it more natural, this should be proportional to the distance between the two endpoints.

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

            // Step 2.
            // Compute the intermediate point between the two closest endpoints.
            Vector3 intermediatePoint = (closestA + closestB) / 2;

            // Step 3.
            // Add a certain amount of noise to the intermediate point to make it more natural,
            // this should be proportional to the distance between the two endpoints.
            float noiseAmount = closestDistance * 0.1f; // Adjust the noise factor as needed
            var (jitterX, jitterZ) = GenerateJitter(noiseAmount);
            intermediatePoint += new Vector3(jitterX, 0, jitterZ);

            GD.Print($"Generated road between {a} and {b} with intermediate point {intermediatePoint}");

            // Following this, we want to successively compute more intermediate points recursively
            // i.e. through a stack of iterated function calls e.g. towers of hanoi-esque, until successive
            // intermediate points are within a certain distance of each other.
            // This will be done in a follow-up commit.
        }
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
        Storage.ForceUpdateMaps(T3.MapType.TYPE_CONTROL);
        _needsUpdate = false;
    }
}
