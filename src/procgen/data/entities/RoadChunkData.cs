using SQLite;
using Godot;
using System.Collections.Generic;
using System.Text.Json;

namespace LayerProcGenExampleProject.Data.Entities
{
    // This class represents a chunk of road data in the SQLite database.
    // It includes the chunk's coordinates and a list of road end positions.
    // The road end positions are serialized to JSON for storage.
    [Table("RoadChunkData")]
    public class RoadChunkData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ChunkX { get; set; }
        public int ChunkY { get; set; }

        // SQLite does not directly store Vector3, so serialize as JSON
        public string RoadEndPositionsJson { get; set; }

        [Ignore]
        public List<Vector3> RoadEndPositions
        {
            get => JsonSerializer.Deserialize<List<Vector3>>(RoadEndPositionsJson);
            set => RoadEndPositionsJson = JsonSerializer.Serialize(value);
        }
    }
}

