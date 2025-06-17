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

        public Node3D CachedPlayerNode => _cachedPlayerNode;
        public Vector3 LastKnownPlayerPosition => _lastKnownPlayerPosition;

        public bool TryInitializeFromArguments(LayerArgumentDictionary layerArguments)
        {
            _cachedPlayerNode = FindPlayerNode(layerArguments);

            var playerPosition = ExtractPlayerPosition(layerArguments);
            if (playerPosition.HasValue)
            {
                _lastKnownPlayerPosition = playerPosition.Value;
                GD.Print($"[PlayerPositionManager] Found and cached player at: {_lastKnownPlayerPosition}");
                return true;
            }

            return false;
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

        private Vector3? ExtractPlayerPosition(LayerArgumentDictionary layerArguments)
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

            var playerNode = FindPlayerNodeByPath(playerPath);
            return playerNode?.GlobalPosition;
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
                GD.Print($"[PlayerPositionManager] Could not resolve player path '{playerPath}': {e.Message}");
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
    }
}
