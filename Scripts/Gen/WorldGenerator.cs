using Godot;

public static class WorldGenerator
{
    public static void Generate(GameMap map, Main settings)
    {
        TerrainDefOf.Load();

        var elevation = new FastNoiseLite();
        elevation.Seed = settings.Seed;
        elevation.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        elevation.Frequency = settings.ElevationFrequency;
        elevation.FractalOctaves = settings.ElevationOctaves;

        var moisture = new FastNoiseLite();
        moisture.Seed = settings.Seed + 1;
        moisture.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        moisture.Frequency = settings.MoistureFrequency;
        moisture.FractalOctaves = settings.MoistureOctaves;

        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                float e = elevation.GetNoise2D(x, y);
                float m = moisture.GetNoise2D(x, y);

                map.Terrain[x, y] = PickTerrain(e, m, settings);
            }
    }

    // scale from 1 to -1 for moisture and elevation.
    // elevation > StoneElevationThreshold => always Stone. Moisture doesn't affect high elevation
    // ele < WaterET && moisture > WaterMoistureThreshold => Water
    // ele < WaterET && moisture <= WaterMT && moisture > GrassMT => Grass
    // ele < WaterET && moisture <= WaterMT && moisture <= GrassMT => Dirt
    // ele between thresholds && moisture > GrassMT => Grass
    // ele between thresholds && moisture <= GrassMT => Dirt
    // TLDR: Stone always on high elevation, water at low elevation + wet, everything else is grass if it's wet enough or dirt if not.
    static TerrainDef PickTerrain(float e, float m, Main settings)
    {
        if (e < settings.WaterElevationThreshold && m > settings.WaterMoistureThreshold)
            return TerrainDefOf.Water;
        if (e > settings.StoneElevationThreshold) return TerrainDefOf.Stone;
        if (m > settings.GrassMoistureThreshold) return TerrainDefOf.Grass;
        return TerrainDefOf.Dirt;
    }
}