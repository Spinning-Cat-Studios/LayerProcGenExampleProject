using System;

namespace LayerProcGenExampleProject.ProcGen.Layers.PlayLayerComponents
{
    public static class PlayLayerConfiguration
    {
        public const float CHECKPOINT_DIST_DELTA_THRESHOLD = 50.0f;
        public const int CHUNK_WIDTH = 8;
        public const int CHUNK_HEIGHT = 8;
        public const float PLAY_LAYER_LOAD_DISTANCE = 75f;
        public const float VILLAGE_LAYER_LOAD_DISTANCE = 150f;

        public static readonly LandscapeLayerConfig[] LandscapeLayerConfigs = new[]
        {
            new LandscapeLayerConfig(typeof(LandscapeLayerD), 2048, 2048, "D", 200f),
            new LandscapeLayerConfig(typeof(LandscapeLayerC), 1024, 1024, "C", 150f),
            new LandscapeLayerConfig(typeof(LandscapeLayerB), 512, 512, "B", 100f),
            new LandscapeLayerConfig(typeof(LandscapeLayerA), 256, 256, "A", 75f)
        };

        public static float GetLoadDistanceForLayer(string layerTypeName)
        {
            return layerTypeName switch
            {
                "LandscapeLayerD" => 200f,
                "LandscapeLayerC" => 150f,
                "LandscapeLayerB" => 100f,
                "LandscapeLayerA" => 75f,
                "LSystemVillageLayer" => VILLAGE_LAYER_LOAD_DISTANCE,
                _ => 100f
            };
        }
    }

    public record LandscapeLayerConfig(
        Type Type,
        int Width,
        int Height,
        string Subtype,
        float LoadDistance
    );
}
