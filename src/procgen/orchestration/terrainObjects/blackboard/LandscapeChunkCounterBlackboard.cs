public static class LandscapeChunkCounterBlackboard
{
    public static Godot.Collections.Dictionary<string, int> ChunkCountDictionary = new()
    {
        { "LandscapeLayerA", 0 },
        { "LandscapeLayerB", 0 },
        { "LandscapeLayerC", 0 },
        { "LandscapeLayerD", 0 }
    };

    public static int GridDoneCounter { get; set; } = 0;

    public static bool LandscapeChunksAreReady = false;

    public static readonly object ChunkCountLock = new();
}
