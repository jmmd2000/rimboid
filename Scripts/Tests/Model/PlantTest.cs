using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class PlantTest
{
    [BeforeTest]
    public void Setup()
    {
        PlantDefOf.Load();
    }

    [TestCase]
    public void SpawnPlantStoresPlant()
    {
        var map = new GameMap(10, 10);
        var cell = new Vector2I(3, 3);
        map.SpawnPlant(PlantDefOf.Pine, cell);

        AssertObject(map.PlantAt(cell)).IsNotNull();
        AssertBool(map.HasPlant(cell)).IsTrue();
    }

    [TestCase]
    public void RemovePlantClearsCell()
    {
        var map = new GameMap(10, 10);
        var cell = new Vector2I(3, 3);
        var plant = map.SpawnPlant(PlantDefOf.Pine, cell);

        map.RemovePlant(plant);

        AssertObject(map.PlantAt(cell)).IsNull();
        AssertBool(map.HasPlant(cell)).IsFalse();
    }

    [TestCase]
    public void BlocksMovementAtReflectsPlant()
    {
        var map = new GameMap(10, 10);
        map.SpawnPlant(PlantDefOf.Pine, new Vector2I(5, 5));
        map.SpawnPlant(PlantDefOf.BerryBush, new Vector2I(6, 6));

        AssertBool(map.BlocksMovementAt(new Vector2I(5, 5))).IsTrue();
        AssertBool(map.BlocksMovementAt(new Vector2I(6, 6))).IsFalse();
        AssertBool(map.BlocksMovementAt(new Vector2I(4, 5))).IsFalse();
    }

    [TestCase]
    public void SpawnRollsDrawSizeWithinDefRange()
    {
        var map = new GameMap(10, 10);
        var plant = map.SpawnPlant(PlantDefOf.Pine, new Vector2I(3, 3));

        AssertFloat(plant.DrawWidth).IsGreaterEqual(PlantDefOf.Pine.MinDrawWidth).IsLessEqual(PlantDefOf.Pine.MaxDrawWidth);
        AssertFloat(plant.DrawHeight).IsGreaterEqual(PlantDefOf.Pine.MinDrawHeight).IsLessEqual(PlantDefOf.Pine.MaxDrawHeight);
    }

    [TestCase]
    public void DrawSizeFallsBackToFootprintWhenUnset()
    {
        var def = new PlantDef { FootprintWidth = 2, FootprintHeight = 3 };
        var plant = Plant.Spawn(def, new Vector2I(0, 0));

        AssertFloat(plant.DrawWidth).IsEqual(2f);
        AssertFloat(plant.DrawHeight).IsEqual(3f);
    }
}