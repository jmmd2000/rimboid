using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WorkGiverDoBillTest
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

    static Building Stove(Vector2I cell)
    {
        var stove = Game.Map.SpawnBuilding(BuildingDefOf.Stove, cell);
        foreach (var c in stove.OccupiedCells) Game.Pathing.RefreshCell(Game.Map, c);
        return stove;
    }

    [TestCase]
    public void OffersBillWhenIngredientsAvailable()
    {
        var stove = Stove(new Vector2I(5, 5));
        stove.WorkBench.Bills.Add(new Bill { Recipe = RecipeDefOf.CookSimpleMeal, RepeatMode = BillRepeatMode.DoForever });
        Game.Map.SpawnItem(ItemDefOf.Wheat, new Vector2I(2, 2), 4);
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_DoBill().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.DoBill).IsTrue();
        AssertBool(job.TargetCell == new Vector2I(5, 5)).IsTrue();
    }

    [TestCase]
    public void NoJobWhenIngredientsMissing()
    {
        var stove = Stove(new Vector2I(5, 5));
        stove.WorkBench.Bills.Add(new Bill { Recipe = RecipeDefOf.CookSimpleMeal, RepeatMode = BillRepeatMode.DoForever });
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_DoBill().TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void NoJobWhenBillSatisfied()
    {
        var stove = Stove(new Vector2I(5, 5));
        stove.WorkBench.Bills.Add(new Bill { Recipe = RecipeDefOf.CookSimpleMeal, RepeatMode = BillRepeatMode.UntilYouHave, TargetCount = 1 });
        Game.Map.SpawnItem(ItemDefOf.SimpleMeal, new Vector2I(2, 2), 1);
        Game.Map.SpawnItem(ItemDefOf.Wheat, new Vector2I(3, 3), 4);
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_DoBill().TryGiveJob(guy)).IsNull();
    }
}