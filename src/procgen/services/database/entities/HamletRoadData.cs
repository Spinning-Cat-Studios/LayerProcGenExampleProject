using SQLite;

namespace LayerProcGenExampleProject.Services.Database.Entities
{
    [Table("HamletRoadData")]
    public class HamletRoadData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed("IX_ChunkCoords", 1, Unique = true)]
        public int ChunkX { get; set; }

        [Indexed("IX_ChunkCoords", 2, Unique = true)]
        public int ChunkY { get; set; }

        public string RoadPositionsJson { get; set; }
        public string RoadStartIndicesJson { get; set; }
        public string RoadEndIndicesJson { get; set; }
    }
}
