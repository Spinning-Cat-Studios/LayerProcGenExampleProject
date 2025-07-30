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
        // GD.Print($"[PlayLayer] Default constructor called - Instance ID: {GetHashCode()}");
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
        // GD.Print($"PlayLayer created with arguments: {layerArguments} - Instance ID: {GetHashCode()}");

        InitializePlayLayer(layerArguments);
    }

    private void InitializePlayLayer(LayerArgumentDictionary layerArguments)
    {
        TerrainBlackboard.Initialize(new NodePath("Controller/TerrainLODManager/Terrain3D"));

        // Defer subscription to ensure the SignalBus singleton is ready.
        Callable.From(SubscribeToPlayerSpawn).CallDeferred();

        _playerManager.InitializeFromArguments(layerArguments);

        // Store the arguments for later use when PlayerSpawn signal is received
        _pendingLayerArguments = layerArguments;

        
        Callable.From(_lazyEvaluationHandler.HookSignalsDeferred).CallDeferred();
    }
    
    private void SubscribeToPlayerSpawn()
    {
        if (_playerSpawnHandlerSubscribed) return;

        // This is the robust way to get a singleton from a non-Node class
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        var signalBus = sceneTree?.Root.GetNode<SignalBus>("SignalBus");

        if (signalBus == null)
        {
            // GD.PrintErr("[PlayLayer] Could not find SignalBus singleton in the scene tree!");
            return;
        }
        
        signalBus.PlayerSpawn += OnPlayerSpawn;
        signalBus.LandscapeChunksReady += OnLandscapeSetupComplete;
        _playerSpawnHandlerSubscribed = true;
        // GD.Print($"[PlayLayer] Subscribed to PlayerSpawn signal on SignalBus instance {signalBus.GetInstanceId()}");
    }
    
    private void OnLandscapeSetupComplete()
    {
        GD.Print("[PlayLayer] Landscape setup complete, proceeding with village layer setup");
        
        if (_pendingLayerArguments == null)
        {
            GD.Print("[PlayLayer] Landscape setup complete but no pending layer arguments");
            return;
        }

        // Use the last known player position for village layer setup
        _villageOrchestrator.SetupLSystemVillageLayerWithLazyLoading(_pendingLayerArguments, _playerManager.LastKnownPlayerPosition);

        // Clear pending arguments as they've been processed
        _pendingLayerArguments = null;

        // Unsubscribe from LandscapeChunksReady signal as we only need it once
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        var signalBus = sceneTree?.Root.GetNode<SignalBus>("SignalBus");
        if (signalBus != null)
        {
            signalBus.LandscapeChunksReady -= OnLandscapeSetupComplete;
        }
    }

    private void OnPlayerSpawn(Vector3 playerPosition)
    {
        // GD.Print($"[PlayLayer] OnPlayerSpawn called with position: {playerPosition}");

        if (_pendingLayerArguments == null)
        {
            GD.Print("[PlayLayer] PlayerSpawn received but no pending layer arguments");
            return;
        }

        // GD.Print($"[PlayLayer] PlayerSpawn received at position: {playerPosition}");

        // Update player manager with the actual spawned position
        _playerManager.UpdatePlayerPosition(playerPosition);

        // GD.Print($"[PlayLayer] Initial player position: {_playerManager.LastKnownPlayerPosition}");
        _landscapeOrchestrator.SetupLandscapeLayersWithLazyLoading(_pendingLayerArguments, _playerManager.LastKnownPlayerPosition);

        // Unsubscribe from PlayerSpawn signal as we only need it once
        // Also get the singleton properly here to unsubscribe
        var sceneTree = Engine.GetMainLoop() as SceneTree;
        var signalBus = sceneTree?.Root.GetNode<SignalBus>("SignalBus");
        if (signalBus != null)
        {
            signalBus.PlayerSpawn -= OnPlayerSpawn;
        }

        _playerSpawnHandlerSubscribed = false;

        GD.Print("[PlayLayer] Layer setup completed based on PlayerSpawn signal");
    }
}
