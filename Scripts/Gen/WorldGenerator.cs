using Godot;

/// <summary>Procedural terrain generator using two SimplexSmooth noise layers (elevation + moisture).</summary>
public static class WorldGenerator
{
    /// <summary>Fills the map's terrain grid.</summary>
    /// <param name="map">The map to populate.</param>
    /// <param name="settings">Main node holding noise and threshold exports.</param>
    public static void Generate(GameMap map, Main settings)
    {
        TerrainDefOf.Load();
        PlantDefOf.Load();

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
        ScatterPlants(map, settings);
    }

    // scale from 1 to -1 for moisture and elevation.
    // elevation > StoneElevationThreshold => always Stone. Moisture doesn't affect high elevation
    // ele < WaterET && moisture > WaterMoistureThreshold => Water
    // ele < WaterET && moisture <= WaterMT && moisture > GrassMT => Grass
    // ele < WaterET && moisture <= WaterMT && moisture <= GrassMT => Dirt
    // ele between thresholds && moisture > GrassMT => Grass
    // ele between thresholds && moisture <= GrassMT => Dirt
    // TLDR: Stone always on high elevation, water at low elevation + wet, everything else is grass if it's wet enough or dirt if not.

    /// <summary>Determines terrain type from elevation and moisture values.</summary>
    /// <param name="e">Elevation noise value (-1 to 1).</param>
    /// <param name="m">Moisture noise value (-1 to 1).</param>
    /// <param name="settings">Threshold settings.</param>
    /// <returns>The terrain def for this cell.</returns>
    static TerrainDef PickTerrain(float e, float m, Main settings)
    {
        if (e < settings.WaterElevationThreshold && m > settings.WaterMoistureThreshold)
            return TerrainDefOf.Water;
        if (e > settings.StoneElevationThreshold) return TerrainDefOf.Stone;
        if (m > settings.GrassMoistureThreshold) return TerrainDefOf.Grass;
        return TerrainDefOf.Dirt;
    }

    /// <summary>Second pass: sprinkles trees and bushes onto walkable ground using a density
    /// noise layer (seed + 2). Plants form "forests" with walkable gaps inside them,
    /// rather than a uniform spread. Runs before pathing init.</summary>
    static void ScatterPlants(GameMap map, Main settings)
    {
        var density = new FastNoiseLite();
        density.Seed = settings.Seed + 2;
        density.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        density.Frequency = settings.PlantFrequency;
        density.FractalOctaves = settings.PlantOctaves;

        var rng = new System.Random(settings.Seed);

        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                var cell = new Vector2I(x, y);
                if (!map.Terrain[x, y].Walkable) continue;
                if (density.GetNoise2D(x, y) < settings.PlantDensityThreshold) continue;
                if (rng.NextDouble() > settings.PlantCoverage) continue;

                map.SpawnPlant(PickPlant(rng), cell);
            }
    }

    /// <summary>Weighted random plant pick, mostly pines, some oaks, occasional berry bushes.</summary>
    static PlantDef PickPlant(System.Random rng)
    {
        double r = rng.NextDouble();
        if (r < 0.5) return PlantDefOf.Pine;
        if (r < 0.8) return PlantDefOf.Oak;
        return PlantDefOf.BerryBush;
    }
}