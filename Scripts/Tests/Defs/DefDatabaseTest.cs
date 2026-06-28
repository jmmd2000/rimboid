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
    public void UnknownDefReturnsNull()
    {
        AssertObject(DefDatabase<ItemDef>.Get("NotAThing")).IsNull();
    }

    [TestCase]
    public void AllReturnsEveryLoadedItem()
    {
        AssertInt(DefDatabase<ItemDef>.All.Count).IsGreater(0);
    }
}