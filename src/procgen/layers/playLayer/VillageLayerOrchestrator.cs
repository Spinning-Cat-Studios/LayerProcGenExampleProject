using System;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;
using LayerProcGenExampleProject.Services;

namespace LayerProcGenExampleProject.ProcGen.Layers.PlayLayerComponents
{
    /// <summary>
    /// VillageLayerOrchestrator manages village chunk generation using a dual-radius approach
    /// to decouple data generation from rendering, solving the nearest neighbor topology problem.
    /// 
    /// Dual-Radius System:
    /// - Data Generation Radius (300f): Larger radius where village data (including road endpoints)
    ///   is generated and persisted to database without creating visual nodes
    /// - Rendering Radius (150f): Smaller radius where visual nodes (houses, roads) are created
    /// 
    /// This ensures road topology stability because road endpoint data is always available
    /// from a wider area for consistent nearest neighbor calculations, even when the player
    /// moves and the rendering boundary shifts.
    /// </summary>
    public class VillageLayerOrchestrator
    {
        private readonly PlayLayer _playLayer;
        private readonly PlayerPositionManager _playerManager;
        private bool _isVillageLayerChunksDone = false;
        private int _lastKnownChunkCount = 0;

        public VillageLayerOrchestrator(PlayLayer playLayer, PlayerPositionManager playerManager)
        {
            _playLayer = playLayer;
            _playerManager = playerManager;
        }

        public void SetupLSystemVillageLayerWithLazyLoading(LayerArgumentDictionary layerArguments, Vector3 playerPosition)
        {
            var villageLayer = LSystemVillageLayer.GetInstance(layerArguments);

            // Perform initial lazy load for the village layer
            PerformInitialLazyLoadForVillageLayer(villageLayer, playerPosition);

            _playLayer.AddLayerDependency(new LayerDependency(
                villageLayer,
                256,
                256,
                villageLayer.GetLevelCount() - 1,
                (bounds, level, levelData) => VillageChunkDoneCallback(bounds, level, levelData, villageLayer)
            ));
        }

        public void SetupLSystemVillageLayerDefault(LayerArgumentDictionary layerArguments)
        {
            var villageLayer = LSystemVillageLayer.GetInstance(layerArguments);

            _playLayer.AddLayerDependency(new LayerDependency(
                villageLayer,
                256,
                256,
                villageLayer.GetLevelCount() - 1,
                (bounds, level, levelData) => VillageChunkDoneCallback(bounds, level, levelData, villageLayer)
            ));
        }

        private void PerformInitialLazyLoadForVillageLayer(LSystemVillageLayer villageLayer, Vector3 playerPosition)
        {
            var villageService = villageLayer.GetService() as VillageService;

            // First pass: Generate data for larger radius (without visual nodes)
            GenerateDataInRadius(villageLayer, villageService, playerPosition, 
                PlayLayerConfiguration.VILLAGE_DATA_GENERATION_DISTANCE);

            // Second pass: Handle visual rendering for smaller radius
            bool ShouldCreateChunk(Point chunkIndex, int level, Vector3 playerPos)
            {
                var chunkWorldPos = new Vector3(
                    chunkIndex.x * villageLayer.chunkW + villageLayer.chunkW / 2,
                    0,
                    chunkIndex.y * villageLayer.chunkH + villageLayer.chunkH / 2
                );

                var playerXZPos = new Vector3(playerPos.X, 0, playerPos.Z);
                var distanceToPlayer = playerXZPos.DistanceTo(chunkWorldPos);
                bool withinRange = distanceToPlayer <= PlayLayerConfiguration.VILLAGE_RENDERING_DISTANCE;

                return withinRange;
            }

            var initialBounds = GetBoundsAroundPosition(playerPosition, PlayLayerConfiguration.VILLAGE_RENDERING_DISTANCE * 1.2f);
            ChunkLevelData levelData = ObjectPool<ChunkLevelData>.GlobalGet();

            // GD.Print($"[Initial Load] Loading Village visual chunks around player at {playerPosition}");
            try
            {
                villageLayer.EnsureLoadedInBounds(initialBounds, 0, levelData, playerPosition, ShouldCreateChunk);
            }
            finally
            {
                ObjectPool<ChunkLevelData>.GlobalReturn(ref levelData);
            }
        }

        public void UpdateVillageLayerBasedOnCameraMovement(
            LSystemVillageLayer villageLayer,
            Vector3 referencePosition)
        {
            // Use dual-radius approach: larger radius for data generation, smaller for rendering
            // This prevents the "nearest neighbor" topology instability problem
            var villageService = villageLayer.GetService() as VillageService;

            // First pass: Generate data for larger radius (without visual nodes)
            // This ensures road endpoint data is available for a wider area
            GenerateDataInRadius(villageLayer, villageService, referencePosition, 
                PlayLayerConfiguration.VILLAGE_DATA_GENERATION_DISTANCE);

            // Second pass: Handle visual rendering for smaller radius
            // This only creates visual nodes within the closer range for performance
            RenderChunksInRadius(villageLayer, referencePosition, 
                PlayLayerConfiguration.VILLAGE_RENDERING_DISTANCE);
        }

        private void GenerateDataInRadius(LSystemVillageLayer villageLayer, VillageService villageService, 
            Vector3 referencePosition, float radius)
        {
            if (villageService == null) return;

            // Calculate chunks that should have data generated based on chunk indices
            int centerChunkX = (int)(referencePosition.X / villageLayer.chunkW);
            int centerChunkZ = (int)(referencePosition.Z / villageLayer.chunkH);
            int radiusInChunks = (int)Math.Ceiling(radius / Math.Min(villageLayer.chunkW, villageLayer.chunkH));
            
            // Ensure minimum chunk coverage by expanding radius if necessary
            int minRadiusInChunks = 2; // Minimum 5x5 grid for adjacencies
            radiusInChunks = Math.Max(radiusInChunks, minRadiusInChunks);
            
            GD.Print($"[Dual-Radius DEBUG] Reference position: {referencePosition}");
            GD.Print($"[Dual-Radius DEBUG] Center chunk: ({centerChunkX}, {centerChunkZ})");
            GD.Print($"[Dual-Radius DEBUG] Chunk size: {villageLayer.chunkW}x{villageLayer.chunkH}");
            GD.Print($"[Dual-Radius DEBUG] Radius: {radius}, radiusInChunks: {radiusInChunks} (min: {minRadiusInChunks})");
            
            int dataChunksGenerated = 0;
            int dataChunksSkipped = 0;
            int totalTargetChunks = 0;
            
            for (int x = centerChunkX - radiusInChunks; x <= centerChunkX + radiusInChunks; x++)
            {
                for (int z = centerChunkZ - radiusInChunks; z <= centerChunkZ + radiusInChunks; z++)
                {
                    var chunkIndex = new Point(x, z);
                    var chunkWorldPos = new Vector3(
                        chunkIndex.x * villageLayer.chunkW + villageLayer.chunkW / 2,
                        0,
                        chunkIndex.y * villageLayer.chunkH + villageLayer.chunkH / 2
                    );

                    var playerPosXZ = new Vector3(referencePosition.X, 0, referencePosition.Z);
                    var distanceToChunk = playerPosXZ.DistanceTo(chunkWorldPos);
                    
                    if (distanceToChunk <= radius)
                    {
                        totalTargetChunks++;
                        GD.Print($"[Dual-Radius DEBUG] Chunk ({x}, {z}) at world pos {chunkWorldPos}, distance {distanceToChunk:F1} <= {radius} - INCLUDED");
                        
                        // Generate data only if it doesn't exist
                        if (!villageService.ChunkDataExists(chunkIndex))
                        {
                            villageService.GenerateChunkDataOnly(chunkIndex, villageLayer);
                            dataChunksGenerated++;
                            GD.Print($"[Dual-Radius DEBUG] Generated new data for chunk ({x}, {z})");
                        }
                        else
                        {
                            dataChunksSkipped++;
                            GD.Print($"[Dual-Radius DEBUG] Skipped existing data for chunk ({x}, {z})");
                        }
                    }
                    else
                    {
                        GD.Print($"[Dual-Radius DEBUG] Chunk ({x}, {z}) at world pos {chunkWorldPos}, distance {distanceToChunk:F1} > {radius} - EXCLUDED");
                    }
                }
            }
            
            if (dataChunksGenerated > 0 || dataChunksSkipped > 0)
            {
                GD.Print($"[Dual-Radius] Data generation: {dataChunksGenerated} new, {dataChunksSkipped} existing chunks (radius: {radius:F0})");
                GD.Print($"[Dual-Radius] Total target chunks in radius: {totalTargetChunks}");
                
                // Check total chunks in database to see if we have enough for adjacencies
                var totalChunksInDb = villageService.GetAllChunks().Count;
                GD.Print($"[Dual-Radius] Total chunks in database: {totalChunksInDb}");
                
                // Only trigger road generation if we have multiple chunks and new chunks were added
                // This prevents duplicate signals while ensuring we generate roads when new adjacencies are possible
                if (totalChunksInDb >= 2 && dataChunksGenerated > 0 && totalChunksInDb > _lastKnownChunkCount)
                {
                    GD.Print($"[Dual-Radius] New chunks detected ({_lastKnownChunkCount} -> {totalChunksInDb}), triggering road generation between hamlets");
                    _lastKnownChunkCount = totalChunksInDb;
                    Callable.From(() => {
                        SignalBus.Instance.CallDeferred(
                            "emit_signal",
                            SignalBus.SignalName.AllLSystemVillageChunksGenerated
                        );
                    }).CallDeferred();
                }
                else if (totalChunksInDb < 2)
                {
                    GD.Print($"[Dual-Radius] Only {totalChunksInDb} chunk(s) available, waiting for more chunks before triggering road generation");
                }
                else if ((dataChunksGenerated + dataChunksSkipped) < totalTargetChunks)
                {
                    GD.Print($"[Dual-Radius] Still processing chunks in radius ({dataChunksGenerated + dataChunksSkipped}/{totalTargetChunks}), waiting for completion");
                }
            }
        }

        private void RenderChunksInRadius(LSystemVillageLayer villageLayer, Vector3 referencePosition, float radius)
        {
            bool ShouldCreateChunk(Point chunkIndex, int level, Vector3 playerPos)
            {
                var chunkWorldPos = new Vector3(
                    chunkIndex.x * villageLayer.chunkW + villageLayer.chunkW / 2,
                    0,
                    chunkIndex.y * villageLayer.chunkH + villageLayer.chunkH / 2
                );

                var playerPosXZ = new Vector3(playerPos.X, 0, playerPos.Z);
                return playerPosXZ.DistanceTo(chunkWorldPos) <= radius;
            }

            var bounds = GetBoundsAroundPosition(referencePosition, radius * 1.5f);
            ChunkLevelData levelData = ObjectPool<ChunkLevelData>.GlobalGet();

            GD.Print($"[Dual-Radius] Visual rendering for chunks within {radius:F0} units of {referencePosition}");
            try
            {
                villageLayer.EnsureLoadedInBounds(bounds, 0, levelData, referencePosition, ShouldCreateChunk);
            }
            finally
            {
                ObjectPool<ChunkLevelData>.GlobalReturn(ref levelData);
            }
        }

        private void VillageChunkDoneCallback(GridBounds bounds, int level, ChunkLevelData levelData, LSystemVillageLayer villageLayer)
        {
            if (_isVillageLayerChunksDone)
            {
                return;
            }

            void Handler()
            {
                if (!LandscapeChunkCounterBlackboard.LandscapeChunksAreReady)
                {
                    return;
                }
                villageLayer.EnsureLoadedInBounds(bounds, level, levelData);
                _isVillageLayerChunksDone = true;
                GD.Print("LSystemVillageLayer dependency loaded after LandscapeChunksReady signal.");
            }

            SignalBus.Instance.LandscapeChunksReady += Handler;

            if (LandscapeChunkCounterBlackboard.LandscapeChunksAreReady)
            {
                Handler();
            }
        }

        private static GridBounds GetBoundsAroundPosition(Vector3 position, float range)
        {
            return new GridBounds(
                (int)(position.X - range),
                (int)(position.Z - range),
                (int)(range * 2),
                (int)(range * 2)
            );
        }
    }
}
