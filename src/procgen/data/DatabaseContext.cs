using SQLite;
using Godot;
using LayerProcGenExampleProject.Data.Entities;
using System.IO;
using System;

namespace LayerProcGenExampleProject.Data
{
    // DatabaseContext is a singleton class that manages a shared SQLite connection.
    // It ensures that only one connection is used across the application,
    // and it handles the reference counting to dispose of the connection when no longer needed.
    public class DatabaseContext : IDisposable
    {
        private static readonly object _lock = new();
        private static SQLiteConnection _sharedConnection;
        private static int _referenceCount = 0;

        // Stores data to /Users/<current_user>/Library/Application\ Support/Godot/app_userdata/LayerProcGenExampleProject/db/LSystemVillageChunk.db
        public DatabaseContext(string databaseFileName = "LSystemVillageChunk.db")
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
    }
}
