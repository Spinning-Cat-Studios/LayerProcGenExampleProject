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
            GD.Print("VillageService: All L-System village chunks have been generated.");
            List<((int, int) a, (int, int) b, string aJson, string bJson)> adjacentHamletRoadEndpoints = _databaseService.RetrieveAdjacentRoadEndPairs();
            GD.Print($"VillageService: Retrieved adjacent hamlet endpoints: {adjacentHamletRoadEndpoints.Count} pairs.");
            // GD.Print($"VillageService: Example pair: {adjacentHamletRoadEndpoints[0].a} and {adjacentHamletRoadEndpoints[0].b}");
            // GD.Print($"VillageService: Example JSON: {adjacentHamletRoadEndpoints[0].aJson} and {adjacentHamletRoadEndpoints[0].bJson}");
            _roadPainterService.GenerateRoadsBetweenHamlets(adjacentHamletRoadEndpoints, _roadGraph);

            GD.Print("VillageService: Road generation started.");
            GD.Print($"[VillageService] Final graph contains {this._roadGraph.Nodes.Count} nodes and {this._roadGraph.Edges.Count} edges.");
        }

        private void OnLSystemVillageChunkReady() { }

        private void OnRoadsGenerated(
            Vector3[] roadPositions,
            Vector3[] roadDirections,
            int[] roadStartIndices,
            int[] roadEndIndices,
            Vector3 chunkIndex)
        {
            // This handler is executed on a background thread.
            // We must defer the actual painting (which interacts with the scene tree)
            // to the main thread to prevent race conditions.
            Callable.From(() =>
                _roadPainterService.PaintRoad(roadPositions, roadStartIndices, roadEndIndices)
            ).CallDeferred();
        }

        private void OnRoadPainterServiceTimerTimeout()
        {
            // Handle the event when the road painter service timer times out.
            // GD.Print("RoadPainterService timer timeout.");
            _roadPainterService.UpdateIfNeeded();
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

            _databaseService.InsertOrReplace(new RoadChunkData
            {
                ChunkX = chunkIndex.x,
                ChunkY = chunkIndex.y,
                RoadEndPositions = roadEndPositionsString
            });
        }

        public void ClearPersistedRoadChunk(Runevision.Common.Point chunkIndex)
        {
            _databaseService.DeleteRoadChunk(chunkIndex);
        }

        // ─────────────────────────────────────────────────────────────
        //  Data generation utilities
        // ─────────────────────────────────────────────────────────────
        public bool ChunkDataExists(Runevision.Common.Point chunkIndex)
        {
            return _databaseService.ChunkDataExists(chunkIndex);
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
                if (!ChunkDataExists(chunkIndex))
                {
                    GD.Print($"[Data-Only Generation] Generating data for chunk ({chunkIndex.x}, {chunkIndex.y})");
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
