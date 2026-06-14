using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WorkGiverConstructTest
{
    [BeforeTest]
    public void Setup()
    {
        ItemDefOf.Load();
        TerrainDefOf.Load();
        BuildingDefOf.Load();

        Game.Map = new GameMap(10, 10);
        for (int x = 0; x < Game.Map.Width; x++)
            for (int y = 0; y < Game.Map.Height; y++)
                Game.Map.Terrain[x, y] = TerrainDefOf.Dirt;

        Game.Pathing = new Pathing();
        Game.Pathing.Init(Game.Map);
    }

    static Frame AddFrame(Vector2I cell)
    {
        var frame = new Frame { Def = BuildingDefOf.WallStone, Cell = cell };
        Game.Map.Frames.Add(frame);
        return frame;
    }

    [TestCase]
    public void HaulsStoneToFrameNeedingMaterials()
    {
        AddFrame(new Vector2I(5, 5));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(2, 2), 10);
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_Construct().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.HaulToFrame).IsTrue();
        AssertBool(job.TargetCell == new Vector2I(5, 5)).IsTrue();
        AssertBool(job.TargetItem.Def == ItemDefOf.Stone).IsTrue();
    }

    [TestCase]
    public void CarriesOnlyWhatTheFrameNeeds()
    {
        AddFrame(new Vector2I(5, 5));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(2, 2), 10);
        var guy = new Guy { Position = Vector2.Zero };

        AssertInt(new WorkGiver_Construct().TryGiveJob(guy).Count).IsEqual(5); // not all 10
    }

    [TestCase]
    public void NoJobWhenNoStoneAvailable()
    {
        AddFrame(new Vector2I(5, 5));
        var guy = new Guy { Position = Vector2.Zero };
        AssertObject(new WorkGiver_Construct().TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void NoJobWhenMaterialsComplete()
    {
        var frame = AddFrame(new Vector2I(5, 5));
        frame.MaterialsDelivered = frame.Def.MaterialCost;
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(2, 2), 10);
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_Construct().TryGiveJob(guy)).IsNull();
    }
}