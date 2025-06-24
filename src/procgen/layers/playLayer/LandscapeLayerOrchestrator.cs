using System;
using System.Reflection;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;
using Godot.Collections;

namespace LayerProcGenExampleProject.ProcGen.Layers.PlayLayerComponents
{
    public class LandscapeLayerOrchestrator
    {
        private readonly PlayLayer _playLayer;
        private readonly PlayerPositionManager _playerManager;

        public LandscapeLayerOrchestrator(PlayLayer playLayer, PlayerPositionManager playerManager)
        {
            _playLayer = playLayer;
            _playerManager = playerManager;
        }

        public void SetupLandscapeLayersWithLazyLoading(LayerArgumentDictionary layerArguments, Vector3 playerPosition)
        {
            foreach (var config in PlayLayerConfiguration.LandscapeLayerConfigs)
            {
                ConstructLandscapeLayerWithLazyLoading(
                    layerArguments,
                    config.Type,
                    config.Width,
                    config.Height,
                    config.Subtype,
                    playerPosition,
                    config.LoadDistance
                );
            }
        }

        public void SetupLandscapeLayersDefault(LayerArgumentDictionary layerArguments)
        {
            foreach (var config in PlayLayerConfiguration.LandscapeLayerConfigs)
            {
                ConstructLandscapeLayerDependency(layerArguments, config.Type, config.Width, config.Height, config.Subtype);
            }
        }

        private void ConstructLandscapeLayerWithLazyLoading(
            LayerArgumentDictionary layerArguments,
            Type landscapeLayerType,
            int width,
            int height,
            string subtype,
            Vector3 playerPosition,
            float loadDistance)
        {
            var landscapeLayerInstance = CreateLandscapeLayerInstance(layerArguments, landscapeLayerType, subtype);
            PerformInitialLazyLoadForLayer(landscapeLayerInstance, playerPosition, loadDistance);
            _playLayer.AddLayerDependency(new LayerDependency(landscapeLayerInstance, width, height));
        }

        private void ConstructLandscapeLayerDependency(
            LayerArgumentDictionary layerArguments,
            Type landscapeLayerType,
            int width,
            int height,
            string subtype)
        {
            var landscapeLayerInstance = CreateLandscapeLayerInstance(layerArguments, landscapeLayerType, subtype);
            _playLayer.AddLayerDependency(new LayerDependency(landscapeLayerInstance, width, height));
        }

        private AbstractChunkBasedDataLayer CreateLandscapeLayerInstance(
            LayerArgumentDictionary layerArguments,
            Type landscapeLayerType,
            string subtype)
        {
            var landscapeLayerArgs = layerArguments.Clone();
            landscapeLayerArgs.parameters["landscape_layer_id"] = new Dictionary<string, Variant>
            {
                { "id", landscapeLayerType.Name }
            };

            var getInstanceMethod = landscapeLayerType.GetMethod(
                "GetInstance",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[] { typeof(LayerArgumentDictionary), typeof(string) },
                null
            );

            if (getInstanceMethod == null)
                throw new InvalidOperationException($"GetInstance(LayerArgumentDictionary, string) not found on {landscapeLayerType.Name}");

            return (AbstractChunkBasedDataLayer)getInstanceMethod.Invoke(
                null,
                new object[] { landscapeLayerArgs, subtype }
            );
        }

        private void PerformInitialLazyLoadForLayer(
            AbstractChunkBasedDataLayer layer,
            Vector3 playerPosition,
            float loadDistance)
        {
            bool ShouldCreateChunk(Point chunkIndex, int level, Vector3 playerPos)
            {
                var chunkWorldPos = new Vector3(
                    chunkIndex.x * layer.chunkW + layer.chunkW / 2,
                    0,
                    chunkIndex.y * layer.chunkH + layer.chunkH / 2
                );

                var playerXZPos = new Vector3(playerPos.X, 0, playerPos.Z);
                var distanceToPlayer = playerXZPos.DistanceTo(chunkWorldPos);
                bool withinRange = distanceToPlayer <= loadDistance;

                if (!withinRange)
                {
                    // GD.Print($"[Initial Load] Skipping {layer.GetType().Name} chunk {chunkIndex} - outside player range (distance: {distanceToPlayer:F1})");
                }

                return withinRange;
            }

            var initialBounds = GetBoundsAroundPosition(playerPosition, loadDistance * 1.2f);
            ChunkLevelData levelData = ObjectPool<ChunkLevelData>.GlobalGet();

            // GD.Print($"[Initial Load] Loading {layer.GetType().Name} chunks around player at {playerPosition}");
            try
            {
                layer.EnsureLoadedInBounds(initialBounds, 0, levelData, playerPosition, ShouldCreateChunk);
            }
            finally
            {
                ObjectPool<ChunkLevelData>.GlobalReturn(ref levelData);
            }
        }

        public void UpdateLayerBasedOnCameraMovement(
            AbstractChunkBasedDataLayer layer,
            Vector3 referencePosition,
            LayerDependency dependency)
        {
            float loadDistance = PlayLayerConfiguration.GetLoadDistanceForLayer(layer.GetType().Name);

            bool ShouldCreateChunk(Point chunkIndex, int level, Vector3 playerPos)
            {
                var chunkWorldPos = new Vector3(
                    chunkIndex.x * layer.chunkW + layer.chunkW / 2,
                    0,
                    chunkIndex.y * layer.chunkH + layer.chunkH / 2
                );

                var playerPosXZ = new Vector3(playerPos.X, 0, playerPos.Z);
                return playerPosXZ.DistanceTo(chunkWorldPos) <= loadDistance;
            }

            var bounds = GetBoundsAroundPosition(referencePosition, loadDistance * 1.5f);
            ChunkLevelData levelData = ObjectPool<ChunkLevelData>.GlobalGet();

            // GD.Print($"[Camera Movement] Updating {layer.GetType().Name} chunks around camera at {referencePosition}");
            try
            {
                layer.EnsureLoadedInBounds(bounds, 0, levelData, referencePosition, ShouldCreateChunk);
            }
            finally
            {
                ObjectPool<ChunkLevelData>.GlobalReturn(ref levelData);
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
