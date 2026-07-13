using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class DefDatabaseTest
{
    [BeforeTest]
    public void Setup()
    {
        DefDatabase<ItemDef>.Clear();
        DefLoader.LoadFolder<ItemDef>("res://Defs/Items");
    }

    [TestCase]
    public void LoadsItemsFromFolder()
    {
        AssertObject(DefDatabase<ItemDef>.Get("Wheat")).IsNotNull();
        AssertObject(DefDatabase<ItemDef>.Get("SimpleMeal")).IsNotNull();
    }

    [TestCase]
    public void UnknownDefThrows()
    {
        bool threw = false;
        try { DefDatabase<ItemDef>.Get("NotAThing"); }
        catch (System.Collections.Generic.KeyNotFoundException) { threw = true; }
        AssertBool(threw).IsTrue();
    }

    [TestCase]
    public void TryGetUnknownReturnsFalse()
    {
        AssertBool(DefDatabase<ItemDef>.TryGet("NotAThing", out _)).IsFalse();
    }

    [TestCase]
    public void TryGetKnownReturnsTrueAndDef()
    {
        AssertBool(DefDatabase<ItemDef>.TryGet("Wheat", out var def)).IsTrue();
        AssertObject(def).IsNotNull();
    }

    [TestCase]
    public void AllReturnsEveryLoadedItem()
    {
        AssertInt(DefDatabase<ItemDef>.All.Count).IsGreater(0);
    }
}