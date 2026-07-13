using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class ThinkNodeWanderTest
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
    public void ReturnsWanderJobToAReachableNearbyCell()
    {
        var guy = new Guy { Position = new Vector2(5, 5) };
        var job = new ThinkNode_Wander().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Wander).IsTrue();
        AssertBool(job.TargetCell != guy.Cell).IsTrue();                        // never the current cell
        AssertBool(Game.Pathing.IsReachable(guy.Cell, job.TargetCell)).IsTrue(); // and reachable
    }

    [TestCase]
    public void ReturnsNullWhenBoxedIn()
    {
        // wall the guy into its single cell, so nothing nearby is reachable
        foreach (var d in Grid.Adjacent8)
            Game.Map.SpawnBuilding(BuildingDefOf.WallStone, new Vector2I(5, 5) + d);
        Game.Pathing.Init(Game.Map); // rebuild pathing with the walls

        var guy = new Guy { Position = new Vector2(5, 5) };

        AssertObject(new ThinkNode_Wander().TryGiveJob(guy)).IsNull();
    }
}
