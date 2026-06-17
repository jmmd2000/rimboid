using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WorldGeneratorTest
{
    static GameMap GrassMap()
    {
        TerrainDefOf.Load();
        var map = new GameMap(10, 10);
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                map.Terrain[x, y] = TerrainDefOf.Grass;
        return map;
    }

    [TestCase]
    public void DissolvesRegionSmallerThanMin()
    {
        var map = GrassMap();
        foreach (var c in Grid.CellsInRect(new Vector2I(5, 5), new Vector2I(6, 6)))
            map.Terrain[c.X, c.Y] = TerrainDefOf.Stone;

        WorldGenerator.CleanupSmallRegions(map, 8);

        AssertObject(map.Terrain[5, 5]).IsEqual(TerrainDefOf.Grass);
    }

    [TestCase]
    public void KeepsRegionAtOrAboveMin()
    {
        var map = GrassMap();
        foreach (var c in Grid.CellsInRect(new Vector2I(4, 4), new Vector2I(6, 6)))
            map.Terrain[c.X, c.Y] = TerrainDefOf.Stone;

        WorldGenerator.CleanupSmallRegions(map, 8);

        AssertObject(map.Terrain[5, 5]).IsEqual(TerrainDefOf.Stone);
    }

    [TestCase]
    public void DissolvesIntoDominantBorderTerrain()
    {
        var map = GrassMap();
        foreach (var c in Grid.CellsInRect(new Vector2I(0, 0), new Vector2I(5, 9)))
            map.Terrain[c.X, c.Y] = TerrainDefOf.Dirt;
        map.Terrain[5, 5] = TerrainDefOf.Stone;

        WorldGenerator.CleanupSmallRegions(map, 8);

        AssertObject(map.Terrain[5, 5]).IsEqual(TerrainDefOf.Dirt);
    }
}