using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class ReservationManagerTest
{
    [BeforeTest]
    public void Setup() => DefLoader.LoadAll(); // Guy/Item construction reads the def database

    [TestCase]
    public void UnreservedCellIsAvailableToAnyone()
    {
        var rm = new ReservationManager();
        AssertBool(rm.AvailableCell(new Vector2I(1, 1), new Guy())).IsTrue();
    }

    [TestCase]
    public void ReservedCellIsFreeToOwnerButNotOthers()
    {
        var rm = new ReservationManager();
        var owner = new Guy();
        var other = new Guy();
        var cell = new Vector2I(1, 1);
        rm.ReserveCell(cell, owner);

        AssertBool(rm.AvailableCell(cell, owner)).IsTrue();  // the owner still sees it free
        AssertBool(rm.AvailableCell(cell, other)).IsFalse(); // nobody else does
    }

    [TestCase]
    public void ReservedItemIsFreeToOwnerButNotOthers()
    {
        var rm = new ReservationManager();
        var owner = new Guy();
        var other = new Guy();
        var item = new Item { Def = ItemDefOf.Stone, Cell = Vector2I.Zero, Count = 1 };
        rm.ReserveItem(item, owner);

        AssertBool(rm.AvailableItem(item, owner)).IsTrue();
        AssertBool(rm.AvailableItem(item, other)).IsFalse();
    }

    [TestCase]
    public void NullItemIsAlwaysAvailableAndReserveIsANoOp()
    {
        var rm = new ReservationManager();
        var guy = new Guy();
        AssertBool(rm.AvailableItem(null, guy)).IsTrue(); // null-guard, not a throw
        rm.ReserveItem(null, guy);                        // no-op
        AssertBool(rm.AvailableItem(null, guy)).IsTrue();
    }

    [TestCase]
    public void ReleaseAllFreesTheGuysOwnClaims()
    {
        var rm = new ReservationManager();
        var owner = new Guy();
        var other = new Guy();
        var cell = new Vector2I(2, 2);
        var item = new Item { Def = ItemDefOf.Stone, Cell = cell, Count = 1 };
        rm.ReserveCell(cell, owner);
        rm.ReserveItem(item, owner);

        rm.ReleaseAll(owner);

        AssertBool(rm.AvailableCell(cell, other)).IsTrue(); // freed
        AssertBool(rm.AvailableItem(item, other)).IsTrue(); // freed
    }

    [TestCase]
    public void ReleaseAllLeavesAnotherGuysClaimsIntact()
    {
        var rm = new ReservationManager();
        var a = new Guy();
        var b = new Guy();
        var cellB = new Vector2I(3, 3);
        rm.ReserveCell(new Vector2I(1, 1), a);
        rm.ReserveCell(cellB, b);

        rm.ReleaseAll(a);

        AssertBool(rm.AvailableCell(cellB, a)).IsFalse(); // b's claim survives a's release
    }
}
