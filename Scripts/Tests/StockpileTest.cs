using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class StockpileTest
{
    [BeforeTest]
    public void Setup()
    {
        ItemDefOf.Load();
        Game.Map = new GameMap(10, 10);
    }

    [TestCase]
    public void EmptyFilterAcceptsAnyItem()
    {
        var sp = new Stockpile();
        AssertBool(sp.WouldAccept(ItemDefOf.Stone)).IsTrue();
    }

    [TestCase]
    public void FilterAcceptsMatchingDef()
    {
        var sp = new Stockpile();
        sp.Accepts.Add(ItemDefOf.Stone);
        AssertBool(sp.WouldAccept(ItemDefOf.Stone)).IsTrue();
    }

    [TestCase]
    public void FreeCellForFindsEmptyCell()
    {
        var sp = new Stockpile();
        sp.Cells.Add(new Vector2I(0, 0));
        AssertObject(sp.FreeCellFor(ItemDefOf.Stone)).IsNotNull();
    }

    [TestCase]
    public void FreeCellForReturnsPartialStack()
    {
        var sp = new Stockpile();
        sp.Cells.Add(new Vector2I(0, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(0, 0), 5);
        var cell = sp.FreeCellFor(ItemDefOf.Stone);
        AssertObject(cell).IsNotNull();
    }

    [TestCase]
    public void FreeCellForSkipsFullStack()
    {
        var sp = new Stockpile();
        sp.Cells.Add(new Vector2I(0, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(0, 0), ItemDefOf.Stone.MaxStackSize);
        sp.Cells.Add(new Vector2I(1, 0));
        var cell = sp.FreeCellFor(ItemDefOf.Stone);
        AssertBool(cell == new Vector2I(1, 0)).IsTrue();
    }

    [TestCase]
    public void FreeCellForReturnsNullWhenFull()
    {
        var sp = new Stockpile();
        sp.Cells.Add(new Vector2I(0, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(0, 0), ItemDefOf.Stone.MaxStackSize);
        var cell = sp.FreeCellFor(ItemDefOf.Stone);
        AssertObject(cell).IsNull();
    }

    [TestCase]
    public void ManagerIsInStockpileTrue()
    {
        var mgr = new StockpileManager();
        var sp = mgr.Create();
        sp.Cells.Add(new Vector2I(2, 2));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(2, 2), 3);
        AssertBool(mgr.IsInStockpile(Game.Map.LooseItems[0])).IsTrue();
    }

    [TestCase]
    public void ManagerIsInStockpileFalseWhenOutside()
    {
        var mgr = new StockpileManager();
        mgr.Create();
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(5, 5), 3);
        AssertBool(mgr.IsInStockpile(Game.Map.LooseItems[0])).IsFalse();
    }

    [TestCase]
    public void ManagerIsStockpileCellTrue()
    {
        var mgr = new StockpileManager();
        var sp = mgr.Create();
        sp.Cells.Add(new Vector2I(2, 2));
        AssertBool(mgr.IsStockpileCell(new Vector2I(2, 2))).IsTrue();
    }

    [TestCase]
    public void ManagerIsStockpileCellFalse()
    {
        var mgr = new StockpileManager();
        mgr.Create();
        AssertBool(mgr.IsStockpileCell(new Vector2I(5, 5))).IsFalse();
    }

    [TestCase]
    public void ManagerBestCellForFindsSpace()
    {
        var mgr = new StockpileManager();
        var sp = mgr.Create();
        sp.Cells.Add(new Vector2I(0, 0));
        AssertObject(mgr.BestCellFor(ItemDefOf.Stone)).IsNotNull();
    }

    [TestCase]
    public void ManagerBestCellForReturnsNullWhenNoStockpiles()
    {
        var mgr = new StockpileManager();
        AssertObject(mgr.BestCellFor(ItemDefOf.Stone)).IsNull();
    }
}