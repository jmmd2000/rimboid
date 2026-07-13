using GdUnit4;
using static GdUnit4.Assertions;
using Godot;

[TestSuite]
[RequireGodotRuntime]
public class ThinkNodeSleepTest
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

    [TestCase]
    public void ReturnsSleepJobWhenTired()
    {
        var guy = new Guy();
        guy.Needs.Rest.Level = 0.2f;
        var job = new ThinkNode_Sleep(urgent: false).TryGiveJob(guy);
        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Sleep).IsTrue();
    }

    [TestCase]
    public void ReturnsNullWhenRested()
    {
        var guy = new Guy();
        guy.Needs.Rest.Level = 0.5f;
        AssertObject(new ThinkNode_Sleep(urgent: false).TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void ThresholdIsExclusive()
    {
        var guy = new Guy();
        guy.Needs.Rest.Level = 0.3f;
        AssertObject(new ThinkNode_Sleep(urgent: false).TryGiveJob(guy)).IsNull();
    }

    static Guy Sleeper(Vector2I cell)
    {
        var guy = new Guy { Position = new Vector2(cell.X, cell.Y) };
        guy.Needs.Rest.Level = 0.2f; // tired, above collapse
        return guy;
    }

    [TestCase]
    public void HeadsToAFreeBedWhenTired()
    {
        var bedCell = new Vector2I(5, 5);
        Game.Map.SpawnBuilding(DefDatabase<BuildingDef>.Get("Bed"), bedCell);
        Game.Pathing.Init(Game.Map); // the bed is passable, so it stays reachable

        var job = new ThinkNode_Sleep(urgent: false).TryGiveJob(Sleeper(new Vector2I(0, 0)));

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Sleep).IsTrue();
        AssertBool(job.ClaimsCell).IsTrue();
        AssertBool(job.TargetCell == bedCell).IsTrue();
    }

    [TestCase]
    public void SleepsInPlaceWithNoBed()
    {
        var job = new ThinkNode_Sleep(urgent: false).TryGiveJob(Sleeper(new Vector2I(0, 0)));

        AssertObject(job).IsNotNull();
        AssertBool(job.ClaimsCell).IsFalse(); // no bed, drops on the spot
    }

    [TestCase]
    public void TwoColonistsDoNotShareABed()
    {
        var bedCell = new Vector2I(5, 5);
        Game.Map.SpawnBuilding(DefDatabase<BuildingDef>.Get("Bed"), bedCell);
        Game.Pathing.Init(Game.Map);

        Game.Map.Reservations.ReserveCell(bedCell, Sleeper(new Vector2I(0, 0))); // first has claimed it
        var job = new ThinkNode_Sleep(urgent: false).TryGiveJob(Sleeper(new Vector2I(1, 1)));

        AssertBool(job.ClaimsCell).IsFalse(); // second can't take it, sleeps in place
    }

    [TestCase]
    public void UrgentCollapsesInPlaceEvenWithABed()
    {
        Game.Map.SpawnBuilding(DefDatabase<BuildingDef>.Get("Bed"), new Vector2I(5, 5));

        var guy = new Guy { Position = new Vector2(0, 0) };
        guy.Needs.Rest.Level = 0.02f; // below collapse

        var job = new ThinkNode_Sleep(urgent: true).TryGiveJob(guy);
        AssertObject(job).IsNotNull();
        AssertBool(job.ClaimsCell).IsFalse();
    }

    [TestCase]
    public void SleepsProactivelyInASleepBlockEvenWhenRested()
    {
        var guy = new Guy();
        guy.Needs.Rest.Level = 1f; // fully rested
        guy.Schedule.Set(GameTime.HourOfDay, ScheduleBlock.Sleep);

        var job = new ThinkNode_Sleep(urgent: false).TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Sleep).IsTrue();
    }
}