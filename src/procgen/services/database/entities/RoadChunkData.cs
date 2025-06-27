using SQLite;
using Godot;
using System.Collections.Generic;
using System.Text.Json;

namespace LayerProcGenExampleProject.Services.Database.Entities
{
    // This class represents a chunk of road data in the SQLite database.
    // It includes the chunk's coordinates and a list of road end positions.
    // The road end positions are serialized to JSON for storage.
    [Table("RoadChunkData")]
    public class RoadChunkData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed("IX_ChunkCoords", 1, Unique = true)]
        public int ChunkX { get; set; }
        
        [Indexed("IX_ChunkCoords", 2, Unique = true)]
        public int ChunkY { get; set; }

        // SQLite does not directly store Vector3, so serialize as JSON
        public string RoadEndPositions { get; set; }

        // [Ignore]
        // public List<Vector3> RoadEndPositions
        // {
        //     get => JsonSerializer.Deserialize<List<Vector3>>(RoadEndPositionsJson);
        //     set => RoadEndPositionsJson = JsonSerializer.Serialize(value);
        // }
    }
}

