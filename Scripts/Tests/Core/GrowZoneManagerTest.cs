using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class GrowZoneManagerTest
{
    [TestCase]
    public void ZoneAtFindsCellInZone()
    {
        var mgr = new GrowZoneManager();
        var zone = mgr.Create();
        zone.Cells.Add(new Vector2I(5, 5));

        AssertObject(mgr.ZoneAt(new Vector2I(5, 5))).IsEqual(zone);
    }

    [TestCase]
    public void ZoneAtReturnsNullOutsideAnyZone()
    {
        var mgr = new GrowZoneManager();
        mgr.Create().Cells.Add(new Vector2I(5, 5));

        AssertObject(mgr.ZoneAt(new Vector2I(0, 0))).IsNull();
    }

    [TestCase]
    public void IsGrowZoneCellReflectsMembership()
    {
        var mgr = new GrowZoneManager();
        mgr.Create().Cells.Add(new Vector2I(5, 5));

        AssertBool(mgr.IsGrowZoneCell(new Vector2I(5, 5))).IsTrue();
        AssertBool(mgr.IsGrowZoneCell(new Vector2I(6, 6))).IsFalse();
    }

    [TestCase]
    public void ZoneHoldsItsChosenCrop()
    {
        var crop = new PlantDef { DefName = "TestCrop" };
        var zone = new GrowZoneManager().Create();
        zone.Crop = crop;
        zone.Cells.Add(new Vector2I(2, 2));

        AssertObject(zone.Crop).IsEqual(crop);
    }
}