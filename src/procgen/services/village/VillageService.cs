using LayerProcGenExampleProject.Data;
using LayerProcGenExampleProject.Data.Entities;
using System.Collections.Generic;
using System.Linq;

namespace LayerProcGenExampleProject.Services
{
    public class VillageService
    {
        private readonly DatabaseContext _dbContext;

        public VillageService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
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
