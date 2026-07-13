using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class ThinkNodeWorkTest
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
    public void PrioritisesMineOverHaul()
    {
        Game.Map.Designations.Add(DesignationType.Mine, new Vector2I(5, 5));
        var sp = Game.Map.Stockpiles.Create();
        sp.Cells.Add(new Vector2I(0, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(7, 7), 3);
        var guy = new Guy { Position = Vector2.Zero };

        var job = new ThinkNode_Work().TryGiveJob(guy);

        AssertBool(job.Type == JobType.Mine).IsTrue();
    }

    [TestCase]
    public void ReturnsHaulWhenNoMine()
    {
        var sp = Game.Map.Stockpiles.Create();
        sp.Cells.Add(new Vector2I(0, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(7, 7), 3);
        var guy = new Guy { Position = Vector2.Zero };

        var job = new ThinkNode_Work().TryGiveJob(guy);

        AssertBool(job.Type == JobType.Haul).IsTrue();
    }

    [TestCase]
    public void ReturnsNullWhenNoWork()
    {
        var guy = new Guy { Position = Vector2.Zero };
        AssertObject(new ThinkNode_Work().TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void SkipsADisabledWorkGiver()
    {
        Game.Map.Designations.Add(DesignationType.Mine, new Vector2I(5, 5));
        var sp = Game.Map.Stockpiles.Create();
        sp.Cells.Add(new Vector2I(0, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(7, 7), 3);
        var guy = new Guy { Position = Vector2.Zero };
        guy.WorkSettings.Set(WorkType.Mine, false); // mining off, hauling still on

        var job = new ThinkNode_Work().TryGiveJob(guy);

        AssertBool(job.Type == JobType.Haul).IsTrue(); // falls through to the next allowed giver
    }

    [TestCase]
    public void NoWorkDuringAScheduledSleepBlock()
    {
        Game.Map.Designations.Add(DesignationType.Mine, new Vector2I(5, 5)); // work is available
        var guy = new Guy { Position = Vector2.Zero };
        guy.Schedule.Set(GameTime.HourOfDay, ScheduleBlock.Sleep); // this hour is scheduled sleep

        AssertObject(new ThinkNode_Work().TryGiveJob(guy)).IsNull();
    }
}