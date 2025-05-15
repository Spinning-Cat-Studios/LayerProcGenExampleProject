using LayerProcGenExampleProject.Services.SQLite;
using LayerProcGenExampleProject.Services.SQLite.Entities;
using System.Collections.Generic;
using Godot;
using Runevision.LayerProcGen;

namespace LayerProcGenExampleProject.Services
{
    public class VillageService: LayerService
    {
        private readonly DatabaseContext _dbContext;
        private readonly TurtleInterpreterService _turtleInterpreterService;
        private readonly RoadPainterService _roadPainterService;

        public VillageService(
            DatabaseContext dbContext,
            TurtleInterpreterService turtleInterpreterService,
            RoadPainterService roadPainterService)
            : base("Village") // Pass the required layerName to the base constructor
        {
            _dbContext = dbContext;
            _turtleInterpreterService = turtleInterpreterService;
            _roadPainterService = roadPainterService;

            SignalBus.Instance.AllLSystemVillageChunksGenerated += OnAllLSystemVillageChunksGenerated;
        }

        private void OnAllLSystemVillageChunksGenerated()
        {
            // Handle the event when all L-System village chunks are generated.
            GD.Print("All L-System village chunks have been generated.");
        }

        public void SaveChunk(LSystemVillageChunk chunk)
        {
            _dbContext.Insert(chunk);
        }

        public List<RoadChunkData> GetAllChunks()
        {
            return _dbContext.Table<RoadChunkData>().ToList();
        }
    }
}
