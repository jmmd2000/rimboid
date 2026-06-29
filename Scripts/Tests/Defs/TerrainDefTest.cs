using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class TerrainDefTest
{
    [TestCase]
    public void StoneIsMineableDirtIsNot()
    {
        DefLoader.LoadAll();
        AssertBool(TerrainDefOf.Stone.Mineable).IsTrue();
        AssertBool(TerrainDefOf.Dirt.Mineable).IsFalse();
    }
}