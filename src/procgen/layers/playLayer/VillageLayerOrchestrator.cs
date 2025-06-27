using System;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;
using LayerProcGenExampleProject.Services;

namespace LayerProcGenExampleProject.ProcGen.Layers.PlayLayerComponents
{
    public class VillageLayerOrchestrator
    {
        private readonly PlayLayer _playLayer;
        private readonly PlayerPositionManager _playerManager;
        private bool _isVillageLayerChunksDone = false;

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
            var villageService = villageLayer.GetService() as VillageService;

            // First pass: Generate data for larger radius (without visual nodes)
            GenerateDataInRadius(villageLayer, villageService, referencePosition, 
                PlayLayerConfiguration.VILLAGE_DATA_GENERATION_DISTANCE);

            // Second pass: Handle visual rendering for smaller radius
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
                    if (playerPosXZ.DistanceTo(chunkWorldPos) <= radius)
                    {
                        // Generate data only if it doesn't exist
                        villageService.GenerateChunkDataOnly(chunkIndex, villageLayer);
                    }
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

            // GD.Print($"[Camera Movement] Updating Village visual chunks around camera at {referencePosition}");
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
