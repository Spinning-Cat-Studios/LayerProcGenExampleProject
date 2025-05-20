using Godot;
using T3 = Terrain3DBindings;

public static class TerrainBlackboard
{
    public static T3.Terrain3D Terrain { get; set; }
    public static T3.Terrain3DStorage Storage { get; set; }
    public static float VSpacing { get; set; }
    public static NodePath TerrainPath { get; set; }

    public static void Initialize(NodePath terrainPath)
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        var sceneRoot = tree?.CurrentScene as Node3D;
        if (sceneRoot != null && sceneRoot.HasNode(terrainPath))
        {
            var terrainNode = sceneRoot.GetNode<Node3D>(terrainPath);
            Terrain = new T3.Terrain3D(terrainNode);
            Storage = Terrain.Storage;
            TerrainPath = terrainPath;
            // GD.Print($"TerrainBlackboard initialized with node: {terrainNode.Name}");
        }
        else
        {
            GD.PrintErr($"Terrain node not found at path: {terrainPath}");
        }
    }
}
