using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;

namespace LayerProcGenExampleProject.ProcGen.Layers.PlayLayerComponents
{
    public class LazyEvaluationHandler
    {
        private readonly PlayLayer _playLayer;
        private readonly PlayerPositionManager _playerManager;
        private readonly LandscapeLayerOrchestrator _landscapeOrchestrator;
        private bool _subscribed;

        public LazyEvaluationHandler(
            PlayLayer playLayer,
            PlayerPositionManager playerManager,
            LandscapeLayerOrchestrator landscapeOrchestrator)
        {
            _playLayer = playLayer;
            _playerManager = playerManager;
            _landscapeOrchestrator = landscapeOrchestrator;
        }

        public void HookSignalsDeferred()
        {
            if (_subscribed) return;

            SignalBus.Instance.ReconstructNodes += OnReconstructNodes;
            _subscribed = true;

            GD.Print("[LazyEvaluationHandler] Subscribed to ReconstructNodes signal");
        }

        public void OnReconstructNodes(Vector3 checkpointPos, Vector3 cameraPos, float distance)
        {
            // GD.Print($"[LazyEvaluationHandler] ReconstructNodes received: distance={distance:F1}");

            if (distance < PlayLayerConfiguration.CHECKPOINT_DIST_DELTA_THRESHOLD)
            {
                GD.Print("[LazyEvaluationHandler] Distance too small, skipping reconstruction");
                return;
            }

            Vector3 referencePosition = (_playerManager.CachedPlayerNode != null)
                ? _playerManager.GetCurrentPlayerPosition()
                : cameraPos;

            // Handle PlayLayer chunks
            // HandlePlayLayerReconstruction(referencePosition);  // This is an expensive operation, commenting out for now

            // Handle Landscape layer chunks
            HandleLandscapeLayerReconstruction(referencePosition);
        }

        // This is an expensive operation and should be used with caution.
        // It is currently commented out to prevent performance issues during gameplay.
        // private void HandlePlayLayerReconstruction(Vector3 referencePosition)
        // {
        //     // GD.Print($"[LazyEvaluationHandler] Reconstructing PlayLayer chunks around {referencePosition}");

        //     ChunkLevelData levelData = ObjectPool<ChunkLevelData>.GlobalGet();

        //     try
        //     {
        //         var bounds = GetBoundsAroundPosition(referencePosition, 100f);
        //         _playLayer.EnsureLoadedInBounds(bounds, 0, levelData, referencePosition, ShouldCreatePlayChunk);
        //     }
        //     finally
        //     {
        //         ObjectPool<ChunkLevelData>.GlobalReturn(ref levelData);
        //     }
        // }

        private void HandleLandscapeLayerReconstruction(Vector3 referencePosition)
        {
            GD.Print($"[LazyEvaluationHandler] Reconstructing LandscapeLayer chunks around {referencePosition}");
            _playLayer.HandleDependenciesForLevel(0, dependency =>
            {
                var layer = dependency.layer;
                if (layer.GetType().Name.StartsWith("LandscapeLayer"))
                {
                    _landscapeOrchestrator.UpdateLayerBasedOnCameraMovement(layer, referencePosition, dependency);
                }
            });
        }

        // private bool ShouldCreatePlayChunk(Point chunkIndex, int level, Vector3 playerPosition)
        // {
        //     var chunkWorldPos = new Vector3(
        //         chunkIndex.x * PlayLayerConfiguration.CHUNK_WIDTH + PlayLayerConfiguration.CHUNK_WIDTH / 2,
        //         0,
        //         chunkIndex.y * PlayLayerConfiguration.CHUNK_HEIGHT + PlayLayerConfiguration.CHUNK_HEIGHT / 2
        //     );

        //     var distance = playerPosition.DistanceTo(chunkWorldPos);
        //     return distance <= PlayLayerConfiguration.PLAY_LAYER_LOAD_DISTANCE;
        // }

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
