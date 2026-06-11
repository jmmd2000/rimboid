using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

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
}