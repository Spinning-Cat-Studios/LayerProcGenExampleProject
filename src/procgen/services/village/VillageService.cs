using LayerProcGenExampleProject.Services.Data;
using LayerProcGenExampleProject.Services.Data.Entities;
using System.Collections.Generic;
using Godot;

namespace LayerProcGenExampleProject.Services
{
    public class VillageService: LayerService<LSystemVillageLayer, LSystemVillageChunk>
    {
        private readonly DatabaseContext _dbContext;
        private readonly TurtleInterpreterService _turtleInterpreterService;
        private readonly LSystemService _lSystemService;

        public VillageService(
            DatabaseContext dbContext,
            TurtleInterpreterService turtleInterpreterService,
            LSystemService lSystemService)
        {
            _dbContext = dbContext;
            _turtleInterpreterService = turtleInterpreterService;
            _lSystemService = lSystemService;

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
