using System.Reflection;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;
using System;
using Godot.Collections;

public class PlayLayer : ChunkBasedDataLayer<PlayLayer, PlayChunk, LayerService>, ILayerWithArguments
{
    private Node3D _cachedPlayerNode; // Cache the player node reference
    private Vector3 _lastKnownPlayerPosition; // Cache the position
    
    public override int chunkW => 8;
    public override int chunkH => 8;
    private bool _subscribed;

    public PlayLayer() { }

    public PlayLayer(LayerArgumentDictionary layerArguments)
    {
        GD.Print($"PlayLayer created with arguments: {layerArguments}");
        InitializePlayLayer(layerArguments);
    }

    private void ConstructLandscapeLayerDependency(
        LayerArgumentDictionary layerArguments,
        Type landscapeLayerType,
        int width,
        int height,
        string subtype)
    {
        var landscapeLayerArgs = layerArguments.Clone();
        // Add "landscape_layer_id": "A" through "landscape_layer_id": "D" to the layer arguments
        landscapeLayerArgs.parameters["landscape_layer_id"] = new Dictionary<string, Variant>
        {
            { "id", landscapeLayerType.Name }
        };

        // Find the static GetInstance method with the correct signature
        var getInstanceMethod = landscapeLayerType.GetMethod(
            "GetInstance",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new Type[] { typeof(LayerArgumentDictionary), typeof(string) },
            null
        );
        if (getInstanceMethod == null)
            throw new InvalidOperationException($"GetInstance(LayerArgumentDictionary, string) not found on {landscapeLayerType.Name}");

        // Call the static method
        var landscapeLayerInstance = (AbstractChunkBasedDataLayer)getInstanceMethod.Invoke(
            null,
            new object[] { landscapeLayerArgs, subtype }
        );

        AddLayerDependency(new LayerDependency(landscapeLayerInstance, width, height));
    }

    private Vector3? ExtractPlayerPosition(LayerArgumentDictionary layerArguments)
    {
        Dictionary<string, Variant> playLayerDict;
        if (!layerArguments.parameters.TryGetValue("PlayLayer", out playLayerDict) || playLayerDict == null || !playLayerDict.ContainsKey("PlayerPath"))
            return null;

        string playerPath = playLayerDict["PlayerPath"].AsString();

        // Try different methods to get the player node
        Node3D playerNode = null;

        // Method 1: Try to get from current scene tree
        if (Engine.IsEditorHint())
        {
            // In editor, might need different approach
            GD.Print($"[PlayLayer] Editor mode - cannot resolve player path: {playerPath}");
            return null;
        }

        // Method 2: Try getting from scene tree
        try
        {
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            if (sceneTree?.CurrentScene != null)
            {
                if (playerPath == ".")
                {
                    // Player is likely the current scene or a direct child
                    playerNode = sceneTree.CurrentScene as Node3D;
                    if (playerNode == null)
                    {
                        // Try finding player in children
                        playerNode = sceneTree.CurrentScene.FindChild("Player*", true, false) as Node3D;
                    }
                }
                else
                {
                    playerNode = sceneTree.CurrentScene.GetNode(playerPath) as Node3D;
                }
            }
        }
        catch (Exception e)
        {
            GD.Print($"[PlayLayer] Could not resolve player path '{playerPath}': {e.Message}");
        }

        // Method 3: Try getting from groups
        if (playerNode == null)
        {
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            var playersInGroup = sceneTree?.GetNodesInGroup("player");
            if (playersInGroup?.Count > 0)
            {
                playerNode = playersInGroup[0] as Node3D;
            }
        }

        if (playerNode != null)
        {
            GD.Print($"[PlayLayer] Found player at: {playerNode.GlobalPosition}");
            return playerNode.GlobalPosition;
        }

        GD.Print($"[PlayLayer] Could not find player node for path: {playerPath}");
        return null;
    }

    private void InitializePlayLayer(LayerArgumentDictionary layerArguments)
    {
        TerrainBlackboard.Initialize(new NodePath("Controller/TerrainLODManager/Terrain3D"));

        // Find player once and cache it
        _cachedPlayerNode = FindPlayerNode(layerArguments);

        // Extract player position for initial lazy loading
        Vector3? playerPosition = ExtractPlayerPosition(layerArguments);

        if (playerPosition.HasValue)
        {
            GD.Print($"[PlayLayer] Initial player position: {playerPosition.Value}");
            SetupLandscapeLayersWithLazyLoading(layerArguments, playerPosition.Value);
        }
        else
        {
            GD.Print("[PlayLayer] No player position found, using default loading");
            SetupLandscapeLayersDefault(layerArguments);
        }

        // Village layer with lazy loading as before
        SetupVillageLayer(layerArguments);

        Callable.From(HookSignalsDeferred).CallDeferred();
    }
    
    private void HookSignalsDeferred()
    {
        if (_subscribed) return;

        SignalBus.Instance.ReconstructNodes += OnReconstructNodes;
        _subscribed = true;
        
        GD.Print("[PlayLayer] Subscribed to ReconstructNodes signal");
    }

    public void OnReconstructNodes(Vector3 checkpointPos, Vector3 cameraPos, float distance)
    {
        GD.Print($"[PlayLayer] ReconstructNodes received: distance={distance:F1}");
        
        // Only process if camera moved significantly
        if (distance < 10f) // Adjust threshold as needed
        {
            GD.Print("[PlayLayer] Distance too small, skipping reconstruction");
            return;
        }

        // Get current player position (or use camera as fallback)
        Vector3 referencePosition = (_cachedPlayerNode != null) 
            ? GetCurrentPlayerPosition() 
            : cameraPos;

        GD.Print($"[PlayLayer] Reconstructing PlayLayer chunks around {referencePosition}");

        // Trigger lazy evaluation on PlayLayer itself
        var bounds = GetBoundsAroundPosition(referencePosition, 100f); // 100f load distance for PlayLayer
        EnsureLoadedInBounds(bounds, 0, null, referencePosition, ShouldCreatePlayChunk);
    }

    private bool ShouldCreatePlayChunk(Point chunkIndex, int level, Vector3 playerPosition)
    {
        // Calculate chunk world position (8x8 chunks)
        var chunkWorldPos = new Vector3(
            chunkIndex.x * chunkW + chunkW / 2,  // chunkW = 8
            0,
            chunkIndex.y * chunkH + chunkH / 2   // chunkH = 8
        );

        var distance = playerPosition.DistanceTo(chunkWorldPos);
        var loadDistance = 75f; // Load distance for PlayLayer chunks
        bool withinRange = distance <= loadDistance;

        // if (!withinRange)
        // {
        //     GD.Print($"[PlayLayer] Skipping PlayChunk {chunkIndex} - outside range (distance: {distance:F1})");
        // }
        // else
        // {
        //     GD.Print($"[PlayLayer] Creating PlayChunk {chunkIndex} - within range (distance: {distance:F1})");
        // }

        return withinRange;
    }

    private Node3D FindPlayerNode(LayerArgumentDictionary layerArguments)
    {
        // Your existing ExtractPlayerPosition logic, but return the Node3D instead
        Dictionary<string, Variant> playLayerDict;
        if (!layerArguments.parameters.TryGetValue("PlayLayer", out playLayerDict) || playLayerDict == null || !playLayerDict.ContainsKey("PlayerPath"))
            return null;

        string playerPath = playLayerDict["PlayerPath"].AsString();

        if (Engine.IsEditorHint())
        {
            GD.Print($"[PlayLayer] Editor mode - cannot resolve player path: {playerPath}");
            return null;
        }

        Node3D playerNode = null;
        try
        {
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            if (sceneTree?.CurrentScene != null)
            {
                if (playerPath == ".")
                {
                    playerNode = sceneTree.CurrentScene as Node3D;
                    if (playerNode == null)
                    {
                        playerNode = sceneTree.CurrentScene.FindChild("Player*", true, false) as Node3D;
                    }
                }
                else
                {
                    playerNode = sceneTree.CurrentScene.GetNode(playerPath) as Node3D;
                }
            }
        }
        catch (Exception e)
        {
            GD.Print($"[PlayLayer] Could not resolve player path '{playerPath}': {e.Message}");
        }

        // Try groups as fallback
        if (playerNode == null)
        {
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            var playersInGroup = sceneTree?.GetNodesInGroup("player");
            if (playersInGroup?.Count > 0)
            {
                playerNode = playersInGroup[0] as Node3D;
            }
        }

        return playerNode;
    }

    private void SetupLandscapeLayersWithLazyLoading(LayerArgumentDictionary layerArguments, Vector3 playerPosition)
    {
        // Define distance thresholds for each landscape layer
        var layerConfigs = new[]
        {
            new { Type = typeof(LandscapeLayerD), Width = 2048, Height = 2048, Subtype = "D", LoadDistance = 200f },
            new { Type = typeof(LandscapeLayerC), Width = 1024, Height = 1024, Subtype = "C", LoadDistance = 150f },
            new { Type = typeof(LandscapeLayerB), Width = 512, Height = 512, Subtype = "B", LoadDistance = 100f },
            new { Type = typeof(LandscapeLayerA), Width = 256, Height = 256, Subtype = "A", LoadDistance = 75f }
        };

        foreach (var config in layerConfigs)
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

        // Set up camera movement signal handling for all layers
        SetupCameraMovementHandling(layerArguments);
    }

    private void SetupLandscapeLayersDefault(LayerArgumentDictionary layerArguments)
    {
        // Fallback to original behavior if no player position
        ConstructLandscapeLayerDependency(layerArguments, typeof(LandscapeLayerD), 2048, 2048, "D");
        ConstructLandscapeLayerDependency(layerArguments, typeof(LandscapeLayerC), 1024, 1024, "C");
        ConstructLandscapeLayerDependency(layerArguments, typeof(LandscapeLayerB), 512, 512, "B");
        ConstructLandscapeLayerDependency(layerArguments, typeof(LandscapeLayerA), 256, 256, "A");
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
        var landscapeLayerArgs = layerArguments.Clone();
        landscapeLayerArgs.parameters["landscape_layer_id"] = new Dictionary<string, Variant>
        {
            { "id", landscapeLayerType.Name }
        };

        // Get the landscape layer instance
        var getInstanceMethod = landscapeLayerType.GetMethod(
            "GetInstance",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new Type[] { typeof(LayerArgumentDictionary), typeof(string) },
            null
        );

        if (getInstanceMethod == null)
            throw new InvalidOperationException($"GetInstance(LayerArgumentDictionary, string) not found on {landscapeLayerType.Name}");

        var landscapeLayerInstance = (AbstractChunkBasedDataLayer)getInstanceMethod.Invoke(
            null,
            new object[] { landscapeLayerArgs, subtype }
        );

        // Perform initial lazy loading for this layer
        PerformInitialLazyLoadForLayer(landscapeLayerInstance, playerPosition, loadDistance);

        // Add as dependency
        AddLayerDependency(new LayerDependency(landscapeLayerInstance, width, height));
    }

    private void PerformInitialLazyLoadForLayer(
        AbstractChunkBasedDataLayer layer,
        Vector3 playerPosition,
        float loadDistance)
    {
        // Define chunk evaluation for this layer
        bool ShouldCreateChunk(Point chunkIndex, int level, Vector3 playerPos)
        {
            var chunkWorldPos = new Vector3(
                chunkIndex.x * layer.chunkW + layer.chunkW / 2,
                0,
                chunkIndex.y * layer.chunkH + layer.chunkH / 2
            );

            var distanceToPlayer = playerPos.DistanceTo(chunkWorldPos);
            bool withinRange = distanceToPlayer <= loadDistance;

            if (!withinRange)
            {
                GD.Print($"[Initial Load] Skipping {layer.GetType().Name} chunk {chunkIndex} - outside player range (distance: {distanceToPlayer:F1})");
            }

            return withinRange;
        }

        // Create bounds around player for initial load
        var initialBounds = GetBoundsAroundPosition(playerPosition, loadDistance * 1.2f); // Slightly larger initial area

        GD.Print($"[Initial Load] Loading {layer.GetType().Name} chunks around player at {playerPosition}");
        layer.EnsureLoadedInBounds(initialBounds, 0, null, playerPosition, ShouldCreateChunk);
    }

    private Vector3 GetCurrentPlayerPosition()
    {
        // Fast lookup - just get current position from cached node
        if (_cachedPlayerNode != null)
        {
            try
            {
                _lastKnownPlayerPosition = _cachedPlayerNode.GlobalPosition;
                return _lastKnownPlayerPosition;
            }
            catch (ObjectDisposedException)
            {
                // Node was freed/disposed
                _cachedPlayerNode = null;
            }
            catch (Exception)
            {
                // Other issues accessing the node
                _cachedPlayerNode = null;
            }
        }
        
        // Fallback to last known position if node became invalid
        return _lastKnownPlayerPosition;
    }
    
    private void SetupCameraMovementHandling(LayerArgumentDictionary layerArguments)
    {
        void ReconstructNodesHandler(Vector3 checkpointPos, Vector3 cameraPos, float distance)
        {
            // Only process if camera moved significantly
            if (distance < 5f) return;

            // Use fast cached player position lookup
            Vector3 referencePosition = (_cachedPlayerNode != null) 
                ? GetCurrentPlayerPosition() 
                : cameraPos;

            // Process each landscape layer dependency
            HandleDependenciesForLevel(0, dependency =>
            {
                var layer = dependency.layer;
                if (layer.GetType().Name.StartsWith("LandscapeLayer"))
                {
                    UpdateLayerBasedOnCameraMovement((IChunkBasedDataLayer)layer, referencePosition, dependency);
                }
            });
        }

        SignalBus.Instance.ReconstructNodes += ReconstructNodesHandler;
    }

    private void UpdateLayerBasedOnCameraMovement(
        IChunkBasedDataLayer layer,
        Vector3 referencePosition,
        LayerDependency dependency)
    {
        // Determine load distance based on layer type
        float loadDistance = layer.GetType().Name switch
        {
            "LandscapeLayerD" => 200f,
            "LandscapeLayerC" => 150f,
            "LandscapeLayerB" => 100f,
            "LandscapeLayerA" => 75f,
            _ => 100f
        };

        bool ShouldCreateChunk(Point chunkIndex, int level, Vector3 playerPos)
        {
            var chunkWorldPos = new Vector3(
                chunkIndex.x * layer.chunkW + layer.chunkW / 2,
                0,
                chunkIndex.y * layer.chunkH + layer.chunkH / 2
            );

            return playerPos.DistanceTo(chunkWorldPos) <= loadDistance;
        }

        var bounds = GetBoundsAroundPosition(referencePosition, loadDistance * 1.5f);
        ((AbstractChunkBasedDataLayer)layer).EnsureLoadedInBounds(bounds, 0, null, referencePosition, ShouldCreateChunk);
    }

    private GridBounds GetBoundsAroundPosition(Vector3 position, float range)
    {
        return new GridBounds(
            (int)(position.X - range),
            (int)(position.Z - range),
            (int)(range * 2),
            (int)(range * 2)
        );
    }

    private void SetupVillageLayer(LayerArgumentDictionary layerArguments)
    {
        var villageLayer = LSystemVillageLayer.GetInstance(layerArguments);

        AddLayerDependency(new LayerDependency(
            villageLayer,
            256,
            256,
            villageLayer.GetLevelCount() - 1,
            (bounds, level, levelData) =>
            {
                void Handler()
                {
                    if (!LandscapeChunkCounterBlackboard.LandscapeChunksAreReady)
                    {
                        return;
                    }
                    villageLayer.EnsureLoadedInBounds(bounds, level, levelData);
                    GD.Print("LSystemVillageLayer dependency loaded after LandscapeChunksReady signal.");
                }

                SignalBus.Instance.LandscapeChunksReady += Handler;

                if (LandscapeChunkCounterBlackboard.LandscapeChunksAreReady)
                    Handler();
            }
        ));
    }
}
