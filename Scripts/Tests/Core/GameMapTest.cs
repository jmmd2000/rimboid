using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using System.Linq;

[TestSuite]
[RequireGodotRuntime]
public class GameMapTest
{
    [BeforeTest]
    public void Setup()
    {
        ItemDefOf.Load();
    }

    [TestCase]
    public void SpawnItemCreatesNewItem()
    {
        var map = new GameMap(10, 10);
        map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), 5);
        AssertInt(map.LooseItems.Count).IsEqual(1);
        AssertInt(map.LooseItems[0].Count).IsEqual(5);
    }

    [TestCase]
    public void SpawnItemMergesExistingStack()
    {
        var map = new GameMap(10, 10);
        map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), 5);
        map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), 3);
        AssertInt(map.LooseItems.Count).IsEqual(1);
        AssertInt(map.LooseItems[0].Count).IsEqual(8);
    }

    [TestCase]
    public void SpawnItemDifferentCellsDoNotMerge()
    {
        var map = new GameMap(10, 10);
        map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), 5);
        map.SpawnItem(ItemDefOf.Stone, new Vector2I(4, 4), 3);
        AssertInt(map.LooseItems.Count).IsEqual(2);
    }

    [TestCase]
    public void ItemAtFindsItem()
    {
        var map = new GameMap(10, 10);
        map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), 5);
        var item = map.ItemAt(new Vector2I(3, 3));
        AssertObject(item).IsNotNull();
        AssertInt(item.Count).IsEqual(5);
    }

    [TestCase]
    public void ItemAtReturnsNullWhenEmpty()
    {
        var map = new GameMap(10, 10);
        var item = map.ItemAt(new Vector2I(3, 3));
        AssertObject(item).IsNull();
    }

    [TestCase]
    public void ItemAtFiltersByDef()
    {
        var map = new GameMap(10, 10);
        map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), 5);
        var item = map.ItemAt(new Vector2I(3, 3), ItemDefOf.Stone);
        AssertObject(item).IsNotNull();
    }

    [TestCase]
    public void SpawnItemReportsNewVersusMerged()
    {
        var map = new GameMap(10, 10);
        var first = map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), 5);
        var second = map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), 3);

        AssertBool(first.isNew).IsTrue();
        AssertBool(second.isNew).IsFalse();
    }

    [TestCase]
    public void InBoundsTrueInsideGrid()
    {
        var map = new GameMap(10, 10);
        AssertBool(map.InBounds(new Vector2I(0, 0))).IsTrue();
        AssertBool(map.InBounds(new Vector2I(9, 9))).IsTrue();
    }

    [TestCase]
    public void InBoundsFalseOutsideGrid()
    {
        var map = new GameMap(10, 10);
        AssertBool(map.InBounds(new Vector2I(-1, 0))).IsFalse();
        AssertBool(map.InBounds(new Vector2I(10, 10))).IsFalse();
    }

    [TestCase]
    public void SpawnItemCapsMergedStackAndReportsOverflow()
    {
        var map = new GameMap(10, 10);
        int max = ItemDefOf.Stone.MaxStackSize;
        map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), max - 5);
        var (item, isNew, overflow) = map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), 20);

        AssertInt(item.Count).IsEqual(max);
        AssertInt(overflow).IsEqual(15);
        AssertBool(isNew).IsFalse();
    }

    [TestCase]
    public void SpawnItemCapsNewPileAtMaxStackSize()
    {
        var map = new GameMap(10, 10);
        int max = ItemDefOf.Stone.MaxStackSize;
        var (item, _, overflow) = map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), max + 30);

        AssertInt(item.Count).IsEqual(max);
        AssertInt(overflow).IsEqual(30);
    }

    [TestCase]
    public void DropItemsCapsOriginAndSpillsRemainder()
    {
        TerrainDefOf.Load();
        var map = new GameMap(10, 10);
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                map.Terrain[x, y] = TerrainDefOf.Dirt;

        int max = ItemDefOf.Stone.MaxStackSize;
        var newPiles = map.DropItems(ItemDefOf.Stone, new Vector2I(5, 5), max + 10);

        AssertInt(map.ItemAt(new Vector2I(5, 5)).Count).IsEqual(max);
        AssertInt(newPiles.Count).IsEqual(2);
        var spill = map.LooseItems.First(i => i.Cell != new Vector2I(5, 5));
        AssertInt(spill.Count).IsEqual(10);
    }

    static GameMap DirtMap()
    {
        TerrainDefOf.Load();
        var map = new GameMap(10, 10);
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                map.Terrain[x, y] = TerrainDefOf.Dirt;
        return map;
    }

    [TestCase]
    public void FreeDropCellPrefersCellBelow()
    {
        var map = DirtMap();
        AssertBool(map.FreeDropCell(new Vector2I(5, 5)) == new Vector2I(5, 6)).IsTrue();
    }

    [TestCase]
    public void FreeDropCellSkipsCellWithPlant()
    {
        PlantDefOf.Load();
        var map = DirtMap();
        map.SpawnPlant(PlantDefOf.BerryBush, new Vector2I(5, 6));
        var cell = map.FreeDropCell(new Vector2I(5, 5));

        AssertBool(cell != new Vector2I(5, 6)).IsTrue();
        AssertBool(map.HasPlant(cell)).IsFalse();
    }

    [TestCase]
    public void FreeDropCellFallsBackToOriginWhenSurrounded()
    {
        var map = DirtMap();
        foreach (var d in Grid.Adjacent8)
        {
            var n = new Vector2I(5, 5) + d;
            map.Terrain[n.X, n.Y] = TerrainDefOf.Stone;
        }
        AssertBool(map.FreeDropCell(new Vector2I(5, 5)) == new Vector2I(5, 5)).IsTrue();
    }

    [TestCase]
    public void SpawnItemRefusesDifferentDefOnOccupiedCell()
    {
        var map = new GameMap(10, 10);
        map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), 5);
        var (item, isNew, overflow) = map.SpawnItem(ItemDefOf.Berries, new Vector2I(3, 3), 4);

        AssertObject(item).IsNull();
        AssertInt(overflow).IsEqual(4);
        AssertInt(map.LooseItems.Count).IsEqual(1);
    }
}
