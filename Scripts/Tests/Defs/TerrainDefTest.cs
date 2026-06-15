using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class TerrainDefTest
{
    [TestCase]
    public void StoneIsMineableDirtIsNot()
    {
        TerrainDefOf.Load();
        AssertBool(TerrainDefOf.Stone.Mineable).IsTrue();
        AssertBool(TerrainDefOf.Dirt.Mineable).IsFalse();
    }
}