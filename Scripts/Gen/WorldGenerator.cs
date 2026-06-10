using Godot;

public static class WorldGenerator
{
    public static void Generate(GameMap map, int seed)
    {
        TerrainDefOf.Load();

        var elevation = new FastNoiseLite();
        elevation.Seed = seed;
        elevation.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        elevation.Frequency = 0.02f;
        elevation.FractalOctaves = 4;

        var moisture = new FastNoiseLite();
        moisture.Seed = seed + 1;
        moisture.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        moisture.Frequency = 0.03f;
        moisture.FractalOctaves = 3;

        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                float e = elevation.GetNoise2D(x, y);
                float m = moisture.GetNoise2D(x, y);

                map.Terrain[x, y] = PickTerrain(e, m);
            }
    }

    static TerrainDef PickTerrain(float e, float m)
    {
        if (e < -0.3f) return TerrainDefOf.Water;
        if (e > 0.5f) return TerrainDefOf.Stone;
        if (m > 0.2f) return TerrainDefOf.Grass;
        return TerrainDefOf.Dirt;
    }
}