using Godot;
using T3 = Terrain3DBindings;

public static class TerrainBlackboard
{
    public static T3.Terrain3D Terrain { get; set; }
    public static T3.Terrain3DStorage Storage { get; set; }
    // New unified data object API (0.9.3a+). Null if not available (older plugin version).
    public static GodotObject TerrainData { get; set; }
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
            // Try to capture the terrain data object (new API). Swallow if not present.
            try { TerrainData = Terrain.Instance.Get("data").AsGodotObject(); } catch { TerrainData = null; }
            TerrainPath = terrainPath;
            // Capture vertex spacing with backward compatibility.
            try
            {
                VSpacing = Terrain.VertexSpacing; // new property (0.9.3a preferred)
            }
            catch
            {
                // Fallback in case binding not yet updated – keep silent
                #pragma warning disable CS0618
                try { VSpacing = Terrain.MeshVertexSpacing; } catch { /* ignore */ }
                #pragma warning restore CS0618
            }
            // GD.Print($"TerrainBlackboard initialized with node: {terrainNode.Name}");
        }
        else
        {
            GD.PrintErr($"Terrain node not found at path: {terrainPath}");
        }
    }
}
