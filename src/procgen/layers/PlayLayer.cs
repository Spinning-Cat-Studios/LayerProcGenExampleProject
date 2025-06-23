using System.Reflection;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;
using System;
using Godot.Collections;
using LayerProcGenExampleProject.ProcGen.Layers.PlayLayerComponents;

public class PlayLayer : ChunkBasedDataLayer<PlayLayer, PlayChunk, LayerService>, ILayerWithArguments
{
    public override int chunkW => PlayLayerConfiguration.CHUNK_WIDTH;
    public override int chunkH => PlayLayerConfiguration.CHUNK_HEIGHT;

    private readonly PlayerPositionManager _playerManager;
    private readonly LandscapeLayerOrchestrator _landscapeOrchestrator;
    private readonly VillageLayerOrchestrator _villageOrchestrator;
    private readonly LazyEvaluationHandler _lazyEvaluationHandler;
    private LayerArgumentDictionary _pendingLayerArguments;
    private bool _playerSpawnHandlerSubscribed = false;

    public PlayLayer()
    {
        GD.Print($"[PlayLayer] Default constructor called - Instance ID: {GetHashCode()}");
        _playerManager = new PlayerPositionManager();
        _landscapeOrchestrator = new LandscapeLayerOrchestrator(this, _playerManager);
        _villageOrchestrator = new VillageLayerOrchestrator(this, _playerManager);
        _lazyEvaluationHandler = new LazyEvaluationHandler(
            this,
            _playerManager,
            _landscapeOrchestrator,
            _villageOrchestrator);
    }

    public PlayLayer(LayerArgumentDictionary layerArguments) : this()
    {
        GD.Print($"PlayLayer created with arguments: {layerArguments} - Instance ID: {GetHashCode()}");

        InitializePlayLayer(layerArguments);
    }

    private void InitializePlayLayer(LayerArgumentDictionary layerArguments)
    {
        TerrainBlackboard.Initialize(new NodePath("Controller/TerrainLODManager/Terrain3D"));

        // Subscribe to PlayerSpawn signal immediately
        if (!_playerSpawnHandlerSubscribed)
        {
            SignalBus.Instance.PlayerSpawn += OnPlayerSpawn;
            _playerSpawnHandlerSubscribed = true;
            GD.Print("[PlayLayer] Subscribed to PlayerSpawn signal");
        }

        _playerManager.InitializeFromArguments(layerArguments);

        // Store the arguments for later use when PlayerSpawn signal is received
        _pendingLayerArguments = layerArguments;

        // Callable.From(() => CheckForExistingPlayer()).CallDeferred();
        
        Callable.From(_lazyEvaluationHandler.HookSignalsDeferred).CallDeferred();
    }
    
    // private void CheckForExistingPlayer()
    // {
    //     if (_pendingLayerArguments != null) // Only if we haven't processed the spawn yet
    //     {
    //         try
    //         {
    //             var sceneTree = Engine.GetMainLoop() as SceneTree;
    //             if (sceneTree?.CurrentScene != null)
    //             {
    //                 // Look for FreeLookCamera or any camera
    //                 var cameraNode = sceneTree.CurrentScene.FindChild("*Camera*", true, false) as Node3D;
    //                 if (cameraNode != null)
    //                 {
    //                     GD.Print($"[PlayLayer] Found existing camera/player at: {cameraNode.GlobalPosition}");
    //                     OnPlayerSpawn(cameraNode.GlobalPosition);
    //                     return;
    //                 }
    //             }
    //         }
    //         catch (System.Exception e)
    //         {
    //             GD.Print($"[PlayLayer] Error checking for existing player: {e.Message}");
    //         }
            
    //         GD.Print("[PlayLayer] No existing camera found, waiting for signal");
    //     }
    // }

    private void OnPlayerSpawn(Vector3 playerPosition)
    {
        if (_pendingLayerArguments == null)
        {
            GD.Print("[PlayLayer] PlayerSpawn received but no pending layer arguments");
            return;
        }

        GD.Print($"[PlayLayer] PlayerSpawn received at position: {playerPosition}");

        // Update player manager with the actual spawned position
        _playerManager.UpdatePlayerPosition(playerPosition);

        GD.Print($"[PlayLayer] Initial player position: {_playerManager.LastKnownPlayerPosition}");
        _landscapeOrchestrator.SetupLandscapeLayersWithLazyLoading(_pendingLayerArguments, _playerManager.LastKnownPlayerPosition);
        _villageOrchestrator.SetupLSystemVillageLayerWithLazyLoading(_pendingLayerArguments, _playerManager.LastKnownPlayerPosition);

        // Clear pending arguments as they've been processed
        _pendingLayerArguments = null;

        // Unsubscribe from PlayerSpawn signal as we only need it once
        SignalBus.Instance.PlayerSpawn -= OnPlayerSpawn;
        _playerSpawnHandlerSubscribed = false;

        GD.Print("[PlayLayer] Layer setup completed based on PlayerSpawn signal");
    }
}
