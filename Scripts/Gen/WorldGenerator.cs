using Godot;
using System.Collections.Generic;

/// <summary>Procedural terrain generator. Samples a domain-warped, redistributed elevation noise
/// into terrain bands, dissolves tiny regions, then scatters plants.</summary>
public static class WorldGenerator
{
    /// <summary>Fills the map's terrain grid.</summary>
    /// <param name="map">The map to populate.</param>
    /// <param name="settings">Main node holding noise and threshold exports.</param>
    public static void Generate(GameMap map, Main settings)
    {
        var elevation = new FastNoiseLite
        {
            Seed = settings.Seed,
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            Frequency = settings.ElevationFrequency,
            FractalOctaves = settings.ElevationOctaves
        };

        var warp = new FastNoiseLite
        {
            Seed = settings.Seed + 3,
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            Frequency = settings.ElevationFrequency * 2f
        };

        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                float warpX = warp.GetNoise2D(x, y) * settings.WarpStrength;
                float warpY = warp.GetNoise2D(x + 100, y + 100) * settings.WarpStrength;

                float e = elevation.GetNoise2D(x + warpX, y + warpY);

                // redistribute elevation: normalise to 0-1, raise to power to push toward
                // extremes (more flat lowland, sharper peaks), restore to -1 to 1
                e = Mathf.Pow((e + 1f) / 2f, settings.ElevationPower) * 2f - 1f;

                map.Terrain[x, y] = PickTerrain(e, settings);
            }
        CleanupSmallRegions(map, settings.MinTerrainRegionSize);
        ScatterPlants(map, settings);
    }

    // Terrain follows a single elevation gradient so the map reads as coherent landscape:
    // water in the lows, a dirt shore ring around it, grass across the middle, dirt
    // foothills, then stone peaks. Moisture no longer splits grass/dirt.
    static TerrainDef PickTerrain(float e, Main settings)
    {
        if (e < settings.WaterElevationThreshold) return TerrainDefOf.Water;
        if (e < settings.ShoreElevationThreshold) return TerrainDefOf.Dirt;
        if (e > settings.StoneElevationThreshold) return TerrainDefOf.Stone;
        if (e > settings.FoothillElevationThreshold) return TerrainDefOf.Dirt;
        return TerrainDefOf.Grass;
    }

    /// <summary>Second pass: sprinkles trees and bushes onto walkable ground using a density
    /// noise layer (seed + 2). Plants form "forests" with walkable gaps inside them,
    /// rather than a uniform spread. Runs before pathing init.</summary>
    static void ScatterPlants(GameMap map, Main settings)
    {
        var density = new FastNoiseLite
        {
            Seed = settings.Seed + 2,
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            Frequency = settings.PlantFrequency,
            FractalOctaves = settings.PlantOctaves
        };

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

    /// <summary>Post-process: dissolves terrain regions smaller than minSize into their
    /// surrounding terrain, removing single-cell specks and tiny puddles the noise leaves behind.</summary>
    internal static void CleanupSmallRegions(GameMap map, int minSize)
    {
        var visited = new bool[map.Width, map.Height];

        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                if (visited[x, y]) continue;
                var region = FloodRegion(map, new Vector2I(x, y), visited);
                if (region.Count >= minSize) continue;

                var replacement = DominantBorderTerrain(map, region);
                if (replacement == null) continue;
                foreach (var cell in region) map.Terrain[cell.X, cell.Y] = replacement;
            }
    }

    /// <summary>Flood-fills the connected (cardinal) region of cells sharing start's terrain.</summary>
    static List<Vector2I> FloodRegion(GameMap map, Vector2I start, bool[,] visited)
    {
        var def = map.Terrain[start.X, start.Y];
        var region = new List<Vector2I>();
        var queue = new Queue<Vector2I>();
        queue.Enqueue(start);
        visited[start.X, start.Y] = true;

        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            region.Add(c);
            foreach (var d in Grid.Cardinal4)
            {
                var n = c + d;
                if (!map.InBounds(n) || visited[n.X, n.Y]) continue;
                if (map.Terrain[n.X, n.Y] != def) continue;
                visited[n.X, n.Y] = true;
                queue.Enqueue(n);
            }
        }
        return region;
    }

    /// <summary>The terrain type that borders the region most often, used to fill it in.</summary>
    static TerrainDef DominantBorderTerrain(GameMap map, List<Vector2I> region)
    {
        var regionSet = new HashSet<Vector2I>(region);
        var counts = new Dictionary<TerrainDef, int>();

        foreach (var cell in region)
            foreach (var d in Grid.Cardinal4)
            {
                var n = cell + d;
                if (!map.InBounds(n) || regionSet.Contains(n)) continue;
                var t = map.Terrain[n.X, n.Y];
                counts[t] = counts.GetValueOrDefault(t) + 1;
            }

        TerrainDef best = null;
        int bestCount = 0;
        foreach (var kv in counts)
            if (kv.Value > bestCount) { best = kv.Key; bestCount = kv.Value; }
        return best;
    }
}