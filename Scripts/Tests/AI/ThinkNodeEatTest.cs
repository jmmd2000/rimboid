using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class ThinkNodeEatTest
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
    public void ReturnsEatJobWhenHungry()
    {
        Game.Map.SpawnItem(ItemDefOf.Berries, new Vector2I(5, 5), 10);
        var guy = new Guy { Position = Vector2.Zero };
        guy.Needs.Food.Level = 0.1f;

        var job = new ThinkNode_Eat(urgent: false).TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Eat).IsTrue();
        AssertBool(job.TargetCell == new Vector2I(5, 5)).IsTrue();
    }

    [TestCase]
    public void NullWhenNotHungry()
    {
        Game.Map.SpawnItem(ItemDefOf.Berries, new Vector2I(5, 5), 10);
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new ThinkNode_Eat(urgent: false).TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void IgnoresNonFoodItems()
    {
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(5, 5), 10);
        var guy = new Guy { Position = Vector2.Zero };
        guy.Needs.Food.Level = 0.1f;

        AssertObject(new ThinkNode_Eat(urgent: false).TryGiveJob(guy)).IsNull();
    }
}