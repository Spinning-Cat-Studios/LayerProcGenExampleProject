using System;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;
using LayerProcGenExampleProject.ProcGen.Layers.PlayLayerComponents;

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
            // Use the larger data generation distance to ensure road adjacency stability
            float loadDistance = PlayLayerConfiguration.VILLAGE_DATA_GENERATION_DISTANCE;

            bool ShouldCreateChunk(Point chunkIndex, int level, Vector3 playerPos)
            {
                var chunkWorldPos = new Vector3(
                    chunkIndex.x * villageLayer.chunkW + villageLayer.chunkW / 2,
                    0,
                    chunkIndex.y * villageLayer.chunkH + villageLayer.chunkH / 2
                );

                var playerXZPos = new Vector3(playerPos.X, 0, playerPos.Z);
                var distanceToPlayer = playerXZPos.DistanceTo(chunkWorldPos);
                return distanceToPlayer <= loadDistance;
            }

            var initialBounds = GetBoundsAroundPosition(playerPosition, loadDistance * 1.2f);
            ChunkLevelData levelData = ObjectPool<ChunkLevelData>.GlobalGet();

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
            // Use the larger data generation distance to ensure road adjacency stability
            float loadDistance = PlayLayerConfiguration.VILLAGE_DATA_GENERATION_DISTANCE;

            bool ShouldCreateChunk(Point chunkIndex, int level, Vector3 playerPos)
            {
                var chunkWorldPos = new Vector3(
                    chunkIndex.x * villageLayer.chunkW + villageLayer.chunkW / 2,
                    0,
                    chunkIndex.y * villageLayer.chunkH + villageLayer.chunkH / 2
                );

                var playerPosXZ = new Vector3(playerPos.X, 0, playerPos.Z);
                return playerPosXZ.DistanceTo(chunkWorldPos) <= loadDistance;
            }

            var bounds = GetBoundsAroundPosition(referencePosition, loadDistance * 1.5f);
            ChunkLevelData levelData = ObjectPool<ChunkLevelData>.GlobalGet();

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
