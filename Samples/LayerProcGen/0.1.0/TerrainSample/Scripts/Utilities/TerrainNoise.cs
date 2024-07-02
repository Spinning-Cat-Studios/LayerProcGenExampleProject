namespace Godot.Util
{
    public static class TerrainNoise
    {
        private static FastNoiseLite Noise;
        private static float TotalHeight = 1;
        private static float MinHeight = 0;

        private static Vector2 TerrainHeight { get; set; }

        public static void SetFullTerrainHeight(Vector2 terrainHeight)
        {
            TerrainHeight = terrainHeight;
            TotalHeight = Mathf.Abs(terrainHeight.X) + terrainHeight.Y;
            MinHeight = Mathf.Abs(terrainHeight.X);
        }

        public static float GetHeight(Vector2 coords)
        {
            return Noise.GetNoise2Dv(coords) * (TotalHeight - MinHeight) + MinHeight;
        }

        static TerrainNoise()
        {
            Noise = new FastNoiseLite();
            Noise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);

            Noise.SetFrequency(0.0005f);
            Noise.SetFractalLacunarity(2f);
            Noise.SetFractalGain(0.5f);
            Noise.SetFractalOctaves(6);

            Noise.SetFractalType(FastNoiseLite.FractalTypeEnum.Fbm);
        }
    }
}