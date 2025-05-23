using LayerProcGenExampleProject.Services.Database;
using LayerProcGenExampleProject.Services.Database.Entities;
using System.Collections.Generic;
using Godot;
using Runevision.LayerProcGen;
using System;
using System.Linq;
using System.Text.Json;

namespace LayerProcGenExampleProject.Services
{
    public class VillageService : LayerService
    {
        private readonly DatabaseService _databaseService;
        private readonly TurtleInterpreterService _turtleInterpreterService;
        private readonly RoadPainterService _roadPainterService;

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
            GD.Print("VillageService: Road generation started.");
            _roadPainterService.GenerateRoadsBetweenHamlets(adjacentHamletRoadEndpoints);
        }

        private void OnLSystemVillageChunkReady() { }

        private void OnRoadsGenerated(
            Vector3[] roadPositions,
            Vector3[] roadDirections,
            int[] roadStartIndices,
            int[] roadEndIndices,
            Vector3 chunkIndex)
        {
            // Handle the event when roads are generated.
            // GD.Print("Received RoadsGenerated signal with chunk index: ", chunkIndex);
            _roadPainterService.PaintRoad(roadPositions, roadStartIndices, roadEndIndices);
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

        // ─────────────────────────────────────────────────────────────
        //  STEP 1A - house/road generation
        // ─────────────────────────────────────────────────────────────
        public LSystemResult GenerateVillageData(
            Runevision.Common.Point chunkIndex,
            LSystemVillageLayer layer)
        {
            // (1) l-system
            int seed = Constants.GLOBAL_SEED + chunkIndex.x * Constants.CHUNK_X_RANDOM + chunkIndex.y * Constants.CHUNK_Y_RANDOM;
            var lSystemService = new LSystemService(seed);
            var axiom = lSystemService.SelectRandomAxiom();

            float spacingModifier = 3.75f;
            float jitterRange = 150f;
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

            _databaseService.Insert(new RoadChunkData
            {
                ChunkX = chunkIndex.x,
                ChunkY = chunkIndex.y,
                RoadEndPositions = roadEndPositionsString
            });
        }
    }
}
