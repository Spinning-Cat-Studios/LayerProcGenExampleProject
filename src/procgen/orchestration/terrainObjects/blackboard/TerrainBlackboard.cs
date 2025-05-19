using Godot;
using T3 = Terrain3DBindings;

public static class TerrainBlackboard
{
    public static T3.Terrain3D Terrain { get; set; }
    public static T3.Terrain3DStorage Storage { get; set; }
    public static float VSpacing { get; set; }
    public static NodePath TerrainPath { get; set; }
}
