using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class BillTest
{
    [BeforeTest]
    public void Setup()
    {
        DefLoader.LoadAll();
        Game.Map = new GameMap(10, 10);
    }

    [TestCase]
    public void DoForeverAlwaysWantsMore()
    {
        var bill = new Bill { Recipe = RecipeDefOf.CookSimpleMeal, RepeatMode = BillRepeatMode.DoForever };
        AssertBool(bill.ShouldDo).IsTrue();
    }

    [TestCase]
    public void UntilYouHaveWantsMoreBelowTarget()
    {
        var bill = new Bill { Recipe = RecipeDefOf.CookSimpleMeal, RepeatMode = BillRepeatMode.UntilYouHave, TargetCount = 10 };
        AssertBool(bill.ShouldDo).IsTrue();   // 0 stored < 10
    }

    [TestCase]
    public void UntilYouHaveStopsAtTarget()
    {
        Game.Map.SpawnItem(ItemDefOf.SimpleMeal, new Vector2I(2, 2), 10);
        var bill = new Bill { Recipe = RecipeDefOf.CookSimpleMeal, RepeatMode = BillRepeatMode.UntilYouHave, TargetCount = 10 };
        AssertBool(bill.ShouldDo).IsFalse();  // 10 stored >= 10
    }
}