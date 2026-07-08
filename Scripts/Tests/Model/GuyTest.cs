using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class GuyTest
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

    // gives him a reachable mining job to be busy with
    static Guy BusyGuy()
    {
        Game.Map.Designations.Add(DesignationType.Mine, new Vector2I(5, 5));
        var guy = new Guy { Position = Vector2.Zero };
        guy.Tick(); // assigns and starts the mine job
        return guy;
    }

    [TestCase]
    public void CollapsesAndSleepsWhenExhausted()
    {
        var guy = BusyGuy();
        guy.Needs.Rest.Level = 0.01f; // below the collapse threshold

        guy.Tick(); // abandons the job, think tree picks Sleep

        AssertBool(guy.IsSleeping).IsTrue();
    }

    [TestCase]
    public void DoesNotCollapseWhenMerelyTired()
    {
        var guy = BusyGuy();
        guy.Needs.Rest.Level = 0.2f; // tired, but above collapse

        guy.Tick();// keeps mining, does not abandon mid-job

        AssertBool(guy.IsSleeping).IsFalse();
    }

    // a fresh guy with the given Endurance, mining at a cell, returns how much Rest fell in one exerted tick
    static float RestDropWhileMining(int endurance, Vector2I cell)
    {
        Game.Map.Designations.Add(DesignationType.Mine, cell);
        var guy = new Guy { Position = new Vector2(cell.X - 1, cell.Y) }; // start adjacent, so it mines at once
        guy.Attributes.Get(AttributeDefOf.Endurance).Level = endurance;
        guy.Tick(); // assigns the job; this tick's decay is still idle-rate
        float before = guy.Needs.Rest.Level;
        guy.Tick();// one tick of actual mining, exerted
        return before - guy.Needs.Rest.Level;
    }

    [TestCase]
    public void MoveSpeedScalesWithAgility()
    {
        var guy = new Guy { Position = Vector2.Zero };
        AssertFloat(guy.MoveSpeed).IsEqual(Guy.BaseMoveSpeed); // baseline agility = base speed

        guy.Attributes.Get(AttributeDefOf.Agility).Level = Attributes.Baseline + 4;
        AssertFloat(guy.MoveSpeed).IsEqual(Guy.BaseMoveSpeed * (1f + 4 * Attributes.FactorPerLevel));
    }

    [TestCase]
    public void LearningRateScalesWithIntelligence()
    {
        var guy = new Guy { Position = Vector2.Zero };
        AssertFloat(guy.LearningRate).IsEqual(1f); // baseline

        guy.Attributes.Get(AttributeDefOf.Intelligence).Level = Attributes.Baseline - 2;
        AssertFloat(guy.LearningRate).IsEqual(1f - 2 * Attributes.FactorPerLevel); // dimmer = slower learner
    }

    [TestCase]
    public void HighEnduranceDrainsNeedsSlowerUnderExertion()
    {
        float baseline = RestDropWhileMining(Attributes.Baseline, new Vector2I(5, 5));
        float tough = RestDropWhileMining(Attributes.Baseline + 5, new Vector2I(5, 8));
        AssertFloat(tough).IsLess(baseline); // same job, tougher colonist tires less
    }
}