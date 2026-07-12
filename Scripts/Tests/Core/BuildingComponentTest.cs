using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class BuildingComponentTest
{
    [BeforeTest]
    public void Setup() => DefLoader.LoadAll();

    [TestCase]
    public void StoveHasAWorkBenchComponent()
    {
        var stove = new GameMap(10, 10).SpawnBuilding(BuildingDefOf.Stove, new Vector2I(2, 2));
        AssertObject(stove.GetComponent<BuildingComponent_WorkBench>()).IsNotNull();
        AssertObject(stove.WorkBench).IsNotNull(); // the convenience
    }

    [TestCase]
    public void WallHasNoWorkBench()
    {
        var wall = new GameMap(10, 10).SpawnBuilding(BuildingDefOf.WallStone, new Vector2I(2, 2));
        AssertObject(wall.GetComponent<BuildingComponent_WorkBench>()).IsNull();
        AssertObject(wall.WorkBench).IsNull();
    }

    [TestCase]
    public void StoveHasALightComponent()
    {
        var stove = new GameMap(10, 10).SpawnBuilding(BuildingDefOf.Stove, new Vector2I(2, 2));
        AssertObject(stove.GetComponent<BuildingComponent_Light>()).IsNotNull();
    }

    [TestCase]
    public void WallHasNoLight()
    {
        var wall = new GameMap(10, 10).SpawnBuilding(BuildingDefOf.WallStone, new Vector2I(2, 2));
        AssertObject(wall.GetComponent<BuildingComponent_Light>()).IsNull();
    }
}