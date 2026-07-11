using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class JobDriverDeconstructTest
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

    /// <summary>Runs a driver to a terminal status, or returns Ongoing if it never settles.</summary>
    static JobStatus Run(JobDriver driver, Guy guy, Job job, int maxTicks = 4000)
    {
        driver.Init(guy, job);
        for (int i = 0; i < maxTicks; i++)
        {
            var status = driver.Tick();
            if (status != JobStatus.Ongoing) return status;
        }
        return JobStatus.Ongoing;
    }

    [TestCase]
    public void RemovesBuildingRefundsMaterialsAndClearsDesignation()
    {
        var def = DefDatabase<BuildingDef>.Get("WallWood");
        var cell = new Vector2I(5, 5);
        Game.Map.SpawnBuilding(def, cell);
        Game.Pathing.Init(Game.Map);
        Game.Map.Designations.Add(DesignationType.Deconstruct, cell);

        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_Deconstruct().TryGiveJob(guy);

        var status = Run(new JobDriver_Deconstruct(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertObject(Game.Map.BuildingAt(cell)).IsNull();// torn down
        AssertBool(Game.Map.BlocksMovementAt(cell)).IsFalse();// cell reopened
        AssertBool(Game.Map.Designations.Has(DesignationType.Deconstruct, cell)).IsFalse(); // designation cleared
        AssertInt(Game.Map.CountStored(DefDatabase<ItemDef>.Get("Wood"))).IsEqual(def.MaterialCost); // full cost refunded
    }

    [TestCase]
    public void DeregistersTickingComponents()
    {
        var cell = new Vector2I(5, 5);
        Game.Map.SpawnBuilding(DefDatabase<BuildingDef>.Get("DoorWood"), cell); // a door ticks
        Game.Pathing.Init(Game.Map);
        AssertInt(Game.Map.TickingComponents.Count).IsEqual(1); // registered on spawn

        Game.Map.Designations.Add(DesignationType.Deconstruct, cell);
        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_Deconstruct().TryGiveJob(guy);

        Run(new JobDriver_Deconstruct(), guy, job);

        AssertInt(Game.Map.TickingComponents.Count).IsEqual(0); // deregistered on removal
    }
}