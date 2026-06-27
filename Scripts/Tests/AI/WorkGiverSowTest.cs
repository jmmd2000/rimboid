using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WorkGiverSowTest
{
    GrowZone _zone;

    [BeforeTest]
    public void Setup()
    {
        TerrainDefOf.Load();

        Game.Map = new GameMap(10, 10);
        for (int x = 0; x < Game.Map.Width; x++)
            for (int y = 0; y < Game.Map.Height; y++)
                Game.Map.Terrain[x, y] = TerrainDefOf.Dirt;

        Game.Pathing = new Pathing();
        Game.Pathing.Init(Game.Map);

        _zone = Game.Map.GrowZones.Create();
        _zone.Crop = new PlantDef { DefName = "TestCrop" };
    }

    [TestCase]
    public void ReturnsSowJobForEmptyZoneCell()
    {
        _zone.Cells.Add(new Vector2I(5, 5));
        var job = new WorkGiver_Sow().TryGiveJob(new Guy { Position = Vector2.Zero });

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Sow).IsTrue();
        AssertBool(job.TargetCell == new Vector2I(5, 5)).IsTrue();
    }

    [TestCase]
    public void ReturnsNullWhenZoneEmpty()
    {
        AssertObject(new WorkGiver_Sow().TryGiveJob(new Guy { Position = Vector2.Zero })).IsNull();
    }

    [TestCase]
    public void SkipsCellThatAlreadyHasPlant()
    {
        var cell = new Vector2I(5, 5);
        _zone.Cells.Add(cell);
        Game.Map.SpawnPlant(_zone.Crop, cell);

        AssertObject(new WorkGiver_Sow().TryGiveJob(new Guy { Position = Vector2.Zero })).IsNull();
    }

    [TestCase]
    public void SkipsWhenZoneHasNoCrop()
    {
        _zone.Crop = null;
        _zone.Cells.Add(new Vector2I(5, 5));

        AssertObject(new WorkGiver_Sow().TryGiveJob(new Guy { Position = Vector2.Zero })).IsNull();
    }

    [TestCase]
    public void PicksNearestEmptyCell()
    {
        _zone.Cells.Add(new Vector2I(8, 8));
        _zone.Cells.Add(new Vector2I(2, 2));
        var job = new WorkGiver_Sow().TryGiveJob(new Guy { Position = Vector2.Zero });

        AssertBool(job.TargetCell == new Vector2I(2, 2)).IsTrue();
    }
}