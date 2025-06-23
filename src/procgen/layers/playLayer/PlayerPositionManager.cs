using Godot;
using Runevision.LayerProcGen;
using System;
using Godot.Collections;

namespace LayerProcGenExampleProject.ProcGen.Layers.PlayLayerComponents
{
    public class PlayerPositionManager
    {
        private Node3D _cachedPlayerNode;
        private Vector3 _lastKnownPlayerPosition;
        private LayerArgumentDictionary _storedLayerArguments;

        public Node3D CachedPlayerNode => _cachedPlayerNode;
        public Vector3 LastKnownPlayerPosition => _lastKnownPlayerPosition;

        public void InitializeFromArguments(LayerArgumentDictionary layerArguments)
        {
            // Store the arguments for later use when PlayerSpawn signal is received
            _storedLayerArguments = layerArguments;

            // Set default position, will be updated when PlayerSpawn signal is received
            _lastKnownPlayerPosition = Vector3.Zero;
            GD.Print("[PlayerPositionManager] Initialized with arguments, waiting for PlayerSpawn signal");
        }

        public Vector3 GetCurrentPlayerPosition()
        {
            if (_cachedPlayerNode != null && GodotObject.IsInstanceValid(_cachedPlayerNode))
            {
                try
                {
                    _lastKnownPlayerPosition = _cachedPlayerNode.GlobalPosition;
                    return _lastKnownPlayerPosition;
                }
                catch (ObjectDisposedException)
                {
                    _cachedPlayerNode = null;
                }
                catch (Exception)
                {
                    _cachedPlayerNode = null;
                }
            }

            return _lastKnownPlayerPosition;
        }

        public void UpdatePlayerPosition(Vector3 newPosition)
        {
            _lastKnownPlayerPosition = newPosition;
            GD.Print($"[PlayerPositionManager] Player position updated to: {newPosition}");

            // Now that we have a confirmed player position, try to find and cache the player node
            if (_cachedPlayerNode == null && _storedLayerArguments != null)
            {
                TryFindAndCachePlayerNode();
            }
        }

        private void TryFindAndCachePlayerNode()
        {
            _cachedPlayerNode = FindPlayerNode(_storedLayerArguments);
            if (_cachedPlayerNode != null)
            {
                GD.Print($"[PlayerPositionManager] Successfully cached player node: {_cachedPlayerNode.Name}");
            }
            else
            {
                GD.Print("[PlayerPositionManager] Could not find player node, will use signal-provided position");
            }
        }

        private Node3D FindPlayerNode(LayerArgumentDictionary layerArguments)
        {
            if (!layerArguments.parameters.TryGetValue("PlayLayer", out var playLayerDict) ||
                playLayerDict == null ||
                !playLayerDict.ContainsKey("PlayerPath"))
                return null;

            string playerPath = playLayerDict["PlayerPath"].AsString();

            if (Engine.IsEditorHint())
            {
                GD.Print($"[PlayerPositionManager] Editor mode - cannot resolve player path: {playerPath}");
                return null;
            }

            return FindPlayerNodeByPath(playerPath);
        }

        private Node3D FindPlayerNodeByPath(string playerPath)
        {
            Node3D playerNode = null;

            try
            {
                var sceneTree = Engine.GetMainLoop() as SceneTree;
                if (sceneTree?.CurrentScene != null)
                {
                    // Add a check to see if the scene is ready
                    if (!sceneTree.CurrentScene.IsInsideTree())
                    {
                        GD.Print($"[PlayerPositionManager] Scene not ready yet for path: {playerPath}");
                        return null;
                    }

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
                        // Use HasNode to check existence before GetNode
                        if (sceneTree.CurrentScene.HasNode(playerPath))
                        {
                            playerNode = sceneTree.CurrentScene.GetNode(playerPath) as Node3D;
                        }
                        else
                        {
                            GD.Print($"[PlayerPositionManager] Player path '{playerPath}' not found in scene yet");
                        }
                    }
                }
                else
                {
                    GD.Print("[PlayerPositionManager] Scene tree or current scene not available yet");
                }
            }
            catch (Exception e)
            {
                GD.Print($"[PlayerPositionManager] Could not resolve player path '{playerPath}': {e.Message}");
            }

            // Try groups as fallback only if scene is ready
            if (playerNode == null)
            {
                try
                {
                    var sceneTree = Engine.GetMainLoop() as SceneTree;
                    if (sceneTree?.CurrentScene?.IsInsideTree() == true)
                    {
                        var playerNodes = sceneTree.GetNodesInGroup("player");
                        if (playerNodes.Count > 0)
                        {
                            playerNode = playerNodes[0] as Node3D;
                            if (playerNode != null)
                            {
                                GD.Print($"[PlayerPositionManager] Found player node via group: {playerNode.Name}");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    GD.Print($"[PlayerPositionManager] Error finding player via group: {e.Message}");
                }
            }

            if (playerNode != null)
            {
                GD.Print($"[PlayerPositionManager] Successfully found player node: {playerNode.Name} at {playerNode.GlobalPosition}");
            }

            return playerNode;
        }
    }
}
