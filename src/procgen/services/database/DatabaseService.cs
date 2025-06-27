using SQLite;
using Godot;
using LayerProcGenExampleProject.Services.Database.Entities;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;

namespace LayerProcGenExampleProject.Services.Database
{
    // DatabaseService is a singleton class that manages a shared SQLite connection.
    // It ensures that only one connection is used across the application,
    // and it handles the reference counting to dispose of the connection when no longer needed.
    public class DatabaseService : IDisposable
    {
        private static readonly object _lock = new();
        private static SQLiteConnection _sharedConnection;
        private static int _referenceCount = 0;

        // Stores data to /Users/<current_user>/Library/Application\ Support/Godot/app_userdata/LayerProcGenExampleProject/db/LSystemVillageChunk.db
        public DatabaseService(string databaseFileName = "LSystemVillageChunk.db")
        {
            lock (_lock)
            {
                if (_sharedConnection == null)
                {
                    var dbFolderPath = OS.GetUserDataDir().PathJoin("db");
                    if (!Directory.Exists(dbFolderPath))
                        Directory.CreateDirectory(dbFolderPath);

                    var dbPath = dbFolderPath.PathJoin(databaseFileName);

                    var flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.FullMutex;

                    _sharedConnection = new SQLiteConnection(dbPath, flags);
                    
                    try
                    {
                        _sharedConnection.CreateTable<RoadChunkData>();
                    }
                    catch (SQLiteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
                    {
                        // Handle migration: existing database has duplicate data that conflicts with new unique constraint
                        GD.PrintErr($"[DB] Database migration required: {ex.Message}");
                        GD.Print("[DB] Clearing existing data to apply new unique constraint...");
                        
                        // Drop and recreate the table to handle the schema change
                        _sharedConnection.DropTable<RoadChunkData>();
                        _sharedConnection.CreateTable<RoadChunkData>();
                        
                        GD.Print("[DB] Database migration completed successfully.");
                    }
                }

                _referenceCount++;
            }
        }

        public void ClearAllData()
        {
            lock (_lock)
            {
                // Wrap everything in one transaction – faster & keeps the DB consistent.
                _sharedConnection.RunInTransaction(() =>
                {
                    var tableNames = _sharedConnection
                        .Query<Scm>("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';")
                        .Select(r => r.Name);

                    foreach (var name in tableNames)
                        _sharedConnection.Execute($"DELETE FROM \"{name}\";");

                    GD.Print($"[DB] Cleared data in {tableNames.Count()} table(s).");
                });
            }
        }

        // Lightweight record to read sqlite_master rows
        private class Scm { public string Name { get; set; } }

        public void Insert<T>(T entity)
        {
            lock (_lock)
            {
                _sharedConnection.Insert(entity);
            }
        }

        public void InsertOrReplace<T>(T entity)
        {
            lock (_lock)
            {
                _sharedConnection.InsertOrReplace(entity);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _referenceCount--;
                if (_referenceCount <= 0)
                {
                    _sharedConnection?.Dispose();
                    _sharedConnection = null;
                }
            }
        }

        public TableQuery<T> Table<T>() where T : new()
        {
            lock (_lock)
            {
                return _sharedConnection.Table<T>();
            }
        }

        public void DeleteRoadChunk(Runevision.Common.Point chunkIndex)
        {
            lock (_lock)
            {
                // Use Execute with parameters to safely delete the specific chunk's data.
                // This targets the RoadChunkData table implicitly via its mapping.
                _sharedConnection.Execute("DELETE FROM RoadChunkData WHERE ChunkX = ? AND ChunkY = ?", chunkIndex.x, chunkIndex.y);
            }
        }

        public bool ChunkDataExists(Runevision.Common.Point chunkIndex)
        {
            lock (_lock)
            {
                var count = _sharedConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM RoadChunkData WHERE ChunkX = ? AND ChunkY = ?", chunkIndex.x, chunkIndex.y);
                return count > 0;
            }
        }

        // Retrieves road end pairs from adjacent hamlets,
        // which were built in different LSystemVillageChunks.
        // This is useful for road generation, as it allows us to connect roads
        // between hamlets that are in different chunks.
        public List<((int, int) a, (int, int) b, string aJson, string bJson)> RetrieveAdjacentRoadEndPairs()
        {
            lock (_lock)
            {
                var allChunks = _sharedConnection.Table<RoadChunkData>().ToList();
                GD.Print($"[DB DEBUG] Total chunks retrieved: {allChunks.Count}");
                
                foreach (var chunk in allChunks)
                {
                    GD.Print($"[DB DEBUG] Chunk ({chunk.ChunkX}, {chunk.ChunkY}) has road data: {chunk.RoadEndPositions?.Length ?? 0} chars");
                    if (!string.IsNullOrEmpty(chunk.RoadEndPositions))
                    {
                        GD.Print($"[DB DEBUG] Chunk ({chunk.ChunkX}, {chunk.ChunkY}) road data preview: {chunk.RoadEndPositions.Substring(0, Math.Min(100, chunk.RoadEndPositions.Length))}...");
                    }
                }

                // Handle potential duplicate chunks by taking the first occurrence of each coordinate pair
                // This prevents the "duplicate key" exception when multiple threads create the same chunk
                var chunkDict = allChunks
                    .GroupBy(c => (c.ChunkX, c.ChunkY))
                    .ToDictionary(g => g.Key, g => g.First());
                
                GD.Print($"[DB DEBUG] Unique chunks after deduplication: {chunkDict.Count}");

                var result = new List<((int, int), (int, int), string, string)>();

                foreach (var chunk in allChunks)
                {
                    var coord = (chunk.ChunkX, chunk.ChunkY);
                    GD.Print($"[DB DEBUG] Checking chunk {coord} for adjacencies...");

                    // Check right neighbor (x+1, y)
                    var right = (chunk.ChunkX + 1, chunk.ChunkY);
                    if (chunkDict.TryGetValue(right, out var rightChunk))
                    {
                        GD.Print($"[DB DEBUG] Found right neighbor: {coord} -> {right}");
                        result.Add((coord, right, chunk.RoadEndPositions, rightChunk.RoadEndPositions));
                    }
                    else
                    {
                        GD.Print($"[DB DEBUG] No right neighbor found for {coord} (looking for {right})");
                    }

                    // Check top neighbor (x, y+1)
                    var up = (chunk.ChunkX, chunk.ChunkY + 1);
                    if (chunkDict.TryGetValue(up, out var upChunk))
                    {
                        GD.Print($"[DB DEBUG] Found top neighbor: {coord} -> {up}");
                        result.Add((coord, up, chunk.RoadEndPositions, upChunk.RoadEndPositions));
                    }
                    else
                    {
                        GD.Print($"[DB DEBUG] No top neighbor found for {coord} (looking for {up})");
                    }
                }

                GD.Print($"[DB] Retrieved {result.Count} adjacent road end pairs.");

                return result;
            }
        }
    }
}
