using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class ThinkNodeSleepTest
{
    [TestCase]
    public void ReturnsSleepJobWhenTired()
    {
        var guy = new Guy();
        guy.Needs.Rest.Level = ThinkNode_Sleep.TiredThreshold - 0.1f;

        var job = new ThinkNode_Sleep().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Sleep).IsTrue();
    }

    [TestCase]
    public void ReturnsNullWhenRested()
    {
        var guy = new Guy();
        guy.Needs.Rest.Level = ThinkNode_Sleep.TiredThreshold + 0.1f;

        AssertObject(new ThinkNode_Sleep().TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void ThresholdIsExclusive()
    {
        var guy = new Guy();
        guy.Needs.Rest.Level = ThinkNode_Sleep.TiredThreshold;   // exactly at threshold = not tired

        AssertObject(new ThinkNode_Sleep().TryGiveJob(guy)).IsNull();
    }
}