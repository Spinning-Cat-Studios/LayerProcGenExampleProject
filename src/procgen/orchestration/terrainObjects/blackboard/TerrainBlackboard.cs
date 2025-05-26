using Godot;
using T3 = Terrain3DWrapper;
using TokisanGames;
using Terrain3DWrapper;

public static class TerrainBlackboard
{
    public static T3.Terrain3DWrapper TerrainWrapper { get; set; }
    public static Terrain3DStorageWrapper Storage { get; set; }
    public static Terrain3DDataWrapper Data { get; set; }
    public static float VSpacing { get; set; }
    public static NodePath TerrainPath { get; set; }

    public static void Initialize(NodePath terrainPath)
    {
        GD.Print($"Initializing TerrainBlackboard with path: {terrainPath}");

        var tree = Engine.GetMainLoop() as SceneTree;
        var sceneRoot = tree?.CurrentScene as Node3D;
        if (sceneRoot != null && sceneRoot.HasNode(terrainPath))
        {
            var terrainNode = sceneRoot.GetNode<Node3D>(terrainPath);
            TerrainWrapper = new T3.Terrain3DWrapper(terrainNode);
            if (TerrainWrapper == null)
            {
                GD.PrintErr("Failed to create Terrain3DWrapper.");
                return;
            }
            Storage = TerrainWrapper.Storage;
            Data = TerrainWrapper.Data;
            TerrainPath = terrainPath;
            GD.Print($"TerrainBlackboard initialized with node: {terrainNode.Name}");
        }
        else
        {
            GD.PrintErr($"Terrain node not found at path: {terrainPath}");
        }
    }
}
