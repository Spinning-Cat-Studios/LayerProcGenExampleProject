using LayerProcGenExampleProject.Services.SQLite;
using LayerProcGenExampleProject.Services.SQLite.Entities;
using System.Collections.Generic;
using Godot;
using Runevision.LayerProcGen;
using System;
using System.Linq;

namespace LayerProcGenExampleProject.Services
{
    public class VillageService : LayerService
    {
        const int GLOBAL_SEED = 12345; // TODO: make this configurable and/or random but stored in the database when finally hook this up to a backend.
        const int CHUNK_X_RANDOM = 73856093;
        const int CHUNK_Y_RANDOM = 19349663;
        const int LSYSTEM_ITERATIONS = 5;

        private readonly SQLiteService _sqliteService;
        private readonly TurtleInterpreterService _turtleInterpreterService;
        private readonly RoadPainterService _roadPainterService;

        private bool _subscribed;

        private void HookSignalsDeferred()
        {
            if (_subscribed) return;

            SignalBus.Instance.AllLSystemVillageChunksGenerated
                += OnAllLSystemVillageChunksGenerated;
            SignalBus.Instance.RoadsGenerated
                += OnRoadsGenerated;
            _subscribed = true;
        }

        public void Dispose()
        {
            if (!_subscribed) return;

            SignalBus.Instance.AllLSystemVillageChunksGenerated
                -= OnAllLSystemVillageChunksGenerated;
            SignalBus.Instance.RoadsGenerated
                -= OnRoadsGenerated;
            _subscribed = false;
        }

        public void SetTerrain(NodePath path)
        {
            // Set the terrain path in the RoadPainter service or any other relevant service.
            _roadPainterService.SetTerrain(path);
        }

        public VillageService(
            SQLiteService sqliteService,
            TurtleInterpreterService turtleInterpreterService,
            RoadPainterService roadPainterService)
            : base("Village") // Pass the required layerName to the base constructor
        {
            _sqliteService = sqliteService;
            _turtleInterpreterService = turtleInterpreterService;
            _roadPainterService = roadPainterService;

            Callable.From(HookSignalsDeferred).CallDeferred();
        }

        private void OnAllLSystemVillageChunksGenerated()
        {
            // Handle the event when all L-System village chunks are generated.
            GD.Print("All L-System village chunks have been generated.");
        }

        private void OnRoadsGenerated(
            Vector3[] roadPositions,
            Vector3[] roadDirections,
            int[] roadStartIndices,
            int[] roadEndIndices,
            Vector3 chunkIndex)
        {
            // Handle the event when roads are generated.
            GD.Print("Received RoadsGenerated signal with chunk index: ", chunkIndex);
            _roadPainterService.PaintRoad(roadPositions, roadStartIndices, roadEndIndices);
        }

        public void SaveChunk(LSystemVillageChunk chunk)
        {
            _sqliteService.Insert(chunk);
        }

        public List<RoadChunkData> GetAllChunks()
        {
            return _sqliteService.Table<RoadChunkData>().ToList();
        }

        // ─────────────────────────────────────────────────────────────
        //  STEP 1A - house/road generation
        // ─────────────────────────────────────────────────────────────
        public LSystemResult GenerateVillageData(
            Runevision.Common.Point chunkIndex,
            LSystemVillageLayer layer)
        {
            // (1) l-system
            int seed = GLOBAL_SEED + chunkIndex.x * CHUNK_X_RANDOM + chunkIndex.y * CHUNK_Y_RANDOM;
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
                Iterations = LSYSTEM_ITERATIONS,
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
        ) {
            _sqliteService.Insert(new RoadChunkData
            {
                ChunkX = chunkIndex.x,
                ChunkY = chunkIndex.y,
                RoadEndPositions = roadEnds
            });
        }
    }
}
