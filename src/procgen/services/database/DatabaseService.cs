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
                    _sharedConnection.CreateTable<RoadChunkData>();
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

        // Retrieves road end pairs from adjacent hamlets,
        // which were built in different LSystemVillageChunks.
        // This is useful for road generation, as it allows us to connect roads
        // between hamlets that are in different chunks.
        public List<((int, int) a, (int, int) b, string aJson, string bJson)> RetrieveAdjacentRoadEndPairs()
        {
            lock (_lock)
            {
                var allChunks = _sharedConnection.Table<RoadChunkData>().ToList();

                var chunkDict = allChunks.ToDictionary(c => (c.ChunkX, c.ChunkY));

                var result = new List<((int, int), (int, int), string, string)>();

                foreach (var chunk in allChunks)
                {
                    var coord = (chunk.ChunkX, chunk.ChunkY);

                    // Check right neighbor (x+1, y)
                    var right = (chunk.ChunkX + 1, chunk.ChunkY);
                    if (chunkDict.TryGetValue(right, out var rightChunk))
                    {
                        result.Add((coord, right, chunk.RoadEndPositions, rightChunk.RoadEndPositions));
                    }

                    // Check top neighbor (x, y+1)
                    var up = (chunk.ChunkX, chunk.ChunkY + 1);
                    if (chunkDict.TryGetValue(up, out var upChunk))
                    {
                        result.Add((coord, up, chunk.RoadEndPositions, upChunk.RoadEndPositions));
                    }
                }

                GD.Print($"[DB] Retrieved {result.Count} adjacent road end pairs.");

                return result;
            }
        }
    }
}
