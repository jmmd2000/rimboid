using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class DoorTest
{
    [BeforeTest]
    public void Setup() { DefLoader.LoadAll(); Game.Map = new GameMap(10, 10); }

    static BuildingComponent_Door Door(Vector2I cell) =>
        Game.Map.SpawnBuilding(DefDatabase<BuildingDef>.Get("DoorWood"), cell).GetComponent<BuildingComponent_Door>();

    [TestCase]
    public void OpensWhenAColonistIsAdjacent()
    {
        var door = Door(new Vector2I(5, 5));
        Game.Map.Guys.Add(new Guy { Position = new Vector2(5, 4) });
        door.Tick();
        AssertBool(door.Open).IsTrue();
    }

    [TestCase]
    public void OpensWhenAColonistIsOnTheDoorCell()
    {
        var door = Door(new Vector2I(5, 5));
        Game.Map.Guys.Add(new Guy { Position = new Vector2(5, 5) });
        door.Tick();
        AssertBool(door.Open).IsTrue();
    }

    [TestCase]
    public void StaysShutWhenNobodyIsNear()
    {
        var door = Door(new Vector2I(5, 5));
        Game.Map.Guys.Add(new Guy { Position = new Vector2(0, 0) });
        door.Tick();
        AssertBool(door.Open).IsFalse();
    }

    [TestCase]
    public void DoesNotBlockMovement()
    {
        Door(new Vector2I(5, 5));
        AssertBool(Game.Map.BlocksMovementAt(new Vector2I(5, 5))).IsFalse();
    }
}