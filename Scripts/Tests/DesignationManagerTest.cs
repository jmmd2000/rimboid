using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class DesignationManagerTest
{
    [TestCase]
    public void AddThenHasReturnsTrue()
    {
        var dm = new DesignationManager();
        dm.Add(DesignationType.Mine, new Vector2I(5, 5));
        AssertBool(dm.Has(DesignationType.Mine, new Vector2I(5, 5))).IsTrue();
    }

    [TestCase]
    public void HasOnWrongCellReturnsFalse()
    {
        var dm = new DesignationManager();
        dm.Add(DesignationType.Mine, new Vector2I(5, 5));
        AssertBool(dm.Has(DesignationType.Mine, new Vector2I(0, 0))).IsFalse();
    }

    [TestCase]
    public void AddDuplicateDoesNotCreateSecondEntry()
    {
        var dm = new DesignationManager();
        var cell = new Vector2I(5, 5);
        dm.Add(DesignationType.Mine, cell);
        dm.Add(DesignationType.Mine, cell);
        int count = 0;
        foreach (var c in dm.CellsOfType(DesignationType.Mine)) count++;
        AssertInt(count).IsEqual(1);
    }

    [TestCase]
    public void RemoveThenHasReturnsFalse()
    {
        var dm = new DesignationManager();
        var cell = new Vector2I(5, 5);
        dm.Add(DesignationType.Mine, cell);
        dm.Remove(DesignationType.Mine, cell);
        AssertBool(dm.Has(DesignationType.Mine, cell)).IsFalse();
    }

    [TestCase]
    public void CellsOfTypeEmptyAfterRemove()
    {
        var dm = new DesignationManager();
        var cell = new Vector2I(5, 5);
        dm.Add(DesignationType.Mine, cell);
        dm.Remove(DesignationType.Mine, cell);
        int count = 0;
        foreach (var c in dm.CellsOfType(DesignationType.Mine)) count++;
        AssertInt(count).IsEqual(0);
    }
}