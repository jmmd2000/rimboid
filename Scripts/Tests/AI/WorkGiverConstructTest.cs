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
        DefLoader.LoadAll();

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
        Game.Map.AddFrame(frame);
        return frame;
    }

    static void Wall(Vector2I cell)
    {
        Game.Map.SpawnBuilding(BuildingDefOf.WallStone, cell);
        Game.Pathing.RefreshCell(Game.Map, cell);
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
        AssertBool(job.ReservedItems[0].Def == ItemDefOf.Stone).IsTrue();
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
    public void BuildsFrameWhenMaterialsComplete()
    {
        var frame = AddFrame(new Vector2I(5, 5));
        frame.MaterialsDelivered = frame.Def.MaterialCost;
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_Construct().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Build).IsTrue();
        AssertBool(job.TargetCell == new Vector2I(5, 5)).IsTrue();
    }

    [TestCase]
    public void BuildsFrameReachableOnlyDiagonally()
    {
        var frame = AddFrame(new Vector2I(5, 5));
        frame.MaterialsDelivered = frame.Def.MaterialCost;
        Wall(new Vector2I(5, 4));
        Wall(new Vector2I(5, 6));
        Wall(new Vector2I(4, 5));
        Wall(new Vector2I(6, 5));
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_Construct().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Build).IsTrue();
    }

    [TestCase]
    public void SkipsFrameSealedInPocket()
    {
        var frame = AddFrame(new Vector2I(5, 5));
        // would build if reachable
        frame.MaterialsDelivered = frame.Def.MaterialCost;
        // wall a 5x5 ring: (5,5) and its 8 neighbours stay open, but the pocket is sealed off
        foreach (var c in Grid.CellsInRect(new Vector2I(3, 3), new Vector2I(7, 7)))
            if (c.X == 3 || c.X == 7 || c.Y == 3 || c.Y == 7)
                Wall(c);
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_Construct().TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void SafeWorkCellAvoidsTheTrappedCentre()
    {
        Wall(new Vector2I(5, 4));
        Wall(new Vector2I(5, 6));
        Wall(new Vector2I(4, 5));
        var cell = Game.Pathing.NearestSafeWorkCell(new Vector2I(6, 5), new Vector2I(5, 5));

        AssertObject(cell).IsNotNull();
        AssertBool(cell.Value == new Vector2I(5, 5)).IsFalse();
    }

    [TestCase]
    public void OffersBuildForEnclosingWallFromOutside()
    {
        Wall(new Vector2I(5, 4));
        Wall(new Vector2I(5, 6));
        Wall(new Vector2I(4, 5));
        var frame = AddFrame(new Vector2I(6, 5));
        frame.MaterialsDelivered = frame.Def.MaterialCost;
        var guy = new Guy { Position = new Vector2(5, 5) };

        var job = new WorkGiver_Construct().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Build).IsTrue();
    }

    [TestCase]
    public void CollectsFromMultiplePilesInOneJob()
    {
        AddFrame(new Vector2I(5, 5));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(2, 2), 2);
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(3, 3), 3);
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_Construct().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertInt(job.Count).IsEqual(5); // 2 + 3, the full wall cost
        AssertInt(job.ReservedItems.Count).IsEqual(2); // both piles claimed
    }
}