using LayerProcGenExampleProject.Services.Database;
using LayerProcGenExampleProject.Services.Database.Entities;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Godot;
using Runevision.LayerProcGen;
using System;
using System.Linq;
using System.Text.Json;
using LayerProcGenExampleProject.Models.Graph;

namespace LayerProcGenExampleProject.Services
{
    public class VillageService : LayerService
    {
        private readonly DatabaseService _databaseService;
        private bool _onAllLSystemVillageChunksGeneratedCalled = false;
        private readonly TurtleInterpreterService _turtleInterpreterService;
        private readonly RoadPainterService _roadPainterService;
        private readonly RoadGraph _roadGraph = new RoadGraph();
        private readonly ConcurrentDictionary<(int x, int y), bool> _chunksBeingGenerated = new ConcurrentDictionary<(int x, int y), bool>();

        private float spacingModifier = 3.75f;
        private float jitterRange = 150f;

        private bool _subscribed;

        private void HookSignalsDeferred()
        {
            if (_subscribed) return;

            SignalBus.Instance.AllLSystemVillageChunksGenerated
                += OnAllLSystemVillageChunksGenerated;
            SignalBus.Instance.LSystemVillageChunkReady
                += OnLSystemVillageChunkReady;
            SignalBus.Instance.RoadsGenerated
                += OnRoadsGenerated;
            SignalBus.Instance.RoadPainterServiceTimerTimeout
                += OnRoadPainterServiceTimerTimeout;
            _subscribed = true;
        }

        public void Dispose()
        {
            if (!_subscribed) return;

            SignalBus.Instance.AllLSystemVillageChunksGenerated
                -= OnAllLSystemVillageChunksGenerated;
            SignalBus.Instance.RoadsGenerated
                -= OnRoadsGenerated;
            SignalBus.Instance.RoadPainterServiceTimerTimeout
                -= OnRoadPainterServiceTimerTimeout;
            _subscribed = false;
        }

        public VillageService(
            DatabaseService databaseService,
            TurtleInterpreterService turtleInterpreterService,
            RoadPainterService roadPainterService)
            : base("Village") // Pass the required layerName to the base constructor
        {
            GD.Print("VillageService: Constructor called.");
            _databaseService = databaseService;
            _turtleInterpreterService = turtleInterpreterService;
            _roadPainterService = roadPainterService;

            Callable.From(HookSignalsDeferred).CallDeferred();
        }

        private void OnAllLSystemVillageChunksGenerated()
        {
            if (_onAllLSystemVillageChunksGeneratedCalled)
            {
                GD.Print("VillageService: OnAllLSystemVillageChunksGenerated already called, skipping.");
                return;
            }
            _onAllLSystemVillageChunksGeneratedCalled = true;

            GD.Print("VillageService: All L-System village chunks have been generated.");
            List<((int, int) a, (int, int) b, string aJson, string bJson)> adjacentHamletRoadEndpoints = _databaseService.RetrieveAdjacentRoadEndPairs();
            GD.Print($"VillageService: Retrieved adjacent hamlet endpoints: {adjacentHamletRoadEndpoints.Count} pairs.");
            
            if (adjacentHamletRoadEndpoints.Count == 0)
            {
                GD.PrintErr("VillageService: No adjacent hamlet road endpoint pairs found! This means no roads will be generated between hamlets.");
                GD.PrintErr("VillageService: This could be because:");
                GD.PrintErr("  1. No data chunks have been generated yet");
                GD.PrintErr("  2. Chunks are not adjacent to each other");
                GD.PrintErr("  3. Database is empty or corrupted");
                
                // Let's check what's in the database
                var allChunks = _databaseService.Table<RoadChunkData>().ToList();
                GD.Print($"VillageService: Total chunks in database: {allChunks.Count}");
                foreach (var chunk in allChunks.Take(5)) // Show first 5 chunks
                {
                    GD.Print($"  Chunk ({chunk.ChunkX}, {chunk.ChunkY}) with {chunk.RoadEndPositions?.Length ?? 0} chars of road data");
                }
                if (allChunks.Count > 5)
                {
                    GD.Print($"  ... and {allChunks.Count - 5} more chunks");
                }
            }
            else
            {
                // // Show first few pairs for debugging
                // foreach (var pair in adjacentHamletRoadEndpoints.Take(3))
                // {
                //     GD.Print($"VillageService: Example pair: {pair.a} and {pair.b}");
                //     GD.Print($"VillageService: A endpoints: {pair.aJson?.Substring(0, Math.Min(100, pair.aJson?.Length ?? 0))}...");
                //     GD.Print($"VillageService: B endpoints: {pair.bJson?.Substring(0, Math.Min(100, pair.bJson?.Length ?? 0))}...");
                // }
            }
            
            _roadPainterService.GenerateRoadsBetweenHamlets(adjacentHamletRoadEndpoints, _roadGraph);

            PaintAllHamletRoads();

            GD.Print("VillageService: Road generation started.");
            GD.Print($"[VillageService] Final graph contains {this._roadGraph.Nodes.Count} nodes and {this._roadGraph.Edges.Count} edges.");
        }

        private string SerializeVector3List(IEnumerable<Vector3> vectors)
        {
            var serializableList = vectors.Select(v => new[] { v.X, v.Y, v.Z }).ToList();
            return JsonSerializer.Serialize(serializableList);
        }

        private List<Vector3> DeserializeVector3List(string json)
        {
            var serializedList = JsonSerializer.Deserialize<List<float[]>>(json);
            return serializedList.Select(p => new Vector3(p[0], p[1], p[2])).ToList();
        }

        private void OnLSystemVillageChunkReady() { }

        private void OnRoadsGenerated(
            Vector3[] roadPositions,
            Vector3[] roadDirections,
            int[] roadStartIndices,
            int[] roadEndIndices,
            Vector3 chunkIndex)
        {
            // GD.Print($"VillageService: Roads generated for chunk {chunkIndex}. Persisting to database.");
            
            var hamletRoadData = new HamletRoadData
            {
                ChunkX = (int)chunkIndex.X,
                ChunkY = (int)chunkIndex.Z,
                RoadPositionsJson = SerializeVector3List(roadPositions),
                RoadStartIndicesJson = JsonSerializer.Serialize(roadStartIndices),
                RoadEndIndicesJson = JsonSerializer.Serialize(roadEndIndices)
            };

            var point = new Runevision.Common.Point((int)chunkIndex.X, (int)chunkIndex.Z);

            try
            {
                // Check if the chunk already exists
                if (!_databaseService.HamletRoadDataExists(point))
                {
                    // If it does not exist, insert a new record
                    _databaseService.Insert(hamletRoadData);
                    return;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error checking chunk existence: {ex.Message}");
            }
        }

        private void OnRoadPainterServiceTimerTimeout()
        {
            // return; // Temporarily disable this handler to avoid unnecessary updates
            // Handle the event when the road painter service timer times out.
            // GD.Print("RoadPainterService timer timeout.");
            _roadPainterService.UpdateIfNeeded();
        }

        private void PaintAllHamletRoads()
        {
            GD.Print("VillageService: Landscape ready. Painting all persisted hamlet roads.");
            var allHamletRoads = _databaseService.Table<HamletRoadData>().ToList();
            GD.Print($"VillageService: Found {allHamletRoads.Count} hamlet road sets to paint.");

            // Print first few for debugging
            foreach (var roadData in allHamletRoads.Take(3))
            {
                GD.Print($"  Chunk ({roadData.ChunkX}, {roadData.ChunkY}) with {roadData.RoadPositionsJson?.Length ?? 0} chars of road positions");
                if (!string.IsNullOrEmpty(roadData.RoadPositionsJson))
                {
                    GD.Print($"  Road positions preview: {roadData.RoadPositionsJson.Substring(0, Math.Min(100, roadData.RoadPositionsJson.Length))}...");
                }
            }

            foreach (var roadData in allHamletRoads)
            {
                var roadPositions = DeserializeVector3List(roadData.RoadPositionsJson).ToArray();
                var roadStartIndices = JsonSerializer.Deserialize<int[]>(roadData.RoadStartIndicesJson);
                var roadEndIndices = JsonSerializer.Deserialize<int[]>(roadData.RoadEndIndicesJson);

                _roadPainterService.PaintRoad(roadPositions, roadStartIndices, roadEndIndices);
            }
        }

        public void SaveChunk(LSystemVillageChunk chunk)
        {
            _databaseService.Insert(chunk);
        }

        public List<RoadChunkData> GetAllChunks()
        {
            return _databaseService.Table<RoadChunkData>().ToList();
        }

        private void AddVillageToGraph(List<(Vector3, Vector3)> roadPositionDirections)
        {
            // Add the village chunk's roads to the road graph
            foreach (var (startPos, endPos) in roadPositionDirections)
            {
                var startNode = _roadGraph.AddNode(startPos);
                var endNode = _roadGraph.AddNode(endPos);
                // For local roads, the waypoints are just the start and end
                _roadGraph.AddEdge(startNode, endNode, new List<Vector3> { startPos, endPos });
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  STEP 1A - house/road generation (data only, no visual nodes)
        // ─────────────────────────────────────────────────────────────
        public LSystemResult GenerateVillageDataOnly(
            Runevision.Common.Point chunkIndex,
            LSystemVillageLayer layer)
        {
            // Same generation logic as GenerateVillageData but without visual node creation
            // (1) l-system
            int seed = Constants.GLOBAL_SEED + chunkIndex.x * Constants.CHUNK_X_RANDOM + chunkIndex.y * Constants.CHUNK_Y_RANDOM;
            var lSystemService = new LSystemService(seed);
            var axiom = lSystemService.SelectRandomAxiom();

            var (jitterX, jitterZ) = lSystemService.GenerateJitter(jitterRange);
            var worldOrigin = new Vector3(
                chunkIndex.x * layer.chunkW * spacingModifier + jitterX,
                0,
                chunkIndex.y * layer.chunkH * spacingModifier + jitterZ);

            var config = new LSystemConfig
            {
                ChunkSeed = seed,
                Iterations = Constants.LSYSTEM_ITERATIONS,
                WorldOrigin = worldOrigin,
                Axiom = axiom
            };

            string sequence = lSystemService.GenerateSequence(config.Axiom, config.Iterations);
            var state = new TurtleState(config.WorldOrigin, Vector3.Forward);
            var result = new LSystemResult();

            _turtleInterpreterService.Interpret(sequence, state, result);

            // After generating data, add the local roads to the graph
            AddVillageToGraph(result.RoadPositionDirections);

            return result;
        }

        // ─────────────────────────────────────────────────────────────
        //  STEP 1A - house/road generation (with visual nodes)
        // ─────────────────────────────────────────────────────────────
        public LSystemResult GenerateVillageData(
            Runevision.Common.Point chunkIndex,
            LSystemVillageLayer layer)
        {
            // (1) l-system
            int seed = Constants.GLOBAL_SEED + chunkIndex.x * Constants.CHUNK_X_RANDOM + chunkIndex.y * Constants.CHUNK_Y_RANDOM;
            var lSystemService = new LSystemService(seed);
            var axiom = lSystemService.SelectRandomAxiom();

            var (jitterX, jitterZ) = lSystemService.GenerateJitter(jitterRange);
            var worldOrigin = new Vector3(
                chunkIndex.x * layer.chunkW * spacingModifier + jitterX,
                0,
                chunkIndex.y * layer.chunkH * spacingModifier + jitterZ);

            var config = new LSystemConfig
            {
                ChunkSeed = seed,
                Iterations = Constants.LSYSTEM_ITERATIONS,
                WorldOrigin = worldOrigin,
                Axiom = axiom
            };

            string sequence = lSystemService.GenerateSequence(config.Axiom, config.Iterations);
            var state = new TurtleState(config.WorldOrigin, Vector3.Forward);
            var result = new LSystemResult();

            _turtleInterpreterService.Interpret(sequence, state, result);

            // After generating data, add the local roads to the graph
            AddVillageToGraph(result.RoadPositionDirections);

            return result;
        }

        // ─────────────────────────────────────────────────────────────
        //  STEP 1B - persistence
        // ─────────────────────────────────────────────────────────────
        public void PersistRoadChunk(
            Runevision.Common.Point chunkIndex,
            List<Vector3> roadEnds
        )
        {
            var serializableList = roadEnds.Select(v => new float[] { v.X, v.Y, v.Z }).ToList();
            var roadEndPositionsString = JsonSerializer.Serialize(serializableList);

            try {
                // Check if the chunk already exists
                if (!_databaseService.RoadChunkDataExists(chunkIndex))
                {
                    // If it does not exist, insert a new record
                    _databaseService.Insert(new RoadChunkData
                    {
                        ChunkX = chunkIndex.x,
                        ChunkY = chunkIndex.y,
                        RoadEndPositions = roadEndPositionsString
                    });
                    return;
                }
                
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error checking chunk existence: {ex.Message}");
            }

        }

        public void ClearPersistedRoadChunk(Runevision.Common.Point chunkIndex)
        {
            _databaseService.DeleteRoadChunk(chunkIndex);
        }

        // ─────────────────────────────────────────────────────────────
        //  Data generation utilities
        // ─────────────────────────────────────────────────────────────
        public bool RoadChunkDataExists(Runevision.Common.Point chunkIndex)
        {
            return _databaseService.RoadChunkDataExists(chunkIndex);
        }

        public void GenerateChunkDataOnly(Runevision.Common.Point chunkIndex, LSystemVillageLayer layer)
        {
            var chunkKey = (chunkIndex.x, chunkIndex.y);
            
            // Try to mark this chunk as being generated
            if (!_chunksBeingGenerated.TryAdd(chunkKey, true))
            {
                // Another thread is already generating this chunk, skip
                return;
            }

            try
            {
                // Only generate data if it doesn't already exist in the database
                if (!RoadChunkDataExists(chunkIndex))
                {
                    // GD.Print($"[Data-Only Generation] Generating data for chunk ({chunkIndex.x}, {chunkIndex.y})");
                    var result = GenerateVillageDataOnly(chunkIndex, layer);
                    PersistRoadChunk(chunkIndex, result.RoadEndPositions);
                }
                else
                {
                    // GD.Print($"[Data-Only Generation] Chunk ({chunkIndex.x}, {chunkIndex.y}) data already exists, skipping");
                }
            }
            finally
            {
                // Always remove the chunk from the generation tracking
                _chunksBeingGenerated.TryRemove(chunkKey, out _);
            }
        }
    }
}
